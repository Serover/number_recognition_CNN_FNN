using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

using System.Text.RegularExpressions;

public class FNNHandler : NeuralNetworkHandler
{
    [Header("Noise injection")]
    public float noiseRawNeg;
    public float noiseRawPos;

    public float noisePercNeg;
    public float noisePercPos;

    [Header("backprop Settings")]
    public float bias;
    public float learningRate = 0.1f;
    public int[] hiddenLayers = new int[] { 32 };

    private FNN fnn;
    private GameMaster gameMaster;
    private Coroutine TrainingCoroutine = null;

    private int[] layers;

    private void Start()
    {
        gameMaster = (GameMaster)FindObjectOfType(typeof(GameMaster));
    }

    public override void StartTraining()
    {
        FixLayerArray();

        if (fnn == null)
            fnn = new FNN(layers, learningRate, bias, noiseRawNeg, noiseRawPos, noisePercNeg, noisePercPos);

        TrainingCoroutine = StartCoroutine(TrainFNN());
    }

    private void FixLayerArray()
    {
        List<int> temp = new List<int>();

        // input ( 28 x 28 pixels)
        temp.Add(784);

        for (int i = 0; i < hiddenLayers.Length; i++)
            temp.Add(hiddenLayers[i]);

        // output 9 digits + 0 = 10 :)
        temp.Add(10);

        layers = temp.ToArray();
    }

    private IEnumerator TrainFNN()
    {
        float trainingStartTime = Time.time;

        for (int i = 0; i < gameMaster.trainingEpochs; i++)
        {
            for (int j = 0; j < gameMaster.trainingImages; j++)
            {
                fnn.FeedForward(ReadData.GetArrayFrom2DArray(ReadData.trainingData.Item1, j));
                fnn.BackPropStart(ReadData.GetArrayFrom2DArray(ReadData.trainingData.Item2, j));

                gameMaster.progressbar.value = j / (float)gameMaster.trainingImages;
                gameMaster.progressbarPercentage.text = (gameMaster.progressbar.value * 100).ToString("0.00") + "%";

                if (j % gameMaster.updateRate == 0)
                {
                    gameMaster.ReadDigit();

                    float v = gameMaster.progressbar.value;

                    if (v > 0)
                    {
                        float timeInTraining = Time.time - trainingStartTime;
                        float timeEstimation = (timeInTraining * (-v + gameMaster.trainingEpochs - i)) / (v + i);

                        TimeSpan ts = TimeSpan.FromSeconds(timeEstimation);

                        string[] parts = string.Format("{0:D1}d:{1:D1}h:{2:D1}m:{3:D1}s", ts.Days, ts.Hours, ts.Minutes, ts.Seconds).Split(':').SkipWhile(s => Regex.Match(s, @"^0\w").Success).ToArray();
                        string result = string.Join(" ", parts);

                        gameMaster.progressbarInfo.text = "Training (Epoch " + (i + 1) + "/" + gameMaster.trainingEpochs + ") " + "Total ETA: " + result;
                    }
                    else
                        gameMaster.progressbarInfo.text = "Training (Epoch " + (i + 1) + "/" + gameMaster.trainingEpochs + ")";

                    yield return null;
                }
            }
        }

        TrainingCoroutine = null;
        gameMaster.StartTestAccuracy();
    }

    public override void SaveData(string path)
    {
        fnn.SaveState(path);
    }

    public override void LoadData(string path)
    {
        fnn.LoadStoredData(path);
    }

    public override float[] FeedForward(float[] data)
    {
        return fnn.FeedForward(data);
    }

    public override bool IsNetworkCreated()
    {
        return fnn == null ? false : true;
    }

    public override bool IsTraining()
    {
        return TrainingCoroutine == null ? false : true;
    }

    public override void CancelTraining()
    {
        StopCoroutine(TrainingCoroutine);
    }

    public override void EstablishTrainingData()
    {
        Debug.Log("EstablishTrainingData is not implemented for FNN");
    }
}