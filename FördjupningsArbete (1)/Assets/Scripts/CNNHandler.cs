using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

using System.IO;
using NeuralNetworkNET.APIs;
using System.Threading.Tasks;
using NeuralNetworkNET.APIs.Enums;
using NeuralNetworkNET.APIs.Results;
using NeuralNetworkNET.APIs.Structs;
using NeuralNetworkNET.APIs.Settings;
using NeuralNetworkNET.APIs.Datasets;
using NeuralNetworkNET.APIs.Interfaces;
using SixLabors.ImageSharp.PixelFormats;
using NeuralNetworkNET.APIs.Interfaces.Data;
using NeuralNetworkNET.SupervisedLearning.Progress;
using System.Threading;

using System.Text.RegularExpressions;

public class CNNHandler : NeuralNetworkHandler
{
    [Header("Network settings")]
    [Range(1, 1000)] public int batchSize = 100; // Numbers larger then 1.000 may work, but 60.000 will for sure run out of memory.

    private INeuralNetwork cnn;
    private GameMaster gameMaster;

    private int currentEpoch = 1;

    private CancellationTokenSource cts;
    private float trainingStartTime;
    private bool firstTrainingRun = false;

    private float[,] trainingImageData;
    private float[,] trainingLabelData;

    private void Start()
    {
        gameMaster = (GameMaster)FindObjectOfType(typeof(GameMaster));
    }

    public override void StartTraining()
    {
        if (cnn == null)
        {
            cnn = NetworkManager.NewSequential(TensorInfo.Image<Alpha8>(28, 28),
                CuDnnNetworkLayers.Convolutional((5, 5), 20, ActivationType.Identity),
                CuDnnNetworkLayers.Pooling(ActivationType.LeakyReLU),
                CuDnnNetworkLayers.FullyConnected(100, ActivationType.LeCunTanh),
                CuDnnNetworkLayers.Softmax(10));
        }

        TrainCNN();
    }

    public override float[] FeedForward(float[] data)
    {
        return cnn.Forward(data);
    }

    public async void TrainCNN()
    {
        currentEpoch = 1;
        firstTrainingRun = true;
        gameMaster.progressbarInfo.text = "Training (Epoch " + currentEpoch + "/" + gameMaster.trainingEpochs + ")";
        gameMaster.progressbar.value = 0;
        gameMaster.progressbarPercentage.text = (gameMaster.progressbar.value * 100).ToString("0.00") + "%";

        cts = new CancellationTokenSource();

        // TODO: only call the stablishdata functions when training start button is presed
        ITrainingDataset trainingData = DatasetLoader.Training((trainingImageData, trainingLabelData), batchSize);
        TrainingSessionResult result = await NetworkManager.TrainNetworkAsync(
        cnn,                                    // The network instance to train
        trainingData,                           // The ITrainingDataset instance   
        TrainingAlgorithms.AdaDelta(),          // The training algorithm to use
        gameMaster.trainingEpochs,              // The expected number of training epochs to run
        0.5f,                                   // Dropout probabilit);
        BatchProgress,
        token: cts.Token);

        if (!cts.Token.IsCancellationRequested)
            gameMaster.StartTestAccuracy();

        cts = null;
    }

