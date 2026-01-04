using UnityEngine;

public interface I_ResourceBar
{
    public void SetHealthBarValue(float value);
    public void SetHealthBarMaxValue(float value);
    public void SetPowerBarValue(float value);
    public void SetPowerBarMaxValue(float value);
    public void AddStatusEffectSymbol(StatusEffects status, int stacks);
}
