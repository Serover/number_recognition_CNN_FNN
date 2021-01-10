using UnityEngine;
using System.Collections.Generic;

public class AugmentData : MonoBehaviour
{
    public static void ElasticDistortion() // try at elastic distortion (not done)
    {
        float[][] kernel = new float[9][];
        kernel[0] = new float[9] { 0.0067f, 0.0083f, 0.0097f, 0.0107f, 0.011f, 0.0107f, 0.0097f, 0.0083f, 0.0067f };
        kernel[1] = new float[9] { 0.0083f, 0.0103f, 0.0121f, 0.0133f, 0.0137f, 0.0133f, 0.0121f, 0.0103f, 0.0083f };
        kernel[2] = new float[9] { 0.0097f, 0.0121f, 0.0141f, 0.0155f, 0.016f, 0.0155f, 0.0141f, 0.0121f, 0.0097f };
        kernel[3] = new float[9] { 0.0107f, 0.0133f, 0.0155f, 0.017f, 0.0176f, 0.017f, 0.0155f, 0.0133f, 0.0107f };
        kernel[4] = new float[9] { 0.011f, 0.0137f, 0.016f, 0.0176f, 0.0181f, 0.0176f, 0.016f, 0.0137f, 0.011f };
        kernel[5] = new float[9] { 0.0107f, 0.0133f, 0.0155f, 0.017f, 0.0176f, 0.017f, 0.0155f, 0.0133f, 0.0107f };
        kernel[6] = new float[9] { 0.0097f, 0.0121f, 0.0141f, 0.0155f, 0.016f, 0.0155f, 0.0141f, 0.0121f, 0.0097f };
        kernel[7] = new float[9] { 0.0083f, 0.0103f, 0.0121f, 0.0133f, 0.0137f, 0.0133f, 0.0121f, 0.0103f, 0.0083f };
        kernel[8] = new float[9] { 0.0067f, 0.0083f, 0.0097f, 0.0107f, 0.011f, 0.0107f, 0.0097f, 0.0083f, 0.0067f };

        float[,] x = new float[28, 28];
        float[,] y = new float[28, 28];

        float[,] Blur(float[,] data)
        {
            float Convolv(int index1, int index2)
            {
                float sum = 0;

                int kernelRadius = -Mathf.FloorToInt(kernel.Length / 2);

                for (int i = 0; i < kernel.Length; i++)
                {
                    for (int j = 0; j < kernel[i].Length; j++)
                    {
                        int temp1 = -kernelRadius + i + index1;
                        int temp2 = -kernelRadius + j + index2;

                        if (temp1 >= 0 && temp1 < data.GetLength(0) && temp2 >= 0 && temp2 < data.GetLength(0))
                            sum += kernel[i][j] * data[temp1, temp2];
                    }
                }

                return sum;
            }

            float[,] newData = new float[28, 28];

            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                    newData[i, j] = Convolv(i, j);

            return newData;
        }

        for (int i = 0; i < x.GetLength(0); i++)
            for (int j = 0; j < x.GetLength(1); j++)
                x[i, j] = UnityEngine.Random.Range(-1, 1);

        x = Blur(x);

        for (int i = 0; i < y.GetLength(0); i++)
            for (int j = 0; j < y.GetLength(1); j++)
                y[i, j] = UnityEngine.Random.Range(-1, 1);

        y = Blur(y);

        Vector2[,] displacementField = new Vector2[28, 28];

        for (int i = 0; i < displacementField.GetLength(0); i++)
            for (int j = 0; j < displacementField.GetLength(1); j++)
                displacementField[i, j] = new Vector2(x[i, j], y[i, j]);
    }

    /// <param name="input"></param>
    /// <param name="outputSize"></param>
    /// <param name="odds"> A number between 0-100, giving the odds for a erase to occur </param>
    /// <returns></returns>
    public static float[,] RandomErasing(float[,] input, int outputSize, int odds)
    {
        // move later
        UnityEngine.Random.InitState(GameMaster.seed);

        float[,] output = new float[outputSize, input.GetLength(1)];

        float[,] picture;

        for (int l = 0; l < outputSize; l++)
        {
            picture = ReadData.Get2DArrayFrom1DArray(ReadData.GetArrayFrom2DArray(input, l), 28);

            for (int i = 0; i < picture.GetLength(0); i++)
            {
                for (int j = 0; j < picture.GetLength(1); j++)
                {
                    int numb = UnityEngine.Random.Range(0, 101);

                    if (numb < odds)
                    {
                        picture[i, j] = 0;
                    }
                }
            }

            float[] longPicture = ReadData.Get1DArrayFrom2DArray(picture);

            // try with buffer
            for (int c = 0; c < longPicture.Length; c++)
            {
                output[l, c] = longPicture[c];
            }
        }
        return output;
    }

