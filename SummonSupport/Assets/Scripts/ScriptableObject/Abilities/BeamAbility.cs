using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Beam Ability")]
public class BeamAbility : Ability
{
    [Header("Beam settings")]
    [field: SerializeField] public GameObject BeamParticleSystem { get; protected set; }
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; protected set; }
    [field: SerializeField] public float TickRate { get; private set; } = .5f;

    public override bool Activate(GameObject user)
    {
        throw new System.NotImplementedException();
    }

    public GameObject ToggleBeam(GameObject user, Transform abilityDirection)
    {
        Beam beamScript = SpawnBeam(user, abilityDirection).GetComponent<Beam>();
        //Debug.Log($"{abilityDirection} = ability direction object");

        beamScript.SetAbilitysettings(user.GetComponent<LivingBeing>(), this, abilityDirection);

        return beamScript.gameObject;
    }

    private GameObject SpawnBeam(GameObject user, Transform abilityDirection)
    {
        return Instantiate(BeamParticleSystem, user.transform.position, abilityDirection.rotation, abilityDirection);
    }
}
