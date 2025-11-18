using UnityEngine;
[CreateAssetMenu(menuName = "Abilities/Dash Ability")]

public class DashAbility : ConjureAbility
{
    public override bool Activate(GameObject user)
    {
        Transform originTransform = user.transform;
        // find backward direction
        Vector3 spawnPosition = user.transform.position - new Vector3(originTransform.right.x, originTransform.right.y, originTransform.right.z);
        Quaternion rotation = originTransform.rotation;
        //Debug.Log($"spawn position = {spawnPosition}");
        return Activate(user, spawnPosition, rotation);
    }
}