    public static float[,] ArrayTranslation(float[,] input, int outputSize, int minRange, int maxRange)
    {
        // move later
        //  UnityEngine.Random.InitState(GameMaster.seed);

        // 28
        int TwoDSize = (int)Mathf.Sqrt(input.GetLength(1));

        float[,] output = new float[outputSize, input.GetLength(1)];

        float[,] picture;
        float[,] pictureCopy = new float[TwoDSize, TwoDSize];

        for (int l = 0; l < outputSize; l++)
        {
            // Fetch an array in an 2d array, in this case, fetch the 784 pixels array, then transform those 784 pixels into a 28 x 28 2d array.
            picture = ReadData.Get2DArrayFrom1DArray(ReadData.GetArrayFrom2DArray(input, l), TwoDSize);

            int xTranslation = UnityEngine.Random.Range(minRange, maxRange);
            int yTranslation = UnityEngine.Random.Range(minRange, maxRange);
            int xMod = UnityEngine.Random.Range(1, 3) > 1 ? -1 : 1;
            int yMod = UnityEngine.Random.Range(1, 3) > 1 ? -1 : 1;

            for (int i = 0; i < picture.GetLength(0) - yTranslation; i++)
            {
                for (int j = 0; j < picture.GetLength(1) - xTranslation; j++)
                {
                    if (yTranslation * yMod + i >= 0 && xTranslation * xMod + j >= 0)
                        pictureCopy[i + yTranslation * yMod, j + xTranslation * xMod] = picture[i, j];
                }
            }

            // put pic into new format
            float[] longPicture = ReadData.Get1DArrayFrom2DArray(pictureCopy);

            // try with buffer
            for (int c = 0; c < longPicture.Length; c++)
            {
                output[l, c] = longPicture[c];
            }
        }
        return output;
    }

    public static float[,] RotatePicture(float[,] input, int outputSize, int angle)
    {
        float[,] output = new float[outputSize, input.GetLength(1)];

        //ImageRotator.RotateImage()
        for (int l = 0; l < outputSize; l++)
        {
            // Create a texture2D to store the OG imagei n.
            Texture2D pictureImage = new Texture2D(28, 28);
            List<Color> pixels = new List<Color>();
            // 784 float that stores the image
            float[] pictureArrayFloat = ReadData.GetArrayFrom2DArray(input, l);

            // Make all float values into color values.
            foreach (float color in pictureArrayFloat)
                pixels.Add(new Color(color, color, color, 1));

            // Store image into the storage.
            pictureImage.SetPixels(pixels.ToArray());
            pictureImage.Apply();

            // Create new picture with smaler size to put the picture into.
            Texture2D newPictureFolder = new Texture2D(28, 28);
            newPictureFolder = ImageRotator.RotateImage(pictureImage, angle);

            // pictureImage.Apply();
            float[] newPictureFloatArray = new float[784];
            Color[] nC = newPictureFolder.GetPixels();

            for (int i = 0; i < newPictureFolder.GetPixels().Length; i++)
            {
                newPictureFloatArray[i] = nC[i].r;
            }

            for (int i = 0; i < newPictureFloatArray.Length; i++)
            {
                output[l, i] = newPictureFloatArray[i];
            }
        }
        return output;
    }

