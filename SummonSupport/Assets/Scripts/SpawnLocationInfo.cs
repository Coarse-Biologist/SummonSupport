using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public class SpawnLocationInfo : MonoBehaviour
{
    [field: SerializeField] public int MinimumCreaturesPerWave { get; private set; } = 3; //This will be added to a number depending on game difficulty
    [field: SerializeField] public int Waves { get; private set; } = 3;
    [field: SerializeField] public int SecondsPerWave { get; private set; } = 20;
    [field: SerializeField] public float Radius { get; private set; } = 3;
    [field: SerializeField] public bool MoveTowardLocation { get; private set; } = false;
    [field: SerializeField] public int ElementChance { get; private set; } = 10; // out of 100, chance to spawn with an element.
    [field: SerializeField] public float CreatureStrengthScalar { get; private set; } = 2f; //modifies affinities.
    [field: SerializeField] public Element PreferedElement { get; private set; } = Element.None;
    [field: SerializeField] public PhysicalType PreferedPhysical { get; private set; } = PhysicalType.None;

    [field: SerializeField] public GameObject[] Creatures { get; private set; } = new GameObject[3];
    [field: SerializeField] public Transform TargetLocation { get; private set; } = null;
    [field: SerializeField] public SpawnTrigger Trigger { get; private set; } = null;



}
