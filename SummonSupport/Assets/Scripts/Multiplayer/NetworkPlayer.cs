using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAbilityHandler))]
[RequireComponent(typeof(PlayerUIHandler))]
[RequireComponent(typeof(AbilityUI_Handler))]
public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private string leftHandPath;
    [SerializeField] private string rightHandPath;

    private readonly NetworkVariable<NetworkVitals> vitals = new();
    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public override void OnNetworkSpawn()
    {
        vitals.OnValueChanged += OnVitalsChanged;

        if (IsOwner) AttachLocalPlayer();
        else
        {
            GetComponent<PlayerMovement>().enabled = false;
            GetComponent<PlayerAbilityHandler>().enabled = false;
            GetComponent<PlayerUIHandler>().enabled = false;
            GetComponent<AbilityUI_Handler>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        if (IsServer) PushVitals();
        else ApplyVitals(vitals.Value);
    }

    public override void OnNetworkDespawn()
    {
        vitals.OnValueChanged -= OnVitalsChanged;
    }

    private void Update()
    {
        if (IsServer && IsSpawned) PushVitals();
    }

    private void PushVitals()
    {
        NetworkVitals current = NetworkVitals.From(stats);
        if (!current.SameAs(vitals.Value)) vitals.Value = current;
    }

    private void OnVitalsChanged(NetworkVitals previous, NetworkVitals next)
    {
        if (!IsServer) ApplyVitals(next);
    }

    private void ApplyVitals(NetworkVitals next)
    {
        stats.ApplyReplicatedHitpoints(next.current, next.max);
        if (next.dead && IsOwner && !stats.Dead) stats.Die();
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
