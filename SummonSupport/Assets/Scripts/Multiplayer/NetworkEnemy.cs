using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkEnemy : NetworkBehaviour
{
    private readonly NetworkVariable<NetworkVitals> vitals = new();
    private LivingBeing being;

    private void Awake()
    {
        being = GetComponent<LivingBeing>();
    }

    public override void OnNetworkSpawn()
    {
        vitals.OnValueChanged += OnVitalsChanged;
        if (IsServer) PushVitals();
        else ApplyVitals(vitals.Value);

        if (IsServer) return;

        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is NetworkBehaviour) continue;
            if (behaviour.GetType().Name.StartsWith("AI")) behaviour.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null) body.isKinematic = true;
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
        NetworkVitals current = NetworkVitals.From(being);
        if (!current.SameAs(vitals.Value)) vitals.Value = current;
    }

    private void OnVitalsChanged(NetworkVitals previous, NetworkVitals next)
    {
        if (!IsServer) ApplyVitals(next);
    }

    private void ApplyVitals(NetworkVitals next)
    {
        being.ApplyReplicatedHitpoints(next.current, next.max);
    }
}
