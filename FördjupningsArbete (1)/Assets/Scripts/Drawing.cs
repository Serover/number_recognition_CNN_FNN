using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class Drawing : MonoBehaviour
{
    [Header("Draw Settings")]
    public float mainStr = 0.2f;
    public float neighborStr;
    public float furtherneighborStr;

    [Header("UI")]
    public RawImage displayImage;

    public static Texture2D drawImage;

    private GameMaster gameMaster;

    private void Start()
    {
        gameMaster = (GameMaster)FindObjectOfType(typeof(GameMaster));

        drawImage = new Texture2D(28, 28);
        drawImage.filterMode = FilterMode.Point;
        ClearTexture();

        displayImage.texture = drawImage;
    }

    public void Draw()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(displayImage.rectTransform, Input.mousePosition, Camera.main, out pos);

        if (!displayImage.rectTransform.rect.Contains(pos))
            return;

        Color drawColor = new Color(mainStr, mainStr, mainStr, 1);

        pos = new Vector2(pos.x + (displayImage.rectTransform.rect.width / 2), pos.y + (displayImage.rectTransform.rect.height / 2));
        pos = new Vector2(Mathf.Floor(pos.x / (displayImage.rectTransform.rect.width / 28f)), Mathf.Floor(pos.y / (displayImage.rectTransform.rect.height / 28f)));

        if (Input.GetMouseButton(1))
        {
            drawColor = new Color(-1, -1, -1, 1);
            Color pixelStr = drawImage.GetPixel((int)pos.x, (int)pos.y);
            Color finalCol = pixelStr + drawColor;
            drawImage.SetPixel((int)pos.x, (int)pos.y, finalCol);
        }
        else
        {
            for (int i = -1; i < 2; i++)
            {
                Color pixelStr = drawImage.GetPixel((int)pos.x + i, (int)pos.y);
                Color finalCol = pixelStr + drawColor;
                drawImage.SetPixel((int)pos.x + i, (int)pos.y, finalCol);
            }
            for (int i = -1; i < 2; i++)
            {
                Color pixelStr = drawImage.GetPixel((int)pos.x, (int)pos.y + i);
                Color finalCol = pixelStr + drawColor;
                drawImage.SetPixel((int)pos.x, (int)pos.y + i, finalCol);
            }
        }
        drawImage.Apply();

        gameMaster.ReadDigit();
    }

    public void DrawTestData()
    {
        List<Color> pixels = new List<Color>();

        int index = gameMaster.showFailsToggle.isOn ? gameMaster.wrongGuesses[(int)gameMaster.testDataSlider.value - 1] : (int)gameMaster.testDataSlider.value - 1;

        foreach (float color in ReadData.GetArrayFrom2DArray(ReadData.testData.Item1,index))
            pixels.Add(new Color(color, color, color, 1));

        gameMaster.testDataLabelText.text = "The Number is: " + gameMaster.FromArrayToLabel(ReadData.GetArrayFrom2DArray(ReadData.testData.Item2, index));

        drawImage.SetPixels(pixels.ToArray());
        drawImage.Apply();

        gameMaster.ReadDigit();
    }

    public void DrawMyImage(float[] inputPixels)
    {
        List<Color> pixels = new List<Color>();

        foreach (float color in inputPixels)
            pixels.Add(new Color(color, color, color, 1));

        drawImage.SetPixels(pixels.ToArray());
        drawImage.Apply();

        gameMaster.ReadDigit();
    }

    public void ClearTexture()
    {
        for (int i = 0; i < 28; i++)
            for (int j = 0; j < 28; j++)
                drawImage.SetPixel(i, j, new Color(0, 0, 0, 1));

        gameMaster.ReadDigit();

        drawImage.Apply();
    }
}