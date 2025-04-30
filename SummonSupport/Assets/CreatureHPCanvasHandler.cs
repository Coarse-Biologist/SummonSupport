using SummonSupportEvents;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class CreatureHPCanvasHandler : MonoBehaviour, I_HealthBar
{

    private LivingBeing livingBeing;
    private Slider hpSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        livingBeing = GetComponent<LivingBeing>();
        hpSlider = GetComponentInChildren<Slider>();
        hpSlider.maxValue = livingBeing.GetAttribute(AttributeType.MaxHitpoints);
        hpSlider.value = livingBeing.GetAttribute(AttributeType.CurrentHitpoints);
    }


    public void SetHealthBarValue()
    {
        hpSlider.value = livingBeing.GetAttribute(AttributeType.CurrentHitpoints);
    }
    public void SetHealthBarMaxValue()
    {
        hpSlider.maxValue = livingBeing.GetAttribute(AttributeType.MaxHitpoints);
    }
}
