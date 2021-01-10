using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System;

public static class BigEndianUtils
{
    public static int ReadBigInt32(this BinaryReader br)
    {
        byte[] bytes = br.ReadBytes(sizeof(Int32));
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
}

public class ReadData : MonoBehaviour
{
    public static Tuple<float[,], float[,]> trainingData;
    public static Tuple<float[,], float[,]> testData;

    public static Tuple<float[,], float[,]> ElasticDistortionData;
    public static Tuple<float[,], float[,]> AffineDistortionData;
    public static Tuple<float[,], float[,]> RandomErasingData;
    public static Tuple<float[,], float[,]> ArrayTranslationData;
    public static Tuple<float[,], float[,]> RotatePictureData;
    public static Tuple<float[,], float[,]> ScalePictureData;

    private readonly string trainingLabelsPath = "Assets/TrainingData/training-labels.idx1-ubyte";
    private readonly string trainingImagesPath = "Assets/TrainingData/training-images.idx3-ubyte";

    private readonly string testLabelsPath = "Assets/TrainingData/test-labels.idx1-ubyte";
    private readonly string testImagesPath = "Assets/TrainingData/test-images.idx3-ubyte";

    private void Awake()
    {
        trainingData = ReadMNIST(trainingLabelsPath, trainingImagesPath);
        testData = ReadMNIST(testLabelsPath, testImagesPath);
    }

    private Tuple<float[,], float[,]> ReadMNIST(string labelPath, string imagePath)
    {
        FileStream fsLabels = new FileStream(labelPath, FileMode.Open);
        FileStream fsImages = new FileStream(imagePath, FileMode.Open);

        BinaryReader brLabels = new BinaryReader(fsLabels);
        BinaryReader brImages = new BinaryReader(fsImages);

        int magic2 = brLabels.ReadInt32();
        int numLabels = BigEndianUtils.ReadBigInt32(brLabels);

        int magic1 = brImages.ReadInt32();
        int numImages = BigEndianUtils.ReadBigInt32(brImages);
        int numRows = BigEndianUtils.ReadBigInt32(brImages);
        int numCols = BigEndianUtils.ReadBigInt32(brImages);

        float[,] pixelArray = new float[numImages, numRows * numCols];
        float[,] labels = new float[numLabels, 10];

        byte[][] pixels = new byte[28][];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new byte[28];

        for (int di = 0; di < numImages; di++)
        {
            for (int i = 27; i >= 0; i--)
            {
                for (int j = 0; j < 28; j++)
                {
                    byte b = brImages.ReadByte();
                    pixels[j][i] = b;
                }
            }

            labels[di, brLabels.ReadByte()] = 1;

            for (int i = 0; i < 28; i++)
                for (int j = 0; j < 28; j++)
                    pixelArray[di, (j * 28) + i] = pixels[i][j] / 255f;
        }

        fsImages.Close();
        brImages.Close();
        fsLabels.Close();
        brLabels.Close();

        return Tuple.Create(pixelArray, labels);
    }

    public static float[] GetArrayFrom2DArray(float[,] input, int index)
    {
        int SecondDimLenght = input.GetLength(1);
        float[] output = new float[SecondDimLenght];

        Buffer.BlockCopy(input, index * 4 * SecondDimLenght, output, 0, 4 * SecondDimLenght);
        return output;
    }

    public static float[,] GetPartial2DArray(float[,] input, int lenght)
    {
        float[,] output = new float[lenght, input.GetLength(1)];
        Buffer.BlockCopy(input, 0, output, 0, 4 * input.GetLength(1) * lenght);

        return output;
    }

    public static float[,] Get2DArrayFrom1DArray(float[] input, int size)
    {
        float[,] output = new float[size, size];
        Buffer.BlockCopy(input, 0, output, 0, input.Length * 4);

        return output;
    }

    public static float[] Get1DArrayFrom2DArray(float[,] input)
    {
        float[] output = new float[input.GetLength(0) * input.GetLength(1)];
        Buffer.BlockCopy(input, 0, output, 0, input.GetLength(0) * input.GetLength(1) * 4);

        return output;
    }

    public static float[,] CombineTwo2DArrays(float[,] input1, float[,] input2)
    {
        if (input1 == null)
            return input2;

        if (input2 == null)
            return input1;

        float[,] output = new float[(input1.GetLength(0) + input2.GetLength(0)), input1.GetLength(1)];
        Buffer.BlockCopy(input1, 0, output, 0, input1.GetLength(0) * input1.GetLength(1) * 4);
        Buffer.BlockCopy(input2, 0, output, input1.GetLength(0) * input1.GetLength(1) * 4, input2.GetLength(0) * input2.GetLength(1) * 4);

        return output;
    }

    public static float[,] Shuffle(float[,] array)
    {
        UnityEngine.Random.InitState(GameMaster.seed);

        List<int> indexes = new List<int>();
        float[,] output = new float[array.GetLength(0), array.GetLength(1)];

        for (int i = 0; i < array.GetLength(0); i++)
            indexes.Add(i);

        int index = 0;

        while (indexes.Count > 0)
        {
            int k = indexes[UnityEngine.Random.Range(0, indexes.Count)];

            for (int i = 0; i < array.GetLength(1); i++)
                output[index, i] = array[k, i];

            indexes.RemoveAt(indexes.IndexOf(k));
            index++;
        }

        return output;
    }
}