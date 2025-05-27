using UnityEngine;

[System.Serializable]
public class Projectile_AT
{
    [field: SerializeField] public float Speed { set; private get; } = 1f;
    [field: SerializeField] public float Range { set; private get; } = 1f;

}
