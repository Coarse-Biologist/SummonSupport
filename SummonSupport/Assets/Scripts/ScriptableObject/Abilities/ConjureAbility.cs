using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Conjure Ability")]
public class ConjureAbility : Ability
{
    [field: Header("Conjure settings"), Tooltip("Ability prefab")]
    [field: SerializeField] public GameObject ObjectToSpawn { get; protected set; }
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; set; } = null;

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
        CharacterTag desiredTag = CharacterTag.Enemy;
        if (user.TryGetComponent(out EnemyStats enemyStats))
        {
            desiredTag = CharacterTag.Minion;
        }

        int targetNum = 1;
        float duration = Duration;
        if (user.TryGetComponent(out AbilityModHandler modHandler))
        {
            targetNum += modHandler.GetModAttributeByType(this, AbilityModTypes.ProjectileNumber);
            duration += modHandler.GetModAttributeByType(this, AbilityModTypes.Duration);

        }

        List<LivingBeing> targets = GetTargetfromSphereCast(this, user.GetComponent<AbilityHandler>().abilitySpawn.transform, targetNum, desiredTag);
        int targetsFound = 0;
        foreach (LivingBeing target in targets)
        {
            if (targetsFound < targetNum)
            {
                Quaternion newRotation = Quaternion.identity;
                if (!LeaveRotation)
                    newRotation = rotation * Quaternion.Euler(0, 0, RotationOffset);
                GameObject spawnedObject = Instantiate(ObjectToSpawn, target.transform.position, newRotation);

                if (IsDecaying)
                    Destroy(spawnedObject, duration);

                Aura auraInChildren = spawnedObject.GetComponentInChildren<Aura>();
                if (auraInChildren != null)
                {
                    auraInChildren.HandleInstantiation(user.GetComponent<LivingBeing>(), null, this);
                }
            }
            else return true;

        }



        return true;
    }



}