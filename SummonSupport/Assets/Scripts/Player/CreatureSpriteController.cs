using SummonSupportEvents;
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
    [SerializeField] private Sprite DeathSprite;


    [field: SerializeField] public bool unfinished { get; private set; } = false;
    [field: SerializeField] public bool IsoOnly { get; private set; } = false;

    private bool dead = false;








    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

    }
    void OnEnable()
    {
        EventDeclarer.minionDied?.AddListener(CheckDead);
    }


    void OnDisable()
    {
        EventDeclarer.minionDied?.RemoveListener(CheckDead);
    }

    private void CheckDead(GameObject minionObject)
    {
        if (transform.parent == minionObject)
            dead = true;
    }

    public void SetSpriteDirection(float angle)
    {
        if (dead) sr.sprite = DeathSprite;
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
        SetColor(EffectColorChanger.GetColorFromElement(strongestElement));
        SetGlow(EffectColorChanger.GetGlowByElement(strongestElement));
    }
    public void SetColor(float[] rgbaValues)
    {
        float r = rgbaValues[0];
        float g = rgbaValues[1];
        float b = rgbaValues[2];
        float a = rgbaValues[3];
        sr.color = new Color(r, g, b, a);
    }
    private void SetGlow(Material glowMaterial)
    {
        sr.material = glowMaterial;
    }
}
