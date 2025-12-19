using SummonSupportEvents;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject ExplosionObject;
    public static ExplosionHandler Instance;

    void Awake()
    {
        if (Instance != null) Destroy(this);
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
            GameObject instance = Instantiate(ExplosionObject, livingBeing.transform.position, Quaternion.identity);
            if (instance.TryGetComponent(out ViciousDeathExplosion explodeScript))
            {
                explodeScript.CheckTargetToExplodeOn(livingBeing.CharacterTag);
            }
        }
    }
}
