using System.Collections.Generic;
using UnityEngine;

public class SpawnLocationInfo : MonoBehaviour
{
    [field: SerializeField] public int desiredSpawns { get; private set; } = 3;
    [field: SerializeField] public float radius { get; private set; } = 3;
    [field: SerializeField] public List<GameObject> Creatures { get; private set; } = null;
    [field: SerializeField] public bool MoveTowardLocation { get; private set; } = false;
    [field: SerializeField] public Transform TargetLocation { get; private set; } = null;
}
