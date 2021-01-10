using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

public class GameMaster : MonoBehaviour
{
    [Header("Neural network")]
    public NeuralNetworkHandler selectedNeuralNetwork;

    [Header("Training Settings")]
    public int trainingEpochs = 1;
    [Range(0, 60000)] public int trainingImages;
    [Range(0, 10000)] public int testImages;

    [Header("UI")]
    public Text accuracyText;
    public Text networkGuessText;

    public Slider[] numGuesses = new Slider[10];
    public Text[] numPercentages = new Text[10];

    public Button trainButton;
    public Button stopTrainingButton;

    public Slider testDataSlider;
    public InputField testDataInputField;
    public Text testDataLabelText;
    public Toggle showFailsToggle;

    [Header("Progress bar UI")]
    public Text progressbarInfo;
    public Text progressbarPercentage;
    public Slider progressbar;

    [Header("Progress bar settings")]
    public int updateRate = 100;

    [Header("Network Seed")]
    public bool useSeed;

    [HideInInspector] public static int seed;
    private System.Random rnd = new System.Random();

    private Drawing drawing;

    [HideInInspector] public List<int> wrongGuesses;
    [HideInInspector] public Coroutine TestingCoroutine = null;

    void Start()
    {
        if (!useSeed)
            seed = rnd.Next();

        drawing = (Drawing)FindObjectOfType(typeof(Drawing));
    }

    [ContextMenu("test")]
    public void testFunc()
    {
        drawing.DrawMyImage(ReadData.Get1DArrayFrom2DArray(AugmentData.ElasticEJ(ReadData.testData.Item1, 1, 2)));
    }

    public void StartTraining()
    {
        TrainingAndTestingPrep();
        selectedNeuralNetwork.StartTraining();
    }

    public void TrainingAndTestingPrep()
    {
        progressbar.gameObject.SetActive(true);
        trainButton.gameObject.SetActive(false);
        stopTrainingButton.gameObject.SetActive(true);

        if (showFailsToggle.isOn)
        {
            showFailsToggle.isOn = false;
            ShowFails();
        }

        showFailsToggle.gameObject.SetActive(false);
    }

    public void StartTestAccuracy()
    {
        TestingCoroutine = StartCoroutine(TestAccuracy());
    }

    private IEnumerator TestAccuracy()
    {
        TrainingAndTestingPrep();
        progressbarInfo.text = "Testing...";
        wrongGuesses = new List<int>();

        float accuracy = 0;

        for (int i = 0; i < testImages; i++)
        {
            float[] output = selectedNeuralNetwork.FeedForward(ReadData.GetArrayFrom2DArray(ReadData.testData.Item1, i));

            int numGuess = FromArrayToLabel(output);
            int correctNum = FromArrayToLabel(ReadData.GetArrayFrom2DArray(ReadData.testData.Item2, i));

            if (numGuess == correctNum)
                accuracy++;
            else
                wrongGuesses.Add(i);

            progressbar.value = (i / (float)testImages);
            progressbarPercentage.text = (progressbar.value * 100).ToString("0.00") + "%";

            if (i % updateRate == 0)
                yield return null;
        }

        accuracyText.text = "Accuracy: " + accuracy + " out of " + testImages + " (" + ((accuracy / testImages) * 100) + "%)";

        CancelTraining();
    }

    public int FromArrayToLabel(float[] listOfBits)
    {
        return Array.IndexOf(listOfBits, listOfBits.Max());
    }

    public void ReadDigit()
    {
        if (selectedNeuralNetwork == null || !selectedNeuralNetwork.IsNetworkCreated())
            return;

        List<float> pixels = new List<float>();

        foreach (Color color in Drawing.drawImage.GetPixels())
            pixels.Add(color.r);

        float[] prediction = selectedNeuralNetwork.FeedForward(pixels.ToArray());
        networkGuessText.text = "The networks guess is: " + FromArrayToLabel(prediction);

        for (int i = 0; i < prediction.Length; i++)
        {
            numPercentages[i].text = (prediction[i] * 100).ToString("0.") + "%";
            numGuesses[i].value = prediction[i];
        }
    }

    public void SaveData(string path)
    {
        selectedNeuralNetwork.SaveData(path);
    }

    public void LoadData(string path)
    {
        selectedNeuralNetwork.LoadData(path);
        StartTestAccuracy();
    }

    public void UpdateTestDataSlider()
    {
        if (testDataInputField.text != "")
        {
            int value = int.Parse(testDataInputField.text);

            if (value > 0)
            {
                testDataSlider.value = value;
                drawing.DrawTestData();
            }
        }
    }

    public void UpdateTestDataInputField()
    {
        testDataInputField.text = testDataSlider.value.ToString();
        drawing.DrawTestData();
    }

    public void ShowFails()
    {
        testDataSlider.value = 1;

        if (showFailsToggle.isOn)
            testDataSlider.maxValue = wrongGuesses.Count;
        else
            testDataSlider.maxValue = 10000;

        UpdateTestDataInputField();
        drawing.DrawTestData();
    }

    public void CancelTraining()
    {
        if (TestingCoroutine != null)
        {
            StopCoroutine(TestingCoroutine);
            TestingCoroutine = null;
        }

        selectedNeuralNetwork.CancelTraining();

        stopTrainingButton.gameObject.SetActive(false);
        progressbar.gameObject.SetActive(false);
        trainButton.gameObject.SetActive(true);
    }
}