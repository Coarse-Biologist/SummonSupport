using UnityEngine;

public class CreatureSpriteController : MonoBehaviour
{
    private SpriteRenderer sr;
    [SerializeField] private Sprite NSprite;
    [SerializeField] private Sprite ESprite;
    [SerializeField] private Sprite SSprite;
    [SerializeField] private Sprite WSprite;
    [SerializeField] private Sprite NESprite;
    [SerializeField] private Sprite SESprite;
    [SerializeField] private Sprite SWSprite;
    [SerializeField] private Sprite NWSprite;
    [field: SerializeField] public bool unfinished { get; private set; } = false;
    [field: SerializeField] public bool IsoOnly { get; private set; } = false;







    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetSpriteDirection(float angle)
    {
        if (IsoOnly)
        {
            if (angle >= -180 && angle < -90) sr.sprite = SWSprite;
            else if (angle >= -90 && angle < 0) sr.sprite = SESprite;
            else if (angle >= 90 && angle < 180) sr.sprite = NWSprite;
            else if (angle >= 0 && angle < 90) sr.sprite = NESprite;

        }
        else
        {
            if (!unfinished)
            {
                if (angle >= -22.5f && angle < 22.5f) sr.sprite = ESprite;
                else if (angle >= 22.5f && angle < 67.5f) sr.sprite = NESprite;
                else if (angle >= 67.5f && angle < 112.5f) sr.sprite = NSprite;
                else if (angle >= 112.5f && angle < 157.5f) sr.sprite = NWSprite;
                else if (angle >= 157.5f || angle < -157.5f) sr.sprite = WSprite;
                else if (angle >= -157.5f && angle < -112.5f) sr.sprite = SWSprite;
                else if (angle >= -112.5f && angle < -67.5f) sr.sprite = SSprite;
                else if (angle >= -67.5f && angle < -22.5f) sr.sprite = SESprite;
            }
        }

    }

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
    public void SetColor(float[] rgbaValues)
    {
        float r = rgbaValues[0];
        float g = rgbaValues[1];
        float b = rgbaValues[2];
        float a = rgbaValues[3];
        sr.color = new Color(r, g, b, a);
    }
}
