using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

[HideInInspector]
public enum AugmentationsTypes
{
    None = 0,
    ElasticDistortion = 1 << 1,
    AffineDistortion = 1 << 2,
    RandomErasing = 1 << 3,
    ArrayTranslation = 1 << 4,
    RotatePicture = 1 << 5,
    ScalePicture = 1 << 6,
    Everything = ~0
}

public class GameMaster : MonoBehaviour
{
    [Header("Neural network")]
    public NeuralNetworkHandler selectedNeuralNetwork;

    [Header("Training Settings")]
    public bool RepeatTillConvergence = false;
    public int ConvergenceMargin = 5;[Tooltip("Number of epokes to run after no improvement is made to ensure convergence")]

    public bool LogAccuracy = false;

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
    private float accuracy = 0;
    private float bestAccuracy = 0;
    private float epokesWithNoImprovement = 0;

    private List<float> accuracyLog = new List<float>();

    private Drawing drawing;

    [HideInInspector] public List<int> wrongGuesses;
    [HideInInspector] public Coroutine TestingCoroutine = null;

    [HideInInspector] public AugmentationsTypes SelectedAugmentations = AugmentationsTypes.None;

    [HideInInspector]
    public class AugmentationInfo
    {
        public AugmentationInfo()
        {
            foldout = false;
            imagesToProduce = 0;
            sourceImages = 0;
        }

        public bool foldout;
        public int imagesToProduce;
        public int sourceImages;
    }

    [HideInInspector] public AugmentationInfo elasticDistortionInfo = new AugmentationInfo();
    [HideInInspector] public float elasticDistortionAlpha = 8;

    [HideInInspector] public AugmentationInfo affineDistortionInfo = new AugmentationInfo();
    [HideInInspector] public float affineDistortionHorizontalSkew;
    [HideInInspector] public float affineDistortionVerticalSkew;

    [HideInInspector] public AugmentationInfo randomErasingInfo = new AugmentationInfo();
    [HideInInspector] public int randomErasingOdds;

    [HideInInspector] public AugmentationInfo arrayTranslationInfo = new AugmentationInfo();
    [HideInInspector] public int arrayTranslationMinRange;
    [HideInInspector] public int arrayTranslationMaxRange;

    [HideInInspector] public AugmentationInfo rotatePictureInfo = new AugmentationInfo();
    [HideInInspector] public int rotatePictureAngelMin;
    [HideInInspector] public int rotatePictureAngelMax;

    [HideInInspector] public AugmentationInfo scalePictureInfo = new AugmentationInfo();
    [HideInInspector] public int scalePictureDimensionMin;
    [HideInInspector] public int scalePictureDimensionMax;

    void Start()
    {
        if (!useSeed)
            seed = rnd.Next();

        drawing = (Drawing)FindObjectOfType(typeof(Drawing));
    }

    [ContextMenu("Test Data augmentation")]
    public void TestFunc()
    {
        float[,] maneyPictures = AugmentData.AffineDistortionSkew(ReadData.testData.Item1,12,-0.5f,0);
        float[] maneyPicturesOnePicutre = ReadData.GetArrayFrom2DArray(maneyPictures, 10);

        drawing.DrawMyImage(maneyPicturesOnePicutre);
    }

