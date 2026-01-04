using SummonSupportEvents;
using UnityEngine;

public class SE_ObjectHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject ExplosionObject;
    [field: SerializeField] public GameObject IceCubeObject;
    [field: SerializeField] public GameObject VineGrabObject;
    [field: SerializeField] public GameObject VirusObject;



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

    }
    void OnDisable()
    {
        EventDeclarer.ViciousDeath.RemoveListener(ViciousDeathExplosion);
        EventDeclarer.FrozenSolid?.RemoveListener(FrozenSolid);

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
        Instantiate(VirusObject, livingBeing.transform.position, Quaternion.identity, livingBeing.transform);
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
                Debug.Log("Congrats, kneeler");
                anim.Play("VineGrab");
            }
            else Debug.Log("You wish, kneeler");
        }
    }
}
