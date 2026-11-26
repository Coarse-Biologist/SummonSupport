using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAbilityHandler))]
[RequireComponent(typeof(PlayerUIHandler))]
public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private string leftHandPath;
    [SerializeField] private string rightHandPath;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            AttachLocalPlayer();
            return;
        }

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerAbilityHandler>().enabled = false;
        GetComponent<PlayerUIHandler>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void AttachLocalPlayer()
    {
        PlayerStats.Instance = GetComponent<PlayerStats>();
        PlayerUIHandler.SetLocal(GetComponent<PlayerUIHandler>());

        foreach (CinemachineCamera cam in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
            cam.Follow = transform;

        UniversalAdditionalCameraData cameraData = Camera.main != null ? Camera.main.GetComponent<UniversalAdditionalCameraData>() : null;
        if (cameraData != null) cameraData.volumeTrigger = transform;

        Transform left = string.IsNullOrEmpty(leftHandPath) ? null : transform.Find(leftHandPath);
        Transform right = string.IsNullOrEmpty(rightHandPath) ? null : transform.Find(rightHandPath);
        foreach (AlchemyObjectInteraction bench in FindObjectsByType<AlchemyObjectInteraction>(FindObjectsSortMode.None))
            bench.SetPlayerHands(left, right);
    }
}
