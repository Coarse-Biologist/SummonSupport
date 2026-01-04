using SummonSupportEvents;
using UnityEngine;

public class SE_ObjectHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject ExplosionObject;
    [field: SerializeField] public GameObject IceCubeObject;
    [field: SerializeField] public GameObject VineGrabObject;
    [field: SerializeField] public GameObject PulledObject;
    [field: SerializeField] public GameObject VirusObject;
    [field: SerializeField] public GameObject PoisonedEffect;
    [field: SerializeField] public GameObject IonizedEffect;
    [field: SerializeField] public GameObject ChilledEffect;
    [field: SerializeField] public GameObject OverheatedEffect;
    [field: SerializeField] public GameObject MadnessEffect;
    [field: SerializeField] public GameObject CharmedEffect;
    [field: SerializeField] public GameObject PlantEffect;






    public static SE_ObjectHandler Instance;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    void OnEnable()
    {
        EventDeclarer.ViciousDeath.AddListener(ViciousDeathExplosion);
        EventDeclarer.FrozenSolid?.AddListener(FrozenSolid);
        EventDeclarer.GraspingVines?.AddListener(GraspingVines);
        EventDeclarer.SpreadVirus?.AddListener(SpreadVirus);
        EventDeclarer.IonizedAttack?.AddListener(ShowIonization);
        EventDeclarer.PlantAttack?.AddListener(ShowPlantEffect);
        EventDeclarer.Overheating?.AddListener(ShowOverheatingEffect);



    }
    void OnDisable()
    {
        EventDeclarer.ViciousDeath.RemoveListener(ViciousDeathExplosion);
        EventDeclarer.FrozenSolid?.RemoveListener(FrozenSolid);
        EventDeclarer.GraspingVines?.RemoveListener(GraspingVines);
        EventDeclarer.SpreadVirus?.RemoveListener(SpreadVirus);
        EventDeclarer.IonizedAttack?.RemoveListener(ShowIonization);
        EventDeclarer.PlantAttack?.RemoveListener(ShowPlantEffect);
        EventDeclarer.Overheating?.RemoveListener(ShowOverheatingEffect);



    }
    public void ViciousDeathExplosion(LivingBeing livingBeing)
    {
        if (ExplosionObject != null)
        {
            GameObject explosionInstance = Instantiate(ExplosionObject, livingBeing.transform.position, Quaternion.identity);
            if (explosionInstance.TryGetComponent(out ViciousDeathExplosion explodeScript))
            {
                explodeScript.CheckTargetToExplodeOn(livingBeing.CharacterTag);
            }
        }
    }
    public void SpreadVirus(LivingBeing livingBeing)
    {
        GameObject virusObjectInstance = Instantiate(VirusObject, livingBeing.transform.position, Quaternion.identity, livingBeing.transform);

        Destroy(virusObjectInstance, 5f);

        SpreadEffectScript newEffectScript = virusObjectInstance.GetComponent<SpreadEffectScript>();

        newEffectScript.HandleNewInstance(newEffectScript, null, livingBeing);
    }
    public void FrozenSolid(LivingBeing livingBeing)
    {
        if (ExplosionObject != null)
        {
            Vector3 spawnLoc = livingBeing.transform.position;

            GameObject IceCubeInstance = Instantiate(IceCubeObject, spawnLoc, Quaternion.identity);

            IceCubeInstance.transform.position = new Vector3(spawnLoc.x, spawnLoc.y - 1, spawnLoc.z);
            Destroy(IceCubeInstance, 5f);
        }
    }
    public void GraspingVines(LivingBeing livingBeing)
    {
        if (VineGrabObject != null)
        {
            Vector3 spawnLoc = new Vector3(livingBeing.transform.position.x, livingBeing.transform.position.y - 2, livingBeing.transform.position.z);
            GameObject vines = Instantiate(VineGrabObject, spawnLoc, Quaternion.identity);
            if (vines.TryGetComponent(out Animator anim))
            {
                anim.Play("VineGrab");
            }
        }
    }

    public void ShowIonization(LivingBeing livingBeing)
    {
        if (IonizedEffect != null)
        {
            GameObject instance = Instantiate(IonizedEffect, livingBeing.transform.position, Quaternion.identity, livingBeing.transform);
            Destroy(instance, .5f);
        }
    }
    public void ShowPlantEffect(Rigidbody rb)
    {
        if (PlantEffect != null)
        {
            GameObject instance = Instantiate(PlantEffect, rb.transform.position, Quaternion.identity, rb.transform);
            Destroy(instance, 3f);
        }
    }
    public void ShowOverheatingEffect(LivingBeing livingBeing)
    {
        if (OverheatedEffect != null)
        {
            GameObject instance = Instantiate(OverheatedEffect, livingBeing.transform.position, Quaternion.identity, livingBeing.transform);
            Destroy(instance, 6f);
        }
    }
}
