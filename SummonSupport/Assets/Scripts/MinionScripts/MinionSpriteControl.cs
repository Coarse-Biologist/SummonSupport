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
        SetColor(EffectColorChanger.GetColorFromElement(strongestElement));
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
