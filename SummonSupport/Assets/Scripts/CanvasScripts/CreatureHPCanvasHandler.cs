using UnityEngine;
using UnityEngine.UI;

public class CreatureHPCanvasHandler : MonoBehaviour, I_ResourceBar
{
    //[field: SerializeField] public Sprite statusEffectSprite;
    private LivingBeing livingBeing;
    private Slider hpSlider;
    private Canvas canvas;
    //private Slider manaSlider;

    void Awake()
    {
        livingBeing = gameObject.GetComponent<LivingBeing>();
        Slider[] sliders = GetComponentsInChildren<Slider>();
        hpSlider = sliders[0];
        canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            Debug.Log("Canvas found!");
        }

        // manaSlider = sliders[1];



        //Logging.Info($"{manaSlider} = mana slider. {hpSlider} = hpslider");
    }

    void Start()
    {
        SetHealthBarMaxValue();
        SetHealthBarValue();
        SetPowerBarMaxValue();
        SetPowerBarValue();
    }
    public void SetHealthBarValue(float value = 1)
    {
        hpSlider.value = livingBeing.GetAttribute(AttributeType.CurrentHitpoints);
    }
    public void SetHealthBarMaxValue(float value = 1)
    {
        hpSlider.maxValue = livingBeing.GetAttribute(AttributeType.MaxHitpoints);
    }

    public void SetPowerBarValue(float value = 1)
    {
        //manaSlider.value = livingBeing.GetAttribute(AttributeType.CurrentPower);
    }

    public void SetPowerBarMaxValue(float value = 1)
    {
        //manaSlider.maxValue = livingBeing.GetAttribute(AttributeType.MaxPower);
    }

    public void AddStatusEffectSymbol(StatusEffects status, int stacks)
    {
        if (status != null && status.Icon != null)
        {
            Debug.Log("Trying to add the thing.");
            GameObject statusImage = new GameObject("StatusEffectImage");

            // Parent it to the Canvas
            statusImage.transform.SetParent(canvas.transform, false);

            // Add Image component
            Image img = statusImage.AddComponent<Image>();
            img.sprite = status.Icon;

            // Configure RectTransform
            RectTransform rt = img.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(.5f, .5f);
            Destroy(statusImage, 5f);
        }
    }
}
