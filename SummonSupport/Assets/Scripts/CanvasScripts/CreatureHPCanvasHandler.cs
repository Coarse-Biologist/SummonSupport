using UnityEngine;
using UnityEngine.UI;

public class CreatureHPCanvasHandler : MonoBehaviour, I_ResourceBar
{

    private LivingBeing livingBeing;
    private Slider hpSlider;
    private Slider manaSlider;

    void Awake()
    {
        livingBeing = gameObject.GetComponent<LivingBeing>();
        Slider[] sliders = GetComponentsInChildren<Slider>();
        hpSlider = sliders[0];

        manaSlider = sliders[1];



        Logging.Info($"{manaSlider} = mana slider. {hpSlider} = hpslider");
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
        manaSlider.value = livingBeing.GetAttribute(AttributeType.CurrentPower);
    }

    public void SetPowerBarMaxValue(float value = 1)
    {
        manaSlider.maxValue = livingBeing.GetAttribute(AttributeType.MaxPower);
    }
}
