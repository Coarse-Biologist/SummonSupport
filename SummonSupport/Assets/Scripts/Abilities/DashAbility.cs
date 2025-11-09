using UnityEngine;
[CreateAssetMenu(menuName = "Abilities/Dash Ability")]

public class DashAbility : ConjureAbility
{
    public override bool Activate(GameObject user)
    {
        Transform originTransform = user.transform;
        // find backward direction
        Vector2 spawnPosition = (Vector2)user.transform.position - new Vector2(originTransform.right.x, originTransform.right.y);
        Quaternion rotation = originTransform.rotation;
        //Debug.Log($"spawn position = {spawnPosition}");
        return Activate(user, spawnPosition, rotation);
    }
}
