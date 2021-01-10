using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;

public class DigitImage
{
    public Texture2D digitTexture;
    public byte[][] pixels;
    public float[] linearPixels;
    public byte label;

    public DigitImage(byte[][] pixels, byte label)
    {
        digitTexture = new Texture2D(28, 28);

        for (int i = 0; i < 28; i++)
            for (int j = 0; j < 28; j++)
                digitTexture.SetPixel(i, j, new Color(pixels[i][j] / 255f, pixels[i][j] / 255f, pixels[i][j] / 255f, 1));

        digitTexture.filterMode = FilterMode.Point;
        digitTexture.Apply();

        this.pixels = new byte[28][];
        for (int i = 0; i < this.pixels.Length; i++)
            this.pixels[i] = new byte[28];

        for (int i = 0; i < 28; i++)
            for (int j = 0; j < 28; j++)
            {
                this.pixels[i][j] = pixels[i][j];

                linearPixels[(i * 28) +  j] = pixels[i][j];
            }

        this.label = label;
    }
}

public class ReadData : MonoBehaviour
{
    public RawImage displayImage;

    public List<DigitImage> digitImages = new List<DigitImage>();
    private Texture2D drawImage;

    private void Start()
    {
        ReadMNIST();

        byte[] _bytes = digitImages[2].digitTexture.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "test.png"), _bytes);

        drawImage = new Texture2D(28, 28);

        for (int i = 0; i < 28; i++)
            for (int j = 0; j < 28; j++)
                drawImage.SetPixel(i, j, new Color(0, 0, 0, 1));

        drawImage.filterMode = FilterMode.Point;
        drawImage.Apply();

        displayImage.texture = drawImage;

        //displayImage.texture = digitImages[0].digitTexture;
    }

    public void Draw()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(displayImage.rectTransform, Input.mousePosition, Camera.main, out pos);

        if (!displayImage.rectTransform.rect.Contains(pos))
            return;

        Color drawColor = new Color(1, 1, 1, 1);

        if (Input.GetMouseButton(1))
            drawColor = new Color(0, 0, 0, 1);

        pos = new Vector2(pos.x + (displayImage.rectTransform.rect.width / 2), pos.y + (displayImage.rectTransform.rect.height / 2)); // move origo
        pos = new Vector2(Mathf.Floor(pos.x / (displayImage.rectTransform.rect.width / 28f)), Mathf.Floor(pos.y / (displayImage.rectTransform.rect.height / 28f)));

        drawImage.SetPixel((int)pos.x, (int)pos.y, drawColor);
        drawImage.Apply();
    }

    public void ClearTexture()
    {
        for (int i = 0; i < 28; i++)
            for (int j = 0; j < 28; j++)
                drawImage.SetPixel(i, j, new Color(0, 0, 0, 1));

        drawImage.Apply();
    }

    private void ReadMNIST()
    {
        try
        {
            FileStream ifsLabels = new FileStream("Assets/TrainingData/train-labels.idx1-ubyte", FileMode.Open);
            FileStream ifsImages = new FileStream("Assets/TrainingData/train-images.idx3-ubyte", FileMode.Open);

            BinaryReader brLabels = new BinaryReader(ifsLabels);
            BinaryReader brImages = new BinaryReader(ifsImages);

            int magic1 = brImages.ReadInt32(); // discard
            int numImages = brImages.ReadInt32();
            int numRows = brImages.ReadInt32();
            int numCols = brImages.ReadInt32();

            int magic2 = brLabels.ReadInt32();
            int numLabels = brLabels.ReadInt32();

            byte[][] pixels = new byte[28][];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new byte[28];

            for (int di = 0; di < 10000; di++)
            {
                for (int i = 27; i >= 0; i--)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        byte b = brImages.ReadByte();
                        pixels[j][i] = b;
                    }
                }

                byte lbl = brLabels.ReadByte();

                digitImages.Add(new DigitImage(pixels, lbl));
            }

            ifsImages.Close();
            brImages.Close();
            ifsLabels.Close();
            brLabels.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
}