using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Beam Ability")]
public class BeamAbility : Ability
{
    [Header("Beam settings")]
    [field: SerializeField] public GameObject BeamParticleSystem { get; protected set; }
    //[field: SerializeField] public GameObject SpawnEffectOnHit { get; protected set; }
    [field: SerializeField] public float TickRate { get; private set; } = .5f;

    public override bool Activate(LivingBeing casterStats)
    {
        ToggleBeam(casterStats, casterStats.abilityHandler.abilitySpawn.transform);
        return true;
    }

    public GameObject ToggleBeam(LivingBeing casterStats, Transform abilityDirection)
    {
        Beam beamScript = SpawnBeam(abilityDirection).GetComponent<Beam>();
        //Debug.Log($"{abilityDirection} = ability direction object");

        beamScript.SetAbilitySettings(casterStats, this, abilityDirection);

        return beamScript.gameObject;
    }

    private GameObject SpawnBeam(Transform abilityDirection)
    {
        return Instantiate(BeamParticleSystem, abilityDirection.position, abilityDirection.rotation, abilityDirection);
    }
}
