using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile Ability")]
public class ProjectileAbility : Ability
{
    [field: SerializeField, Header("Projectile settings")]
                            public float                Speed               {get; protected set; }
    [field: SerializeField] public float                Direction           {get; protected set; }
    [field: SerializeField] public float                MaxRange            {get; protected set; }
    [field: SerializeField] public float                Lifetime            {get; protected set; }
    [field: SerializeField] public PiercingBehaviour    PiercingMode        {get; protected set; } //Pass through, break on first hit, break on x hit, 
    [field: SerializeField] public OnEventDo            OnHit               {get; protected set; } // nothing, effect, cast, 
    [field: SerializeField] public OnEventDo            OnDestroy           {get; protected set; } // nothing, effect, cast, 


    public override void Activate(GameObject user)
    {
        Logging.Info($"{user.name} casted a {Name} HP!");
    }
}