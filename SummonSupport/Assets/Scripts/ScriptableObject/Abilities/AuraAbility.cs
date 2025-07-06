using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Aura Ability")]
public class AuraAbility : Ability
{
    [field: SerializeField, Header("Aura settings"), Tooltip("time in [seconds]")]
    public float Uptime { get; protected set; }
    [field: SerializeField] public GameObject AuraObject { get; protected set; }

    public bool ActivateAura(GameObject caster, Vector2 TargetLoc)
    {
        RaycastHit2D hit = Physics2D.Raycast(TargetLoc, Vector2.zero);

        if (hit.collider != null && hit.collider.GetComponent<MinionStats>() != null)
        {
            GameObject auraInstance = Instantiate(AuraObject, hit.collider.transform.position, Quaternion.identity, hit.collider.transform);
            auraInstance.GetComponent<Aura>().SetAuraStats(hit.collider.gameObject, this);
        }
        else
        {
            GameObject auraInstance = Instantiate(AuraObject, caster.transform.position, Quaternion.identity, caster.transform);
            Logging.Info($"aura instance = {auraInstance}");
            Aura auraMonoScript = auraInstance.GetComponent<Aura>();
            if (auraMonoScript != null) auraMonoScript.SetAuraStats(caster, this);
            else Logging.Info($"auraMonoScript = {auraMonoScript}");
        }


        Logging.Info($"{caster.GetComponent<LivingBeing>().name} applys aura");
        return true;
    }

    public override bool Activate(GameObject user)
    {
        throw new System.NotImplementedException();
    }
}