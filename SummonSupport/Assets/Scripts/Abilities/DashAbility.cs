using UnityEngine;
[CreateAssetMenu(menuName = "Abilities/Dash Ability")]

public class DashAbility : ConjureAbility
{
    public override bool Activate(LivingBeing casterStats)
    {
        Transform originTransform = casterStats.transform;
        // find backward direction
        Vector3 spawnPosition = casterStats.transform.position - new Vector3(originTransform.right.x, originTransform.right.y, originTransform.right.z);
        Quaternion rotation = originTransform.rotation;
        //Debug.Log($"spawn position = {spawnPosition}");
        SpawnConjuredObject(casterStats, rotation, spawnPosition);
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
