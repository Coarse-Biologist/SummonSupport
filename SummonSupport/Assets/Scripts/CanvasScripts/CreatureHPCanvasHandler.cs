using UnityEngine;
using UnityEngine.UI;

public class CreatureHPCanvasHandler : MonoBehaviour, I_ResourceBar
{

    //private LivingBeing livingBeing;
    private Slider hpSlider;
    private Slider manaSlider;

    void Awake()
    {
        //livingBeing = GetComponent<LivingBeing>();
        Slider[] sliders = GetComponentsInChildren<Slider>();
        hpSlider = sliders[0];
        hpSlider.value = 200f;
        hpSlider.maxValue = 200f;
        manaSlider = sliders[1];
        manaSlider.value = 200f;
        manaSlider.maxValue = 200f;
    }


    public void SetHealthBarValue(float value)
    {
        hpSlider.value = value;
    }
    public void SetHealthBarMaxValue(float value)
    {
        hpSlider.maxValue = value;
    }

    public void SetPowerBarValue(float value)
    {
        manaSlider.value = value;
    }

    public void SetPowerBarMaxValue(float value)
    {
        manaSlider.maxValue = value;
    }
}
