using UnityEngine;
[System.Serializable]
public class Heal_AT
{
    [field: SerializeField] public float Value { get; private set; } = 0f;

    public void Mod_HealValue(float heal_Change)
    {
        Value += heal_Change;
    }

}
