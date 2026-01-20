using SummonSupportEvents;
using UnityEngine;

public class FootStepSoundInvoker : MonoBehaviour
{
    public void OnFootStep()
    {
        EventDeclarer.PlayerFootstep?.Invoke();
    }
}
