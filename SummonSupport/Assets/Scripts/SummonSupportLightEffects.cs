using System;
using System.Collections;
using UnityEngine;

public class SummonSupportLightEffects : MonoBehaviour
{
    [field: SerializeField] new public Light light;
    [field: SerializeField] public int flashStrength = 10000;
    private WaitForSeconds waitTime = new WaitForSeconds(.05f);
    public bool flashing = false;
    public float flashDelay = 5f;
    public float counter = 0f;


    void Start()
    {
        InvokeRepeating("StartFlashOfLightningCoroutine", 1f, 1f);
    }
    private void StartFlashOfLightningCoroutine()
    {
        counter += 1f;
        if (flashing) return;
        if (flashDelay > counter) return;

        else
        {
            StartCoroutine(FlashOfLightning());
            counter = 0;
        }
    }

    private IEnumerator FlashOfLightning()
    {
        flashing = true;
        float flashDuration = UnityEngine.Random.Range(.2f, 1f);
        while (flashDuration > 0)
        {
            light.intensity *= flashDuration * flashStrength * Math.Abs((float)Math.Cos(flashDuration));
            yield return waitTime;
            flashDuration -= .05f;
        }
        light.intensity = 1;
        flashDelay = UnityEngine.Random.Range(5f, 11f);
        flashing = false;
    }

    private void DimLight()
    {
        light.intensity = 1;
    }
}
