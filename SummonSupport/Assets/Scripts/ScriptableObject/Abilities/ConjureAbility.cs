using System.Collections.Generic;
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
    [field: SerializeField] public bool SeeksTarget { get; protected set; } = false;
    [field: SerializeField] public float Speed { get; private set; } = 2f;
    [field: SerializeField] public float SearchRadius { get; private set; } = 10f;



    public override bool Activate(GameObject user)
    {
        Debug.Log("Activating conjure ability");
        Vector3 spawnPosition = user.transform.position + (Vector3)SpawnOffset;
        Quaternion rotation = user.transform.rotation;
        return Activate(user, spawnPosition, rotation);
    }

    public bool Activate(GameObject user, Vector3 spawnPosition, Quaternion rotation)
    {
        LivingBeing casterStats = user.GetComponent<LivingBeing>();

        int targetNum = 1;
        float duration = Duration;

        if (user.TryGetComponent(out AbilityModHandler modHandler))
        {
            targetNum += modHandler.GetModAttributeByType(this, AbilityModTypes.Number);
            duration += modHandler.GetModAttributeByType(this, AbilityModTypes.Duration);
        }
        TeamType desiredTargetType = this.GetTargetPreference(casterStats);

        List<LivingBeing> targets = GetTargetfromSphereCast(user.GetComponent<AbilityHandler>().abilitySpawn.transform, targetNum, desiredTargetType);
        foreach (LivingBeing target in targets)
        {
            Debug.Log($"{target.Name} found in GetTarget from spherecast function");

            Quaternion newRotation = Quaternion.identity;
            if (!LeaveRotation)
                newRotation = rotation * Quaternion.Euler(0, 0, RotationOffset);

            Vector3 SpawnLoc = target.transform.position + SpawnOffset;

            GameObject spawnedObject = Instantiate(ObjectToSpawn, SpawnLoc, newRotation);

            Aura auraInChildren = spawnedObject.GetComponentInChildren<Aura>();
            if (auraInChildren != null)
            {
                auraInChildren.HandleInstantiation(user.GetComponent<LivingBeing>(), target, this);
            }
        }

        return true;
    }



}