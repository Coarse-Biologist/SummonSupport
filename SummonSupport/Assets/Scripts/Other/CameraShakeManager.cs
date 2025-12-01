using SummonSupportEvents;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { private set; get; }
    [field: SerializeField] public CinemachineCamera virtualCamera { private set; get; }

    private CinemachineBasicMultiChannelPerlin cameraShaker;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }
    private void Start()
    {
        if (virtualCamera == null) throw new System.Exception("Camera not assigned to camera shaker script.");
        if (virtualCamera.TryGetComponent(out CinemachineBasicMultiChannelPerlin cameraShakeComponent))
        {
            Debug.Log("Mission accomplished");
            cameraShaker = cameraShakeComponent;
        }
    }
    private void OnEnable()
    {
        EventDeclarer.ShakeCamera?.AddListener(ShakeCamera);
    }

    private void OnDisable()
    {
        EventDeclarer.ShakeCamera?.AddListener(ShakeCamera);

    }
    public void ShakeCamera(float shakeAmplitude)
    {
        cameraShaker.AmplitudeGain += shakeAmplitude;
        Invoke("ResetShake", .3f);
    }
    private void ResetShake()
    {
        cameraShaker.AmplitudeGain = 0;
    }

}
