using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Beam Ability")]
public class BeamAbility : Ability //#TODO logic of toggle abilities is not supported yet by the potion reload animation system. null reference caused.
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
        int beamCount = 1 + GetExtraBeamCount(casterStats);
        GameObject beamGroup = new($"{Name} Beam");
        beamGroup.transform.SetParent(abilityDirection, false);
        beamGroup.transform.position = abilityDirection.position;

        for (int i = 0; i < beamCount; i++)
        {
            GameObject beam = SpawnBeam(abilityDirection);
            beam.name = $"{beamGroup.name} {i + 1}";
            beam.transform.SetParent(beamGroup.transform, true);
            beam.transform.localPosition = new Vector3(i * .35f, 0f, 0f);
            ConfigureBeam(beam.GetComponent<Beam>(), casterStats, abilityDirection);
        }

        return beamGroup;
    }

    private int GetExtraBeamCount(LivingBeing casterStats)
    {
        if (casterStats == null || casterStats.CharacterTag == CharacterTag.Enemy || AbilityModHandler.Instance == null)
            return 0;
        return Mathf.Max(0, AbilityModHandler.Instance.GetModAttributeByType(this, AbilityModTypes.Number));
    }

    private void ConfigureBeam(Beam beamScript, LivingBeing casterStats, Transform abilityDirection)
    {
        if (beamScript == null)
        {
            Debug.LogError($"{Name} tried to spawn a beam without a Beam component.");
            return;
        }
        beamScript.SetAbilitySettings(casterStats, this, abilityDirection);
    }

    private GameObject SpawnBeam(Transform abilityDirection)
    {
        return Instantiate(BeamParticleSystem, abilityDirection.position, abilityDirection.rotation, abilityDirection);
    }
}
