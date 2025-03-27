using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Heal Ability")]
public class HealAbility : Ability
{
    [field: SerializeField, Header("Heal settings"), Tooltip("Ability prefab")]
    public float HealAmount { get; protected set; }


    public override void Activate(GameObject user)
    {
        Logging.Info($"{user.name} heals {HealAmount} HP!");
    }
}