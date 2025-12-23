using SummonSupportEvents;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject ExplosionObject;
    public static ExplosionHandler Instance;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    void OnEnable()
    {
        EventDeclarer.ViciousDeath.AddListener(ViciousDeathExplosion);
    }
    void OnDisable()
    {
        EventDeclarer.ViciousDeath.RemoveListener(ViciousDeathExplosion);
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
}