    public static float[,] ScalePicture(float[,] input, int outputSize, int Dimension)
    {
        // create a new float[,] with correct output size
        float[,] output = new float[outputSize, input.GetLength(1)];

        for (int l = 0; l < outputSize; l++)
        {

            // Create a texture2D to store the OG imagei n.
            Texture2D pictureImage = new Texture2D(28, 28);
            List<Color> pixels = new List<Color>();
            // 784 float that stores the image
            float[] pictureArrayFloat = ReadData.GetArrayFrom2DArray(input, l);

            // Make all float values into color values.
            foreach (float color in pictureArrayFloat)
                pixels.Add(new Color(color, color, color, 1));

            // Store image into the storage.
            pictureImage.SetPixels(pixels.ToArray());
            pictureImage.Apply();
            // Resize
            TextureScale.Bilinear(pictureImage, Dimension, Dimension);

            pictureImage.Apply();

            // Create new picture with smaler size to put the picture into.
            Texture2D newPictureFolder = new Texture2D(28, 28);

            // Get 28x28 colors from the OG pixels
            int tSize = 28;
            if (Dimension < 28)
                tSize = Dimension;

            Color[] c;
            if (Dimension < 28)
            {
                int dDiff = 28 - Dimension;
                float[,] TwoDPic = new float[28, 28];

                for (int j = 0; j < 28; j++)
                {
                    for (int i = 0; i < 28; i++)
                    {
                        if (j < dDiff / 2 || j >= Dimension + dDiff / 2)
                        {
                            TwoDPic[j, i] = 0;
                        }
                        else if (i < dDiff / 2 || i >= Dimension + dDiff / 2)
                        {
                            TwoDPic[j, i] = 0;
                        }
                        else
                        {
                            float Col = pictureImage.GetPixel(i - dDiff / 2, j - dDiff / 2).r;
                            TwoDPic[j, i] = Col;
                        }
                    }
                }

                float[] oneDOfNewPic = new float[784];
                oneDOfNewPic = ReadData.Get1DArrayFrom2DArray(TwoDPic);

                Color[] cc = new Color[784];
                for (int i = 0; i < oneDOfNewPic.Length; i++)
                {
                    cc[i] = new Color(oneDOfNewPic[i], oneDOfNewPic[i], oneDOfNewPic[i], 1);
                }

                c = cc;
            }
            else
            {
                c = pictureImage.GetPixels(0, 0, 28, 28);
            }
            // Put them into the new picture
            newPictureFolder.SetPixels(c);
            newPictureFolder.Apply();

            // Make the new pictures colors into floats
            float[] newPictureFloatArray = new float[784];
            Color[] nC = newPictureFolder.GetPixels();

            for (int i = 0; i < newPictureFolder.GetPixels().Length; i++)
            {
                newPictureFloatArray[i] = nC[i].r;
            }


            for (int i = 0; i < newPictureFloatArray.Length; i++)
            {
                output[l, i] = newPictureFloatArray[i];
            }

        }

        return output;
    }

