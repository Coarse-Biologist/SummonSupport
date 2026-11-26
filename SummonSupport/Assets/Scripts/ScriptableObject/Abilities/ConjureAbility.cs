using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Conjure Ability")]
public class ConjureAbility : Ability
{
    [field: Header("Conjure settings"), Tooltip("Ability prefab")]
    [field: SerializeField] public GameObject ObjectToSpawn { get; protected set; }

    [field: SerializeField] public Vector3 SpawnOffset { get; protected set; }
    [field: SerializeField] public float RotationOffset { get; protected set; } = 0;
    [field: SerializeField] public float Radius = 1f;


    [field: Tooltip("Activate this if ability should only last a specific amount of time")]
    [field: SerializeField] public bool IsDecaying { get; protected set; }
    [field: SerializeField] public bool LeaveRotation { get; protected set; }



    [field: Header("Tracking Settings"), Tooltip("in seconds")]
    [field: SerializeField] public bool SpawnsOnTarget { get; protected set; } = false;
    [field: SerializeField] public bool SeeksTarget { get; protected set; } = false;
    [field: SerializeField] public float Speed { get; private set; } = 2f;
    [field: SerializeField] public float SearchRadius { get; private set; } = 10f;

    protected Transform CurrentAuraGroup { get; set; }

    protected Transform GetAuraGroup(LivingBeing casterStats)
    {
        if (ObjectToSpawn == null || ObjectToSpawn.GetComponentInChildren<Aura>() == null) return null;
        if (CurrentAuraGroup == null) CurrentAuraGroup = Aura.CreateAuraGroup(casterStats, this);
        return CurrentAuraGroup;
    }

    public override bool Activate(LivingBeing casterStats)
    {
        Quaternion rotation = casterStats.transform.rotation;
        return Activate(casterStats, rotation);
    }

    public bool Activate(LivingBeing casterStats, Quaternion rotation)
    {
        CurrentAuraGroup = null;

        int targetNum = 1;
        int sizeMod = 0;
        int speedMod = 0;

        if (casterStats != null && casterStats.CharacterTag != CharacterTag.Enemy)
        {
            targetNum += Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Number, casterStats));
            sizeMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Size, casterStats));
            speedMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Speed, casterStats));
        }
        float castRadius = 4f + sizeMod * .5f;
        float castRange = Range + speedMod * .5f;

        if (SpawnsOnTarget)
        {
            TeamType desiredTargetType = this.GetTargetPreference(casterStats);

            List<LivingBeing> targets = GetTargetfromSphereCast(casterStats, casterStats.abilityHandler.abilitySpawn.transform, targetNum, desiredTargetType, castRadius, castRange);
            foreach (LivingBeing target in targets)
            {
                SpawnOnTarget(casterStats, target, rotation);
            }
        }
        else for (int i = targetNum; i > 0; i--)
        {
            SpawnConjuredObject(casterStats, rotation, i);
        }


        return true;
    }

    private void SpawnOnTarget(LivingBeing casterStats, LivingBeing target, Quaternion rotation)
    {
        Quaternion newRotation = Quaternion.identity;
        if (!LeaveRotation)
            newRotation = rotation * Quaternion.Euler(0, 0, RotationOffset);

        Vector3 SpawnLoc = target.transform.position + SpawnOffset;

        Transform auraParent = GetAuraGroup(casterStats);
        GameObject spawnedObject = Instantiate(ObjectToSpawn, SpawnLoc, newRotation, auraParent);

        Aura auraInChildren = spawnedObject.GetComponentInChildren<Aura>();
        if (auraInChildren != null)
        {
            auraInChildren.HandleInstantiation(casterStats, target, this);
        }
    }
    protected void SpawnConjuredObject(LivingBeing casterStats, Quaternion rotation, int iterator)
    {
        Quaternion newRotation = Quaternion.identity;
        if (!LeaveRotation)
            newRotation = rotation * Quaternion.Euler(0, 0, RotationOffset);

        float spawnDistance = Range + Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Speed, casterStats)) * .5f;
        Vector3 SpawnLoc = casterStats.transform.position + casterStats.transform.forward * spawnDistance;
        Vector3 newSpawnOffset = new Vector3(SpawnOffset.x, SpawnOffset.y, SpawnOffset.z + iterator);
        Transform auraParent = GetAuraGroup(casterStats);
        GameObject spawnedObject = Instantiate(ObjectToSpawn, SpawnLoc + newSpawnOffset, newRotation, auraParent);

        Aura auraInChildren = spawnedObject.GetComponentInChildren<Aura>();
        if (auraInChildren != null)
        {
            auraInChildren.HandleInstantiation(casterStats, null, this);

        }
    }




}