using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class LightManager
{

    private static WaitForSeconds waitTime = new WaitForSeconds(.5f);
    public static IEnumerator MakeLightOscillate(Light2D light)
    {
        bool small = false;
        while (light != null)
        {
            if (small)
            {
                light.intensity += .4f;
                light.falloffIntensity += .2f;
                light.color = Color.aliceBlue;
                small = false;
            }
            else
            {
                light.intensity -= .4f;
                light.falloffIntensity -= .2f;
                light.color = Color.orange;
                small = true;
            }

            yield return waitTime;
        }
    }



}
