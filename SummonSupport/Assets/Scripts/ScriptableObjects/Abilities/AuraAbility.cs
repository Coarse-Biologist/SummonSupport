using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Aura Ability")]
public class AuraAbility : Ability
{
    [field: SerializeField, Header("Aura settings"), Tooltip("time in [seconds]")]
    public float Uptime {get; protected set; }

    public override void Activate(GameObject user)
    {
        Logging.Info($"{user.name} applys aura");
    }
}