    public void NewTrainingStart()
    {
        CreateAugmentedData();
        selectedNeuralNetwork.EstablishTrainingData();
        StartTraining();
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

        float rightAnswers = 0;

        for (int i = 0; i < testImages; i++)
        {
            float[] output = selectedNeuralNetwork.FeedForward(ReadData.GetArrayFrom2DArray(ReadData.testData.Item1, i));

            int numGuess = FromArrayToLabel(output);
            int correctNum = FromArrayToLabel(ReadData.GetArrayFrom2DArray(ReadData.testData.Item2, i));

            if (numGuess == correctNum)
                rightAnswers++;
            else
                wrongGuesses.Add(i);

            progressbar.value = (i / (float)testImages);
            progressbarPercentage.text = (progressbar.value * 100).ToString("0.00") + "%";

            if (i % updateRate == 0)
                yield return null;
        }

        accuracy = (rightAnswers / testImages);
        accuracyText.text = "Accuracy: " + rightAnswers + " out of " + testImages + " (" + ((rightAnswers / testImages) * 100) + "%)";

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

    public void ResetEpokesWithNoImprovement()
    {
        epokesWithNoImprovement = 0;
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
        showFailsToggle.gameObject.SetActive(true);
        progressbar.gameObject.SetActive(false);
        trainButton.gameObject.SetActive(true);

        if (RepeatTillConvergence)
        {
            if (LogAccuracy)
                accuracyLog.Add(accuracy * 100);

            if (accuracy > bestAccuracy)
            {
                bestAccuracy = accuracy;
                epokesWithNoImprovement = 0;
            }
            else
            {
                if (epokesWithNoImprovement >= ConvergenceMargin)
                {
                    if (LogAccuracy)
                        CreateAccuracyLog();

                    return;
                }

                epokesWithNoImprovement++;
            }

            StartTraining();
        }
    }

    public void CreateAccuracyLog()
    {
        string log = string.Format(
            "Best Accuracy: " + (bestAccuracy * 100) + "%" +
            "{0}{0}Seed: " + seed +
            "{0}{0}Training Images: " + trainingImages +
            "{0}Testing Images: " + testImages +
            "{0}{0}", Environment.NewLine);

        // Elastic Distortion
        if (IsAugmentationSelected(AugmentationsTypes.ElasticDistortion))
        {
            log += string.Format(
                "Elastic distortion settings: " +
                "{0}Produced Images: " + elasticDistortionInfo.imagesToProduce +
                "{0}Source Images: " + elasticDistortionInfo.sourceImages +
                "{0}Alpha: " + elasticDistortionAlpha +
                "{0}{0}", Environment.NewLine);
        }

        // Affine Distortion
        if (IsAugmentationSelected(AugmentationsTypes.AffineDistortion))
        {
            log += string.Format(
                "Affine distortion settings: " +
                "{0}Produced Images: " + affineDistortionInfo.imagesToProduce +
                "{0}Source Images: " + affineDistortionInfo.sourceImages +
                "{0}Horizontal Skew: " + affineDistortionHorizontalSkew +
                "{0}Vertical Skew: " + affineDistortionVerticalSkew +
                "{0}{0}", Environment.NewLine);
        }

        // Random Erasing
        if (IsAugmentationSelected(AugmentationsTypes.RandomErasing))
        {
            log += string.Format(
                "Random Erasing settings: " +
                "{0}Produced Images: " + randomErasingInfo.imagesToProduce +
                "{0}Source Images: " + randomErasingInfo.sourceImages +
                "{0}Odds: " + randomErasingOdds +
                "{0}{0}", Environment.NewLine);
        }

        // Array Translation
        if (IsAugmentationSelected(AugmentationsTypes.ArrayTranslation))
        {
            log += string.Format(
                "Array Translation settings: " +
                "{0}Produced Images: " + arrayTranslationInfo.imagesToProduce +
                "{0}Source Images: " + arrayTranslationInfo.sourceImages +
                "{0}Min Range: " + arrayTranslationMinRange +
                "{0}Max Range: " + arrayTranslationMaxRange +
                "{0}{0}", Environment.NewLine);
        }

        // Rotate Picture
        if (IsAugmentationSelected(AugmentationsTypes.RotatePicture))
        {
            log += string.Format(
                "Rotate Picture settings: " +
                "{0}Produced Images: " + rotatePictureInfo.imagesToProduce +
                "{0}Source Images: " + rotatePictureInfo.sourceImages +
                "{0}Angel Min: " + rotatePictureAngelMin +
                "{0}Angel Max: " + rotatePictureAngelMax +
                "{0}{0}", Environment.NewLine);
        }

        // Scale Picture
        if (IsAugmentationSelected(AugmentationsTypes.ScalePicture))
        {
            log += string.Format(
                "Scale Picture settings: " +
                "{0}Produced Images: " + scalePictureInfo.imagesToProduce +
                "{0}Source Images: " + scalePictureInfo.sourceImages +
                "{0}Dimension Min: " + scalePictureDimensionMin +
                "{0}Dimension Max: " + scalePictureDimensionMax +
                "{0}{0}", Environment.NewLine);
        }

        log += string.Format("Epokes Ran: " + accuracyLog.Count() + "{0}", Environment.NewLine);

        for (int i = 0; i < accuracyLog.Count(); i++)
        {
            log += string.Format("{0}Epok " + (i + 1) + " Accuracy: " + accuracyLog[i] + "%", Environment.NewLine);
        }

        string path;

        int logNum = 1;

        while (true)
        {
            path = "Assets/Logs/Log " + logNum + ".txt";

            if (!File.Exists(path))
                break;

            logNum++;
        }

        File.WriteAllText(path, log);
        UnityEditor.AssetDatabase.Refresh();
    }

    public void CreateAugmentedData()
    {
        ReadData.ElasticDistortionData = null;
        ReadData.AffineDistortionData = null;
        ReadData.RandomErasingData = null;
        ReadData.ArrayTranslationData = null;
        ReadData.RotatePictureData = null;
        ReadData.ScalePictureData = null;

        // Elastic Distortion
        if (IsAugmentationSelected(AugmentationsTypes.ElasticDistortion))
        {
            float[,] augmentedData = AugmentData.ElasticDistortion(ReadData.GetPartial2DArray(ReadData.trainingData.Item1, elasticDistortionInfo.sourceImages), elasticDistortionInfo.imagesToProduce, elasticDistortionAlpha);
            float[,] labels = new float[elasticDistortionInfo.imagesToProduce, 10];

            for (int i = 0; i < elasticDistortionInfo.imagesToProduce; i++)
                for (int j = 0; j < 10; j++)
                    labels[i, j] = ReadData.trainingData.Item2[i % elasticDistortionInfo.sourceImages, j];

            ReadData.ElasticDistortionData = Tuple.Create(augmentedData, labels);
        }

        // Affine Distortion
        if (IsAugmentationSelected(AugmentationsTypes.AffineDistortion))
        {
            float[,] augmentedData = AugmentData.AffineDistortionSkew(ReadData.GetPartial2DArray(ReadData.trainingData.Item1, affineDistortionInfo.sourceImages), affineDistortionInfo.imagesToProduce, affineDistortionHorizontalSkew, affineDistortionVerticalSkew);
            float[,] labels = new float[affineDistortionInfo.imagesToProduce, 10];

            for (int i = 0; i < affineDistortionInfo.imagesToProduce; i++)
                for (int j = 0; j < 10; j++)
                    labels[i, j] = ReadData.trainingData.Item2[i % affineDistortionInfo.sourceImages, j];

            ReadData.AffineDistortionData = Tuple.Create(augmentedData, labels);
        }

        // Random Erasing
        if (IsAugmentationSelected(AugmentationsTypes.RandomErasing))
        {
            float[,] augmentedData = AugmentData.RandomErasing(ReadData.GetPartial2DArray(ReadData.trainingData.Item1, randomErasingInfo.sourceImages), randomErasingInfo.imagesToProduce, randomErasingOdds);
            float[,] labels = new float[randomErasingInfo.imagesToProduce, 10];

            for (int i = 0; i < randomErasingInfo.imagesToProduce; i++)
                for (int j = 0; j < 10; j++)
                    labels[i, j] = ReadData.trainingData.Item2[i % randomErasingInfo.sourceImages, j];

            ReadData.RandomErasingData = Tuple.Create(augmentedData, labels);
        }

        // Array Translation
        if (IsAugmentationSelected(AugmentationsTypes.ArrayTranslation))
        {
            float[,] augmentedData = AugmentData.ArrayTranslation(ReadData.GetPartial2DArray(ReadData.trainingData.Item1, arrayTranslationInfo.sourceImages), arrayTranslationInfo.imagesToProduce, arrayTranslationMinRange, arrayTranslationMaxRange);
            float[,] labels = new float[arrayTranslationInfo.imagesToProduce, 10];

            for (int i = 0; i < arrayTranslationInfo.imagesToProduce; i++)
                for (int j = 0; j < 10; j++)
                    labels[i, j] = ReadData.trainingData.Item2[i % arrayTranslationInfo.sourceImages, j];

            ReadData.ArrayTranslationData = Tuple.Create(augmentedData, labels);
        }

        // Rotate Picture
        if (IsAugmentationSelected(AugmentationsTypes.RotatePicture))
        {
            float[,] augmentedData = AugmentData.RotatePicture(ReadData.GetPartial2DArray(ReadData.trainingData.Item1, rotatePictureInfo.sourceImages), rotatePictureInfo.imagesToProduce, rotatePictureAngelMin, rotatePictureAngelMax);
            float[,] labels = new float[rotatePictureInfo.imagesToProduce, 10];

            for (int i = 0; i < rotatePictureInfo.imagesToProduce; i++)
                for (int j = 0; j < 10; j++)
                    labels[i, j] = ReadData.trainingData.Item2[i % rotatePictureInfo.sourceImages, j];

            ReadData.RotatePictureData = Tuple.Create(augmentedData, labels);
        }

        // Scale Picture
        if (IsAugmentationSelected(AugmentationsTypes.ScalePicture))
        {
            float[,] augmentedData = AugmentData.ScalePicture(ReadData.GetPartial2DArray(ReadData.trainingData.Item1, scalePictureInfo.sourceImages), scalePictureInfo.imagesToProduce, scalePictureDimensionMin, scalePictureDimensionMax);
            float[,] labels = new float[scalePictureInfo.imagesToProduce, 10];

            for (int i = 0; i < scalePictureInfo.imagesToProduce; i++)
                for (int j = 0; j < 10; j++)
                    labels[i, j] = ReadData.trainingData.Item2[i % scalePictureInfo.sourceImages, j];

            ReadData.ScalePictureData = Tuple.Create(augmentedData, labels);
        }
    }

    public bool IsAugmentationSelected(AugmentationsTypes type)
    {
        return (SelectedAugmentations & type) == type;
    }
}