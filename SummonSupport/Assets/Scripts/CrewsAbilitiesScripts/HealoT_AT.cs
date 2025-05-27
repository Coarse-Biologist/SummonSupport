using UnityEngine;

[System.Serializable]
public class HealoT_AT : Heal_AT
{
    [field: SerializeField] public float Duration { set; private get; } = 1f;
}
