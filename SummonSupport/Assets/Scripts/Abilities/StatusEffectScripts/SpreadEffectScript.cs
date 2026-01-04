using System;
using System.Linq;
using UnityEngine;

public class SpreadEffectScript : MonoBehaviour
{
    [field: SerializeField] public Ability abilityPackageToSpread;
    [field: SerializeField] public Element element;


    [field: SerializeField] public int duration = 5;

    private LivingBeing[] livingBeings = new LivingBeing[3];
    private LivingBeing source;
    private LivingBeing thisBeing;
    private int beingsInfected = 0;

    public void SetSource(LivingBeing c)
    {
        source = c;
    }
    public void SetandEffectBeing(LivingBeing b)
    {
        thisBeing = b;

        thisBeing.ChangeHealthRegeneration_Limitless(-1);

    }
    void InfectArea()
    {
        Debug.Log($"Spreading the effect in an area centered oon {gameObject}");
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Enemy"));
        foreach (Collider collider in rangeChecks)
        {
            if (beingsInfected == 3) break;

            if (collider.gameObject.TryGetComponent(out LivingBeing being) && being.CharacterTag == CharacterTag.Enemy)
            {
                if (being == source) continue;
                if (being == thisBeing) continue;
                //bool hasThree = Array.IndexOf(numbers, 3) >= 0;
                if (Array.IndexOf(livingBeings, being) < 0)
                {
                    livingBeings[beingsInfected] = being;
                    beingsInfected += 1;
                    GameObject newEffectInstance = Instantiate(this.gameObject, being.transform.position, Quaternion.identity, being.transform);

                    Destroy(newEffectInstance, duration);

                    SpreadEffectScript newEffectScript = newEffectInstance.GetComponent<SpreadEffectScript>();

                    HandleNewInstance(newEffectScript, thisBeing, being);
                }
            }
        }
    }

    public void HandleNewInstance(SpreadEffectScript newScriptInstance, LivingBeing sourceBeing, LivingBeing recieverBeing)
    {
        newScriptInstance.SetandEffectBeing(recieverBeing);
        newScriptInstance.SetSource(sourceBeing);
        newScriptInstance.InvokeRepeating("InfectArea", 0f, .5f);
    }


}
