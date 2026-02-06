using System.Linq;
using SummonSupportEvents;
using UnityEngine;

public class FootStepSoundInvoker : MonoBehaviour
{
    [field: SerializeField] public AudioClip[] PlayerFootSteps { private set; get; } = new AudioClip[5];
    private float FootstepVolume = 1f;
    private AudioSource audioSource;
    private void Start()
    {
        audioSource = gameObject.transform.parent.GetComponent<AudioSource>();
        if (audioSource == null || PlayerFootSteps.Length == 0)
        {
            throw new System.Exception("No AudioSource found on parent object for FootStepSoundInvoker.");
        }

    }

    public void SetAudioSource(AudioSource source)
    {
        audioSource = source;
    }
    public void OnFootstep()
    {
        audioSource.PlayOneShot(PlayerFootSteps[Random.Range(0, PlayerFootSteps.Length)], FootstepVolume * AudioHandler.Instance.GeneralGameVolume);
    }
}
