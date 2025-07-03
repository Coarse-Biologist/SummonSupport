using UnityEngine;

[System.Serializable]
public class Projectile_AT
{
    [field: SerializeField] public float Number { get; private set; } = 1f;
    [field: SerializeField] public float Speed { get; private set; } = 1f;
    [field: SerializeField] public float Range { get; private set; } = 10f;
    [field: SerializeField] public PathType PathType { get; private set; } = PathType.Straight;
    [field: SerializeField] public float SpreadAngle { get; private set; } = 10f;

}

public enum PathType
{
    Straight,
    Serpentine,
    Orbit,
}