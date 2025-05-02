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
        Logging.Info($"I_HealthBar is awake");
    }


    public void SetHealthBarValue(float value)
    {
        Logging.Info($"assigned value of hp =  {value}!!!!!!!!!!");
        hpSlider.value = value;
    }
    public void SetHealthBarMaxValue(float value)
    {
        Logging.Info($"assigned value of  max hp =  {value}");

        hpSlider.maxValue = value;
    }
}
