using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameMaster))]
public class GameMasterEditor : Editor
{
    private string saveDirectory = "Assets/Network saves";
    private string loadDirectory = "Assets/Network saves";

    public override void OnInspectorGUI()
    {
        GameMaster gameMaster = (GameMaster)target;

        DrawDefaultInspector();

        if (gameMaster.useSeed)
            GameMaster.seed = EditorGUILayout.IntField(GameMaster.seed);

        EditorGUILayout.Space();

        gameMaster.SelectedAugmentations = (AugmentationsTypes)EditorGUILayout.EnumFlagsField(gameMaster.SelectedAugmentations);

        // Elastic Distortion
        if (gameMaster.IsAugmentationSelected(AugmentationsTypes.ElasticDistortion))
        {
            gameMaster.elasticDistortionInfo.foldout = EditorGUILayout.Foldout(gameMaster.elasticDistortionInfo.foldout, "Elastic Distortion");

            if (gameMaster.elasticDistortionInfo.foldout)
            {
                gameMaster.elasticDistortionInfo.sourceImages = EditorGUILayout.IntSlider("Source Images", gameMaster.elasticDistortionInfo.sourceImages, 0, 60000);
                gameMaster.elasticDistortionInfo.imagesToProduce = EditorGUILayout.IntField("Images To Produce", gameMaster.elasticDistortionInfo.imagesToProduce);
                gameMaster.elasticDistortionAlpha = EditorGUILayout.FloatField("Alpha", gameMaster.elasticDistortionAlpha);
            }
        }

        // Affine Distortion
        if (gameMaster.IsAugmentationSelected(AugmentationsTypes.AffineDistortion))
        {
            gameMaster.affineDistortionInfo.foldout = EditorGUILayout.Foldout(gameMaster.affineDistortionInfo.foldout, "Affine Distortion");

            if (gameMaster.affineDistortionInfo.foldout)
            {
                gameMaster.affineDistortionInfo.sourceImages = EditorGUILayout.IntSlider("Source Images", gameMaster.affineDistortionInfo.sourceImages, 0, 60000);
                gameMaster.affineDistortionInfo.imagesToProduce = EditorGUILayout.IntField("Images To Produce", gameMaster.affineDistortionInfo.imagesToProduce);
                gameMaster.affineDistortionVerticalSkew = EditorGUILayout.FloatField("Vertical skew", gameMaster.affineDistortionVerticalSkew);
                gameMaster.affineDistortionHorizontalSkew = EditorGUILayout.FloatField("Horizontal skew", gameMaster.affineDistortionHorizontalSkew);
            }
        }

        // Random Erasing
        if (gameMaster.IsAugmentationSelected(AugmentationsTypes.RandomErasing))
        {
            gameMaster.randomErasingInfo.foldout = EditorGUILayout.Foldout(gameMaster.randomErasingInfo.foldout, "Random Erasing");

            if (gameMaster.randomErasingInfo.foldout)
            {
                gameMaster.randomErasingInfo.sourceImages = EditorGUILayout.IntSlider("Source Images", gameMaster.randomErasingInfo.sourceImages, 0, 60000);
                gameMaster.randomErasingInfo.imagesToProduce = EditorGUILayout.IntField("Images To Produce", gameMaster.randomErasingInfo.imagesToProduce);
                gameMaster.randomErasingOdds = EditorGUILayout.IntSlider("Odds", gameMaster.randomErasingOdds, 0, 100);
            }
        }

        // Array Translation
        if (gameMaster.IsAugmentationSelected(AugmentationsTypes.ArrayTranslation))
        {
            gameMaster.arrayTranslationInfo.foldout = EditorGUILayout.Foldout(gameMaster.arrayTranslationInfo.foldout, "Array Translation");

            if (gameMaster.arrayTranslationInfo.foldout)
            {
                gameMaster.arrayTranslationInfo.sourceImages = EditorGUILayout.IntSlider("Source Images", gameMaster.arrayTranslationInfo.sourceImages, 0, 60000);
                gameMaster.arrayTranslationInfo.imagesToProduce = EditorGUILayout.IntField("Images To Produce", gameMaster.arrayTranslationInfo.imagesToProduce);
                gameMaster.arrayTranslationMinRange = EditorGUILayout.IntField("Min Range", gameMaster.arrayTranslationMinRange);
                gameMaster.arrayTranslationMaxRange = EditorGUILayout.IntField("Max Range", gameMaster.arrayTranslationMaxRange);
            }
        }

        // Rotate Picture
        if (gameMaster.IsAugmentationSelected(AugmentationsTypes.RotatePicture))
        {
            gameMaster.rotatePictureInfo.foldout = EditorGUILayout.Foldout(gameMaster.rotatePictureInfo.foldout, "Rotate Picture");

            if (gameMaster.rotatePictureInfo.foldout)
            {
                gameMaster.rotatePictureInfo.sourceImages = EditorGUILayout.IntSlider("Source Images", gameMaster.rotatePictureInfo.sourceImages, 0, 60000);
                gameMaster.rotatePictureInfo.imagesToProduce = EditorGUILayout.IntField("Images To Produce", gameMaster.rotatePictureInfo.imagesToProduce);
                gameMaster.rotatePictureAngelMin = EditorGUILayout.IntField("Angle Min", gameMaster.rotatePictureAngelMin);
                gameMaster.rotatePictureAngelMax = EditorGUILayout.IntField("Angle Max", gameMaster.rotatePictureAngelMax);
            }
        }

        // Scale Picture
        if (gameMaster.IsAugmentationSelected(AugmentationsTypes.ScalePicture))
        {
            gameMaster.scalePictureInfo.foldout = EditorGUILayout.Foldout(gameMaster.scalePictureInfo.foldout, "Scale Picture");

            if (gameMaster.scalePictureInfo.foldout)
            {
                gameMaster.scalePictureInfo.sourceImages = EditorGUILayout.IntSlider("Source Images", gameMaster.scalePictureInfo.sourceImages, 0, 60000);
                gameMaster.scalePictureInfo.imagesToProduce = EditorGUILayout.IntField("Images To Produce", gameMaster.scalePictureInfo.imagesToProduce);
                gameMaster.scalePictureDimensionMin = EditorGUILayout.IntField("Dimension Min", gameMaster.scalePictureDimensionMin);
                gameMaster.scalePictureDimensionMax = EditorGUILayout.IntField("Dimension Max", gameMaster.scalePictureDimensionMax);
            }
        }

        EditorGUILayout.Space();

        // Save and load.
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Save"))
            {
                if (gameMaster.selectedNeuralNetwork == null || !gameMaster.selectedNeuralNetwork.IsNetworkCreated())
                {
                    EditorUtility.DisplayDialog("Attention", "A network type must be selected and trained to enable the option to save.", "ok");
                    return;
                }

                if (gameMaster.selectedNeuralNetwork.IsTraining() || gameMaster.TestingCoroutine != null)
                {
                    EditorUtility.DisplayDialog("Attention", "Network can not be saved while training/Testing is in progress.", "ok");
                    return;
                }

                string savePath = EditorUtility.SaveFilePanel("Save network", saveDirectory, "NeuralNetwork.nnet", "nnet");

                if (savePath.Length != 0)
                    gameMaster.SaveData(savePath);
            }

            if (GUILayout.Button("Load"))
            {
                if (gameMaster.selectedNeuralNetwork == null)
                {
                    EditorUtility.DisplayDialog("Attention", "A network type must be selected.", "ok");
                    return;
                }

                if (gameMaster.selectedNeuralNetwork.IsTraining() || gameMaster.TestingCoroutine != null)
                {
                    EditorUtility.DisplayDialog("Attention", "Network can not be loaded while training/Testing is in progress.", "ok");
                    return;
                }

                if (gameMaster.selectedNeuralNetwork.IsNetworkCreated())
                {
                    if (!EditorUtility.DisplayDialog("Attention", "The current network will be deleted if you proceed.", "ok", "cancel"))
                        return;
                }

                string loadPath = EditorUtility.OpenFilePanel("Load neural network", loadDirectory, "nnet");

                if (loadPath.Length != 0)
                    gameMaster.LoadData(loadPath);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}