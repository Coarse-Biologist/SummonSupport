using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkEnemy : NetworkCombatant
{
    private readonly NetworkVariable<NetworkVitals> vitals = new();

    public ulong LastAttackerClientId { get; private set; } = NetworkManager.ServerClientId;

    public void RecordAttacker(ulong clientId)
    {
        LastAttackerClientId = clientId;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        vitals.OnValueChanged += OnVitalsChanged;
        if (!IsServer) ApplyVitals(vitals.Value);

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
        if (!IsServer || !IsSpawned) return;

        LivingBeing being = GetComponent<LivingBeing>();
        NetworkVitals current = new NetworkVitals
        {
            current = being.GetAttribute(AttributeType.CurrentHitpoints),
            max = being.GetAttribute(AttributeType.MaxHitpoints),
            dead = being.Dead
        };
        if (!current.SameAs(vitals.Value)) vitals.Value = current;
    }

    /// <summary>Host side: shows every client the loot this enemy dropped, so each player can roll their knowledge too.</summary>
    public void ShareDefeatLoot(Element strongestElement, float affinityValue, LootDropData[] drops)
    {
        if (!IsSpawned || !IsServer) return;
        DefeatLootClientRpc(strongestElement, affinityValue, drops);
    }

    [ClientRpc]
    private void DefeatLootClientRpc(Element strongestElement, float affinityValue, LootDropData[] drops)
    {
        if (IsServer || LootSpawnHandler.Instance == null) return;
        LootSpawnHandler.Instance.ApplyRemoteDefeatLoot(strongestElement, affinityValue, drops);
    }

    private void OnVitalsChanged(NetworkVitals previous, NetworkVitals next)
    {
        if (!IsServer) ApplyVitals(next);
    }

    private void ApplyVitals(NetworkVitals next)
    {
        GetComponent<LivingBeing>().ApplyReplicatedHitpoints(next.current, next.max);
    }
}
