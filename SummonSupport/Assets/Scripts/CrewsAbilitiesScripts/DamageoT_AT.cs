using UnityEngine;

[System.Serializable]
public class DamageoT_AT : Damage_AT
{
    [field: SerializeField] public float Duration { set; private get; } = 1f;
}
