using UnityEngine;

[System.Serializable]
public class Projectile_AT
{
    [field: SerializeField] public float Speed { get; private set; } = 1f;
    [field: SerializeField] public float Range { get; private set; } = 1f;

}
