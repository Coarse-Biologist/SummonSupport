using SummonSupportEvents;
using Unity.VisualScripting;
using UnityEngine;

public class SE_ObjectHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject ExplosionObject;
    [field: SerializeField] public GameObject IceCubeObject;

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
}
