using UnityEngine;
using UnityEngine.UI;

public class CreatureHPCanvasHandler : MonoBehaviour, I_HealthBar
{

    //private LivingBeing livingBeing;
    private Slider hpSlider;

    void Awake()
    {
        //livingBeing = GetComponent<LivingBeing>();
        hpSlider = GetComponentInChildren<Slider>();
        hpSlider.value = 200f;
        hpSlider.maxValue = 200f;
    }


    public void SetHealthBarValue(float value)
    {
        hpSlider.value = value;
    }
    public void SetHealthBarMaxValue(float value)
    {
        hpSlider.maxValue = value;
    }
}
