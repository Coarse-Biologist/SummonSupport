using UnityEngine;

public class SummonSupportLightEffects : MonoBehaviour
{
    [field: SerializeField] new public Light light;

    void Start()
    {
        InvokeRepeating("FlashOfLightning", 5f, 5f);
    }

    private void FlashOfLightning()
    {
        light.intensity *= 100;
        Invoke("DimLight", .2f);
    }

    private void DimLight()
    {
        light.intensity = 1;
    }
}