    public void BatchProgress(BatchProgress progress)
    {
        if (cts.Token.IsCancellationRequested)
            return;

        if (firstTrainingRun)
        {
            firstTrainingRun = false;
            trainingStartTime = Time.time;
        }

        if (progress.ProcessedItems == gameMaster.trainingImages && gameMaster.trainingEpochs > currentEpoch)
            currentEpoch++;

        gameMaster.progressbar.value = progress.Percentage / 100;
        gameMaster.progressbarPercentage.text = (gameMaster.progressbar.value * 100).ToString("0.00") + "%";

        float timeInTraining = Time.time - trainingStartTime;

        float v = gameMaster.progressbar.value;
        float timeEstimation = (timeInTraining * (-v + 1 + gameMaster.trainingEpochs - currentEpoch)) / (v + currentEpoch - 1);

        TimeSpan ts = TimeSpan.FromSeconds(timeEstimation);

        string[] parts = string.Format("{0:D1}d:{1:D1}h:{2:D1}m:{3:D1}s", ts.Days, ts.Hours, ts.Minutes, ts.Seconds).Split(':').SkipWhile(s => Regex.Match(s, @"^0\w").Success).ToArray();
        string result = string.Join(" ", parts);

        gameMaster.progressbarInfo.text = "Training (Epoch " + currentEpoch + "/" + gameMaster.trainingEpochs + ") " + "Total ETA: " + result;
    }

    public override void SaveData(string path)
    {
        cnn.Save(new FileInfo(path));
    }

    public override void LoadData(string path)
    {
        FileInfo file = new FileInfo(path);
        cnn = NetworkLoader.TryLoad(file, ExecutionModePreference.Cuda);
    }

    private void OnApplicationQuit()
    {
        CancelTraining();
    }

    public override bool IsNetworkCreated()
    {
        return cnn == null ? false : true;
    }

    public override bool IsTraining()
    {
        return cts == null ? false : true;
    }

    public override void CancelTraining()
    {
        if (cts != null)
            cts.Cancel();
    }

    public override void EstablishTrainingData()
    {
        trainingImageData = ReadData.GetPartial2DArray(ReadData.trainingData.Item1, gameMaster.trainingImages);
        if (ReadData.ElasticDistortionData != null) trainingImageData = ReadData.CombineTwo2DArrays(trainingImageData, ReadData.ElasticDistortionData.Item1);
        if (ReadData.AffineDistortionData != null) trainingImageData = ReadData.CombineTwo2DArrays(trainingImageData, ReadData.AffineDistortionData.Item1);
        if (ReadData.RandomErasingData != null) trainingImageData = ReadData.CombineTwo2DArrays(trainingImageData, ReadData.RandomErasingData.Item1);
        if (ReadData.ArrayTranslationData != null) trainingImageData = ReadData.CombineTwo2DArrays(trainingImageData, ReadData.ArrayTranslationData.Item1);
        if (ReadData.RotatePictureData != null) trainingImageData = ReadData.CombineTwo2DArrays(trainingImageData, ReadData.RotatePictureData.Item1);
        if (ReadData.ScalePictureData != null) trainingImageData = ReadData.CombineTwo2DArrays(trainingImageData, ReadData.ScalePictureData.Item1);
        trainingImageData = ReadData.Shuffle(trainingImageData);

        trainingLabelData = ReadData.GetPartial2DArray(ReadData.trainingData.Item2, gameMaster.trainingImages);
        if (ReadData.ElasticDistortionData != null) trainingLabelData = ReadData.CombineTwo2DArrays(trainingLabelData, ReadData.ElasticDistortionData.Item2);
        if (ReadData.AffineDistortionData != null) trainingLabelData = ReadData.CombineTwo2DArrays(trainingLabelData, ReadData.AffineDistortionData.Item2);
        if (ReadData.RandomErasingData != null) trainingLabelData = ReadData.CombineTwo2DArrays(trainingLabelData, ReadData.RandomErasingData.Item2);
        if (ReadData.ArrayTranslationData != null) trainingLabelData = ReadData.CombineTwo2DArrays(trainingLabelData, ReadData.ArrayTranslationData.Item2);
        if (ReadData.RotatePictureData != null) trainingLabelData = ReadData.CombineTwo2DArrays(trainingLabelData, ReadData.RotatePictureData.Item2);
        if (ReadData.ScalePictureData != null) trainingLabelData = ReadData.CombineTwo2DArrays(trainingLabelData, ReadData.ScalePictureData.Item2);
        trainingLabelData = ReadData.Shuffle(trainingLabelData);
    }
}