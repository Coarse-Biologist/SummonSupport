using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Aura : MonoBehaviour
{
    LivingBeing caster;
    Ability ability;
    List<LivingBeing> listLivingBeingsInAura = new();
    [SerializeField] public float ActivationTime { get; private set; } = .1f;
    private bool Active = false;



    public void HandleInstantiation(LivingBeing caster, Ability ability, float radius)
    {
        GetComponent<CircleCollider2D>().radius = radius;
        this.caster = caster;
        this.ability = ability;
        Invoke("Activate", ActivationTime);
    }
    public void SetAuraStats(GameObject caster, AuraAbility ability)
    {
        this.caster = caster.GetComponent<LivingBeing>();
        this.ability = ability;
        SetAuraTimer(ability.Uptime);
        //transform.Rotate(new Vector3(-110f, 0, 0));

    }

    private void Activate()
    {
        Active = true;
    }

    public void SetAuraTimer(float timeUp)
    {
        Destroy(gameObject, timeUp);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (Active)
        {
            Logging.Info($"{other.gameObject} has entered the bite zone");
            if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))

                if (ability.ListUsableOn.Contains(RelationshipHandler.GetRelationshipType(caster.CharacterTag, otherLivingBeing.CharacterTag)) && !listLivingBeingsInAura.Contains(otherLivingBeing))
                {
                    CombatStatHandler.HandleEffectPackages(ability, caster, otherLivingBeing, false);
                }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        LivingBeing otherLivingBeing = other.gameObject.GetComponent<LivingBeing>();
        if (listLivingBeingsInAura.Contains(otherLivingBeing))
        {         //possibly remove aura effect? 
        }
    }


    void OnDestroy()
    {
        //foreach (LivingBeing livingBeing in listLivingBeingsInAura.ToList())
        // remove status effects ?
    }
}
