using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NeuralNetwork
{
    /// <summary>
    /// Math
    ///     // Feed layer create 
    ///  MSE = mean squared error
    /// error cost expresion = MSE = ((output1 - y1)^2) / 2 + ((output2 -y2)^2) /2
    ///                      derivatan av MSE ger Viktförändringen vi vill göra. aka Weigth[x][y][z] prime
    ///              der[mse] -> der  (((output1 - y1)^2) / 2 / Weigth) -> (output1 - y1) * (der(output1) / der(wikt))
    ///                          forts: vi använder oss av sigmoid operation på output så vi behöver derivatan av sigmoid
    ///                          -> (output1 - y1) * derSigmoid(Output1) / Weigth
    ///          Der av sigmoid =  value * (1 - value);
    ///                der(mse) = (output1 - y1) * ( output1 * ( 1 - output1)) * (der(output1) / weigth)
    ///                (output1 - y1) * derSigmoid(output1) * neuronen som är bunden till vikten före = vikten prime (x1)
    ///                                                          t.ex. se nedan, för vikten w1, så blir det  x1
    ///                                                             
    ///                                                      X1  W1   X11
    ///                                                          W2
    ///                                                      X2     
    ///                     (output1 - y1) * derSigmoid(output1)   används mycket, spara värdet i en array t.ex. neuronShort[]
    ///                         då får man
    ///                      Weigth[index][neuronindex] = neuronShort[neuronindex] * bindadeNeuron[index]
    /// </summary>

    //{ 18 , 9 , 2 } => 18 input, 9 middle 2 output.

    private int[] layer;

    private float[][] neurons;
    private float[][] error;
    private float[][] connectionCalc;

    private float[][][] weights;

    private float learningRate;
    private float bias;

    private float noiseRawNeg;
    private float noiseRawPos;

    private float noisePercNeg;
    private float noisePercPos;

    public NeuralNetwork
        (
        int[] layer,
        float learningRate,
        float bias,
        float noiseRawNeg,
        float noiseRawPos,
        float noisePercNeg,
        float noisePercPos
        )
    {
        this.layer = new int[layer.Length];
        this.learningRate = learningRate;
        this.bias = bias;
        this.noiseRawNeg = noiseRawNeg;
        this.noiseRawPos = noiseRawPos;
        this.noisePercNeg = noisePercNeg;
        this.noisePercPos = noisePercPos;

        for (int i = 0; i < layer.Length; i++)
        {
            this.layer[i] = layer[i];
        }
        Initialize_2D_Arrays();
        InitializeWeights();
    }

    // Generate neuron array
    private void Initialize_2D_Arrays()
    {
        List<float[]> neuronList = new List<float[]>();
        List<float[]> errorList = new List<float[]>();
        List<float[]> conList = new List<float[]>();

        for (int i = 0; i < layer.Length; i++)
        {
            neuronList.Add(new float[layer[i]]);
            errorList.Add(new float[layer[i]]);
            conList.Add(new float[layer[i]]);
        }

        neurons = neuronList.ToArray();
        error = errorList.ToArray();
        connectionCalc = conList.ToArray();
    }

    // Generate Weigth array, aka generate "1 or - 1"
    private void InitializeWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();

        // Weigth for each layer that has a weigth aka all hidden layers, all layers but input layer
        for (int i = 1; i < layer.Length; i++)
        {
            List<float[]> layerWeightList = new List<float[]>();

            int neuronsPreviousLayer = layer[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsPreviousLayer];

                for (int k = 0; k < neuronsPreviousLayer; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-1f, 1f);
                }
                // 1D
                layerWeightList.Add(neuronWeights);
            }
            // 2D
            weightsList.Add(layerWeightList.ToArray());
        }
        // 3D
        weights = weightsList.ToArray();

    }

    // Return output array
    public float[] FeedForward(float[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            // Put input into the input layer in "neurons", first array first number indicate input layer
            neurons[0][i] = input[i];
        }

        // all layers but input layer
        for (int i = 1; i < layer.Length; i++)
        {
            // all neurons in those layers
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float sum = 0f;
                sum += bias;

                // All the neurons in the previous layer
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    // weights[i - 1][j][k]
                    // [i - 1] starting at "1" so "need" to go back one step
                    // [j] current neuron, nothing wierd
                    // [k] current weigth bind

                    // weigth applied on neuron
                    sum += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                neurons[i][j] = Sigmoid(sum);
            }
        }
        // return last layer = output layer.
        return neurons[neurons.Length - 1];
    }

    public void BackPropStart(float[] correctOutput)
    {
        BackPropOutputLayer(correctOutput);
        if (layer.Length > 2)
        {
            BackPropMiddleLayer();
        }
    }

    public float Sigmoid(float value)
    {
        return 1.0f / (1.0f + (float)Math.Exp(-value));
    }

    public float SigDer(float value)
    {
        return value * (1 - value);
    }

    public void BackPropOutputLayer(float[] correctOutput)
    {
        int outPutLayer = layer.Length - 1;

        // all neurons in those layers (output)
        for (int j = 0; j < neurons[outPutLayer].Length; j++)
        {
            error[outPutLayer][j] = neurons[outPutLayer][j] - correctOutput[j];
            connectionCalc[outPutLayer][j] = error[outPutLayer][j] * SigDer(neurons[outPutLayer][j]);

            // All the neurons in the previous layer (input)
            for (int k = 0; k < neurons[outPutLayer - 1].Length; k++)
            {
                float perc = 1;
                float raw = 0;
                if (noisePercPos != 0 || noiseRawPos != 0)
                {
                    perc = UnityEngine.Random.Range(noisePercNeg, noisePercPos);
                    raw = UnityEngine.Random.Range(noiseRawNeg, noiseRawPos);
                }

                weights[outPutLayer - 1][j][k] -= (connectionCalc[outPutLayer][j] * neurons[outPutLayer - 1][k]) * learningRate * perc;
                weights[outPutLayer - 1][j][k] -= raw;
            }
        }
    }

    public void BackPropMiddleLayer()
    {
        for (int i = layer.Length - 2; i > 0; i--)
        {
            float[] calc = new float[neurons[i].Length];
            // ps: weigth is already -1 offseted kinda. aka the weigth for neuron [i][l] is in [i-1][l]

            // output
            for (int j = 0; j < neurons[i].Length; j++)
            {
                for (int k = 0; k < neurons[i + 1].Length; k++)
                {
                    calc[j] += connectionCalc[i + 1][k] * weights[i][k][j];
                }
                calc[j] *= SigDer(neurons[i][j]);
            }

            // output
            for (int j = 0; j < neurons[i].Length; j++)
            {
                // input
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    float perc = 1;
                    float raw = 0;
                    if (noisePercPos != 0 || noiseRawPos != 0)
                    {
                        perc = UnityEngine.Random.Range(noisePercNeg, noisePercPos);
                        raw = UnityEngine.Random.Range(noiseRawNeg, noiseRawPos);
                    }

                    weights[i - 1][j][k] -= (calc[j] * neurons[i - 1][k]) * learningRate * perc;
                    weights[i - 1][j][k] -= raw;
                }
            }
        }
    }
}
