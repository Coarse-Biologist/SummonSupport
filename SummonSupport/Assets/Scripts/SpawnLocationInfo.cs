using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public class SpawnLocationInfo : MonoBehaviour
{
    [field: SerializeField] public int CreaturesPerWave { get; private set; } = 3;
    [field: SerializeField] public int Waves { get; private set; } = 3;
    [field: SerializeField] public int SecondsPerWave { get; private set; } = 20;
    [field: SerializeField] public float Radius { get; private set; } = 3;
    [field: SerializeField] public bool MoveTowardLocation { get; private set; } = false;

    [field: SerializeField] public Element PreferedElement { get; private set; } = Element.None;
    [field: SerializeField] public PhysicalType PreferedPhysical { get; private set; } = PhysicalType.None;

    [field: SerializeField] public List<GameObject> Creatures { get; private set; } = null;
    [field: SerializeField] public Transform TargetLocation { get; private set; } = null;
    [field: SerializeField] public SpawnTrigger Trigger { get; private set; } = null;



}
