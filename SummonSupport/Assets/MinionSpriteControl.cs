using UnityEngine;
using System.Collections.Generic;
using System;


public class MinionSpriteControl : MonoBehaviour
{
    #region Color control
    public Dictionary<RGBAEnum, (Func<float> Get, Action<float> Set)> ColorDict { private set; get; } = new();
    private float redness;
    private float greeness;
    private float blueness;
    private float alphaness;

    private SpriteRenderer spriteRenderer;

    #endregion

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitializeColorDict();
    }



    #region color control
    public void AlterColorByAffinity(Element strongestElement)
    {
        string str = strongestElement.ToString();
        if (str.Contains("Cold") || str.Contains("Water"))
        {
            SetColor(new float[4] { 0f, 0f, 1f, 1f });
        }
        if (str.Contains("Plant") || str.Contains("Bacteria"))
        {
            SetColor(new float[4] { 0f, 1f, 0f, 1f });
        }
        if (str.Contains("Virus") || str.Contains("Acid"))
        {
            SetColor(new float[4] { 0.9f, 0.7f, 0.0f, 1.0f });
        }
        if (str.Contains("Light") || str.Contains("Electricity"))
        {
            SetColor(new float[4] { 0.85f, 0.85f, 0.0f, 1.0f });
        }
        if (str.Contains("Heat") || str.Contains("Radiation"))
        {
            SetColor(new float[4] { 1f, 0f, 0.0f, 1.0f });
        }
        if (str.Contains("Psychic") || str.Contains("Poison"))
        {
            SetColor(new float[4] { 0.5f, 0f, .5f, 1.0f });
        }
        if (str.Contains("Fungi") || str.Contains("Earth"))
        {
            SetColor(new float[4] { .4f, 0.4f, .4f, 1.0f });
        }
    }

    public void SlideColor(RGBAEnum color, float range)
    {
        ColorDict[color].Set(range);
        SetColor(new float[] { redness, greeness, blueness, alphaness });
    }
    public void InitializeColorDict()
    {
        ColorDict = new Dictionary<RGBAEnum, (Func<float> Get, Action<float> Set)>
            {
                { RGBAEnum.Red,           (() => redness,               v => redness = v) },
                { RGBAEnum.Green,           (() => greeness,               v => greeness = v) },
                { RGBAEnum.Blue,           (() => blueness,               v => blueness = v) },
                { RGBAEnum.Alpha,           (() => alphaness,               v => alphaness = v) },
            };
    }

    public void SetColor(float[] rgbaValues)
    {
        float r = rgbaValues[0];
        float g = rgbaValues[1];
        float b = rgbaValues[2];
        float a = rgbaValues[3];
        spriteRenderer.color = new Color(r, g, b, a);
    }
    public enum RGBAEnum
    {
        Red,
        Green,
        Blue,
        Alpha
    }
    #endregion
}
