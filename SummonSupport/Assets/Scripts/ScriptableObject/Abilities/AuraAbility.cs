using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Aura Ability")]
public class AuraAbility : Ability
{
    [field: SerializeField, Header("Aura settings"), Tooltip("time in [seconds]")]
    public float Uptime { get; protected set; }
    [field: SerializeField]
    public float Radius { get; protected set; } = 1f;

    [field: SerializeField] public GameObject AuraObject { get; protected set; }
    [field: SerializeField] public bool LeaveRotation { get; protected set; } = true;

    public override bool Activate(GameObject caster)
    {
        if (caster.CompareTag("Player"))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            {
                if (hit.collider != null && hit.collider.TryGetComponent<LivingBeing>(out LivingBeing livingBeing))
                {
                    GameObject auraInstance = Instantiate(AuraObject, hit.collider.transform.position, AuraObject.transform.rotation, hit.collider.transform);
                    auraInstance.GetComponent<Aura>().SetAuraStats(caster.GetComponent<LivingBeing>(), livingBeing, this, Uptime);

                }
                else
                {
                    GameObject auraInstance = Instantiate(AuraObject, caster.transform.position, AuraObject.transform.rotation, caster.transform);
                    Aura auraMonoScript = auraInstance.GetComponent<Aura>();
                    if (auraMonoScript != null) auraMonoScript.HandleInstantiation(caster.GetComponent<LivingBeing>(), null, this, Radius, Uptime);
                }
            }
        }
        else
        {
            GameObject auraInstance = Instantiate(AuraObject, caster.transform.position, AuraObject.transform.rotation, caster.transform);
            Aura auraMonoScript = auraInstance.GetComponent<Aura>();
            if (auraMonoScript != null) auraMonoScript.HandleInstantiation(caster.GetComponent<LivingBeing>(), null, this, Radius, Uptime);
        }
        return true;
    }
}