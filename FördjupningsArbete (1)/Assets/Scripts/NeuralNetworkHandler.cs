using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NeuralNetworkHandler : MonoBehaviour
{
    public abstract void StartTraining();

    public abstract float[] FeedForward(float[] data);

    public abstract void SaveData(string path);

    public abstract void LoadData(string path);

    public abstract bool IsNetworkCreated();

    public abstract bool IsTraining();

    public abstract void CancelTraining();

    public abstract void EstablishTrainingData();
}