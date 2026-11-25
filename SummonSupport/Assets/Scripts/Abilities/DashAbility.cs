using UnityEngine;
[CreateAssetMenu(menuName = "Abilities/Dash Ability")]

public class DashAbility : ConjureAbility
{
    public override bool Activate(LivingBeing casterStats)
    {
        Transform originTransform = casterStats.transform;
        Quaternion rotation = originTransform.rotation;
        int extraObjects = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Number, casterStats));

        for (int i = 0; i <= extraObjects; i++)
        {
            // find backward direction, staggering extra conjured objects behind the caster
            Vector3 spawnPosition = originTransform.position - originTransform.right * (1f + i * .5f);
            //Debug.Log($"spawn position = {spawnPosition}");
            SpawnConjuredObject(casterStats, rotation, spawnPosition);
        }
        return true;
    }
    protected void SpawnConjuredObject(LivingBeing user, Quaternion rotation, Vector3 position)
    {
        GameObject spawnedObject = Instantiate(ObjectToSpawn, position, rotation);

        Aura auraInChildren = spawnedObject.GetComponentInChildren<Aura>();
        if (auraInChildren != null)
        {
            auraInChildren.HandleInstantiation(user.GetComponent<LivingBeing>(), null, this);
        }
    }
}