    public static float[,] ElasticEJ(float[,] input, int outputSize, float alpha)
    {
        float[,] output = new float[outputSize, input.GetLength(1)];

        for (int l = 0; l < outputSize; l++)
        {
            float[,] filter = new float[28, 28];

            // Sigma = 4
            float[][] kernel = new float[9][];
            kernel[0] = new float[9] { 0.0067f, 0.0083f, 0.0097f, 0.0107f, 0.011f, 0.0107f, 0.0097f, 0.0083f, 0.0067f };
            kernel[1] = new float[9] { 0.0083f, 0.0103f, 0.0121f, 0.0133f, 0.0137f, 0.0133f, 0.0121f, 0.0103f, 0.0083f };
            kernel[2] = new float[9] { 0.0097f, 0.0121f, 0.0141f, 0.0155f, 0.016f, 0.0155f, 0.0141f, 0.0121f, 0.0097f };
            kernel[3] = new float[9] { 0.0107f, 0.0133f, 0.0155f, 0.017f, 0.0176f, 0.017f, 0.0155f, 0.0133f, 0.0107f };
            kernel[4] = new float[9] { 0.011f, 0.0137f, 0.016f, 0.0176f, 0.0181f, 0.0176f, 0.016f, 0.0137f, 0.011f };
            kernel[5] = new float[9] { 0.0107f, 0.0133f, 0.0155f, 0.017f, 0.0176f, 0.017f, 0.0155f, 0.0133f, 0.0107f };
            kernel[6] = new float[9] { 0.0097f, 0.0121f, 0.0141f, 0.0155f, 0.016f, 0.0155f, 0.0141f, 0.0121f, 0.0097f };
            kernel[7] = new float[9] { 0.0083f, 0.0103f, 0.0121f, 0.0133f, 0.0137f, 0.0133f, 0.0121f, 0.0103f, 0.0083f };
            kernel[8] = new float[9] { 0.0067f, 0.0083f, 0.0097f, 0.0107f, 0.011f, 0.0107f, 0.0097f, 0.0083f, 0.0067f };

            /*
            // Sigma = 3
            float[][] kernel = new float[9][];
            kernel[0] = new float[9] {0.004f,   0.0059f,  0.0077f,  0.0091f,  0.0096f,  0.0091f,  0.0077f,  0.0059f,  0.004f };
            kernel[1] = new float[9] {0.0059f,  0.0086f,  0.0114f,  0.0135f,  0.0142f,  0.0135f,  0.0114f,  0.0086f,  0.0059f };
            kernel[2] = new float[9] {0.0077f,  0.0114f,  0.015f,   0.0178f,  0.0188f,  0.0178f,  0.015f,   0.0114f,  0.0077f };
            kernel[3] = new float[9] {0.0091f,  0.0135f,  0.0178f,  0.021f,   0.0222f,  0.021f,   0.0178f,  0.0135f,  0.0091f };
            kernel[4] = new float[9] {0.0096f,  0.0142f,  0.0188f,  0.0222f,  0.0235f,  0.0222f,  0.0188f,  0.0142f,  0.0096f };
            kernel[5] = new float[9] {0.0091f,  0.0135f,  0.0178f,  0.021f,   0.0222f,  0.021f,   0.0178f,  0.0135f,  0.0091f };
            kernel[6] = new float[9] {0.0077f,  0.0114f,  0.015f,   0.0178f,  0.0188f,  0.0178f,  0.015f,   0.0114f,  0.0077f };
            kernel[7] = new float[9] {0.0059f,  0.0086f,  0.0114f,  0.0135f,  0.0142f,  0.0135f,  0.0114f,  0.0086f,  0.0059f };
            kernel[8] = new float[9] {0.004f,   0.0059f,  0.0077f,  0.0091f,  0.0096f,  0.0091f,  0.0077f,  0.0059f,  0.004f };
            */

            /*
            // Sigma = 4
            float[][] kernel = new float[5][];
            kernel[0] = new float[5] { 0.0352f, 0.0387f, 0.0399f, 0.0387f, 0.0352f};
            kernel[1] = new float[5] { 0.0387f, 0.0425f, 0.0438f, 0.0425f, 0.0387f};
            kernel[2] = new float[5] { 0.0399f, 0.0438f, 0.0452f, 0.0438f, 0.0399f};
            kernel[3] = new float[5] { 0.0387f, 0.0425f, 0.0438f, 0.0425f, 0.0387f};
            kernel[4] = new float[5] { 0.0352f, 0.0387f, 0.0399f, 0.0387f, 0.0352f};
            */

            float[,] x = new float[28, 28];
            float[,] y = new float[28, 28];

            float[,] Blur(float[,] data)
            {
                float Convolv(int index1, int index2)
                {
                    float sum = 0;

                    int kernelRadius = -Mathf.FloorToInt(kernel.Length / 2);

                    for (int i = 0; i < kernel.Length; i++)
                    {
                        for (int j = 0; j < kernel[i].Length; j++)
                        {
                            int temp1 = (-kernelRadius + i + index1) % data.GetLength(0);
                            int temp2 = (-kernelRadius + j + index2) % data.GetLength(0);

                            sum += kernel[i][j] * data[temp1, temp2];
                        }
                    }

                    return sum;
                }

                float[,] newData = new float[28, 28];

                for (int i = 0; i < data.GetLength(0); i++)
                    for (int j = 0; j < data.GetLength(1); j++)
                        newData[i, j] = Convolv(i, j);

                return newData;
            }

            for (int i = 0; i < x.GetLength(0); i++)
                for (int j = 0; j < x.GetLength(1); j++)
                    x[i, j] = UnityEngine.Random.Range(-1f, 1f);

            x = Blur(x);

            for (int i = 0; i < y.GetLength(0); i++)
                for (int j = 0; j < y.GetLength(1); j++)
                    y[i, j] = UnityEngine.Random.Range(-1f, 1f);

            y = Blur(y);

            Vector2[,] displacementField = new Vector2[28, 28];

            //for (int i = 0; i < displacementField.GetLength(0); i++)
            //    for (int j = 0; j < displacementField.GetLength(1); j++)
            //        displacementField[i, j] = new Vector2(x[i, j], y[i, j]);

            for (int i = 0; i < displacementField.GetLength(0); i++)
                for (int j = 0; j < displacementField.GetLength(1); j++)
                    displacementField[i, j] = new Vector2(1, 0);

            //for (int i = 0; i < displacementField.GetLength(0); i++)
            //    for (int j = 0; j < displacementField.GetLength(1); j++)
            //        displacementField[i, j].Normalize();

            //for (int i = 0; i < displacementField.GetLength(0); i++)
            //    for (int j = 0; j < displacementField.GetLength(1); j++)
            //        displacementField[i, j] *= alpha;

            DestroyImmediate(GameObject.Find("LineHolder"));
            GameObject lineHolder = new GameObject("LineHolder");

            for (int i = 0; i < displacementField.GetLength(0); i++)
            {
                for (int j = 0; j < displacementField.GetLength(1); j++)
                {
                    GameObject gObject = new GameObject(i + " : " + j);
                    gObject.transform.parent = lineHolder.transform;
                    LineRenderer lRend = gObject.AddComponent<LineRenderer>();

                    lRend.startWidth = 0.1f;
                    lRend.endWidth = 0.2f;
                    lRend.SetPosition(0, new Vector3(i, j, 0));
                    lRend.SetPosition(1, new Vector3(i + displacementField[i, j].x, j + displacementField[i, j].y, 0));
                }
            }

            float[,] picture = ReadData.Get2DArrayFrom1DArray(ReadData.GetArrayFrom2DArray(input, l), 28);

            float[,] newpic = ReadData.Get2DArrayFrom1DArray(ReadData.GetArrayFrom2DArray(input, l), 28);

            for (int i = 0; i < displacementField.GetLength(0); i++)
            {
                for (int j = 0; j < displacementField.GetLength(1); j++)
                {
                    //// pixel + displaysment interpolation sker här?
                    float dx = displacementField[i, j].x;
                    float dy = displacementField[i, j].y;

                    int xOffSet = dx >= 0 ? 1 : -1;
                    int yOffSet = dy >= 0 ? 1 : -1;

                    //int xOffSet = ((int)(dx * 10) - ((int)dx) * 10) >= 5 ? 1 : -1;
                    //int yOffSet = ((int)(dy * 10) - ((int)dy) * 10) >= 5 ? 1 : -1;

                    int y0 = j + (int)dx - xOffSet >= 0 && j + (int)dx - xOffSet < 28 ? j + (int)dx - xOffSet : 0;
                    int y1 = j + (int)dx >= 0 && j + (int)dx < 28 ? j + (int)dx : 0;
                    int y2 = j + (int)dx + xOffSet >= 0 && j + (int)dx + xOffSet < 28 ? j + (int)dx + xOffSet : 0;
                    int y3 = j + (int)dx + (2 * xOffSet) >= 0 && j + (int)dx + (2 * xOffSet) < 28 ? j + (int)dx + (2 * xOffSet) : 0;

                    int x0 = i + (int)dy - yOffSet >= 0 && i + (int)dy - yOffSet < 28 ? i + (int)dy - yOffSet : 0;
                    int x1 = i + (int)dy >= 0 && i + (int)dy < 28 ? i + (int)dy : 0;
                    int x2 = i + (int)dy + yOffSet >= 0 && i + (int)dy + yOffSet < 28 ? i + (int)dy + yOffSet : 0;
                    int x3 = i + (int)dy + (2 * yOffSet) >= 0 && i + (int)dy + (2 * yOffSet) < 28 ? i + (int)dy + (2 * yOffSet) : 0;

                    //float interp_1 = Mathf.Min(Mathf.Max(picture[x1, y1] + (xOffSet * (dx - (int)dx)) * (picture[x2, y1] - picture[x1, y1]), 0), 1);
                    //float interp_2 = Mathf.Min(Mathf.Max(picture[x1, y2] + (yOffSet * (dx - (int)dx)) * (picture[x2, y2] - picture[x1, y2]), 0), 1);

                    //float interp_f = Mathf.Min(Mathf.Max(interp_1 + (dy - (int)dy) * (interp_2 - interp_1), 0), 1);

                    //newpic[i, j] = interp_f;



                    float CubicPolate(float v0, float v1, float v2, float v3, float frac)
                    {
                        float A = (v3 - v2) - (v0 - v1);
                        float B = (v0 - v1) - A;
                        float C = v2 - v0;
                        float D = v1;

                        return A * Mathf.Pow(frac, 3) + B * Mathf.Pow(frac, 2) + C * frac + D;
                    }

                    float xValue1 = CubicPolate(picture[x0, y0], picture[x1, y0], picture[x2, y0], picture[x3, y0], dx);
                    float xValue2 = CubicPolate(picture[x0, y1], picture[x1, y1], picture[x2, y1], picture[x3, y1], dx);
                    float xValue3 = CubicPolate(picture[x0, y2], picture[x1, y2], picture[x2, y2], picture[x3, y2], dx);
                    float xValue4 = CubicPolate(picture[x0, y3], picture[x1, y3], picture[x2, y3], picture[x3, y3], dx);

                    newpic[i, j] = CubicPolate(xValue1, xValue2, xValue3, xValue4, dy);
                }
            }

            float[] longNewPic = ReadData.Get1DArrayFrom2DArray(newpic);

            for (int i = 0; i < 784; i++)
            {
                output[l, i] = longNewPic[i];
            }
        }
        return output;
    }
}
