using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectHandler : MonoBehaviour
{
    private LivingBeing livingBeing;
    public Dictionary<StatusEffectType, int> SufferedStatusEffects { get; private set; } = new();

    void Awake()
    {
        InitializeStatusEffectDict();
    }
    void Start()
    {
        if (gameObject.TryGetComponent(out LivingBeing liv))
        {
            livingBeing = liv;
        }
    }
    public void HandleStatusEffect(StatusEffectType status, bool Add)
    {

    }
    public void AlterStatusEffectList(StatusEffectType status, bool Add) // modifies the list of abilities by which one is affected
    {
        if (Add) SufferedStatusEffects[status] += 1;
        if (!Add) SufferedStatusEffects[status] -= 1;
    }
    public bool HasStatusEffect(StatusEffectType status)
    {
        return SufferedStatusEffects[status] > 0;
    }

    public int GetStatusEffectValue(StatusEffectType status)
    {
        int statusValue = SufferedStatusEffects[status];
        Debug.Log($"Status effect {status} has value {statusValue}");

        return statusValue;// SufferedStatusEffects[status];
    }

    void InitializeStatusEffectDict()
    {
        SufferedStatusEffects = new Dictionary<StatusEffectType, int>();

        foreach (StatusEffectType effect in Enum.GetValues(typeof(StatusEffectType)))
        {
            SufferedStatusEffects[effect] = 0;
        }
    }


}
