using SummonSupportEvents;
using UnityEngine;

public class FootStepSoundInvoker : MonoBehaviour
{
    [field: SerializeField] public AudioClip[] PlayerFootSteps { private set; get; } = new AudioClip[5];
    private AudioSource audioSource;
    private AnimationControllerScript anim;
    private void Start()
    {
        anim = gameObject.transform.parent.GetComponent<AnimationControllerScript>();
        audioSource = gameObject.transform.parent.GetComponent<AudioSource>();
        if (audioSource == null || PlayerFootSteps.Length == 0)
        {
            throw new System.Exception("No AudioSource found on parent object for FootStepSoundInvoker.");
        }
        if (anim == null)
        {
            throw new System.Exception("No animation controller script found on parent object.");
        }

    }

    public void OnFootstep()
    {
        audioSource.PlayOneShot(PlayerFootSteps[Random.Range(0, PlayerFootSteps.Length)], AudioHandler.Instance.FootstepVolume * AudioHandler.Instance.GeneralGameVolume);
    }

    public void TriggerPotionReturnToBelt()
    {
        if (NetworkSession.IsOnline && transform.root.GetComponent<PlayerStats>() != PlayerStats.Instance) return;
        PotionHandler.ReturnPotionToBelt();
    }

    public void ResetAnimLayerWeight()
    {
        anim.SeLayerWeight(1, 0);
    }
}
