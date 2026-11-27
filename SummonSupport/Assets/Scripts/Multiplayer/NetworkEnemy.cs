using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Network side of every creature (enemies and minions). The host runs creature AI and combat, and sends
/// hitpoints to the clients.
///
/// Minions also belong to the player who crafted them (<see cref="CrafterClientId"/>). The host still runs a
/// minion's AI, which follows its crafter's player. The crafter's machine decides what the minion is
/// (MinionProfileData) and sends it to the host, which passes it on to everyone; the crafter commands,
/// recycles and upgrades its own minions through the host.
/// </summary>
public class NetworkEnemy : NetworkCombatant
{
    private const float ProfilePushInterval = 0.5f;

    private readonly NetworkVariable<NetworkVitals> vitals = new();

    // Set by the host before the minion spawns. Minions nobody crafted belong to the host.
    private readonly NetworkVariable<ulong> crafterClientId = new(NetworkManager.ServerClientId);
    // Matches a minion to the crafting request that made it, so the crafter knows which minion to build.
    private readonly NetworkVariable<uint> craftRequestId = new();

    private static readonly List<NetworkEnemy> spawnedMinions = new();

    private MinionStats minionStats;
    private bool craftStarted;
    private float profilePushTimer;
    private MinionProfileData? lastProfile;

    public ulong LastAttackerClientId { get; private set; } = NetworkManager.ServerClientId;

    public ulong CrafterClientId => crafterClientId.Value;

    private bool IsMinion => minionStats != null;

    /// <summary>True on the machine of the player who crafted this minion.</summary>
    private bool IsCraftedByLocalPlayer => IsSpawned && crafterClientId.Value == NetworkManager.LocalClientId;

    private void Awake()
    {
        minionStats = GetComponent<MinionStats>();
    }

    public void RecordAttacker(ulong clientId)
    {
        LastAttackerClientId = clientId;
    }

    /// <summary>Host side: who crafted this minion. Call before spawning it.</summary>
    public void SetCrafter(ulong clientId, uint requestId)
    {
        crafterClientId.Value = clientId;
        craftRequestId.Value = requestId;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        vitals.OnValueChanged += OnVitalsChanged;
        crafterClientId.OnValueChanged += OnCrafterChanged;
        craftRequestId.OnValueChanged += OnCraftRequestChanged;
        if (!IsServer) ApplyVitals(vitals.Value);

        if (IsMinion)
        {
            spawnedMinions.Add(this);
            TryStartCrafting();
            if (!IsServer && !IsCraftedByLocalPlayer) RequestMinionProfileServerRpc();
        }

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
        crafterClientId.OnValueChanged -= OnCrafterChanged;
        craftRequestId.OnValueChanged -= OnCraftRequestChanged;
        spawnedMinions.Remove(this);
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (IsServer) PushVitals();
        if (IsMinion && IsCraftedByLocalPlayer) PushMinionProfile();
    }

    private void PushVitals()
    {
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
        if (next.max <= 0) return; // the host hasn't sent this creature's hitpoints yet

        // A crafter decides its own minions' maximum hitpoints (see MinionProfileData).
        bool keepOwnMax = IsMinion && IsCraftedByLocalPlayer;
        GetComponent<LivingBeing>().ApplyReplicatedHitpoints(next.current, keepOwnMax ? (float?)null : next.max);

        // Clients show minions dying and being resurrected, so any player can resurrect them.
        if (!IsMinion) return;
        if (next.dead && !minionStats.Dead) minionStats.Die();
        else if (!next.dead && minionStats.Dead) minionStats.ApplyReplicatedResurrection();
    }

    #region Minion ownership

    /// <summary>True if the minion is the local player's: always offline, and online if they crafted it.</summary>
    public static bool IsLocalPlayersMinion(LivingBeing minion)
    {
        if (minion == null) return false;
        if (!NetworkSession.IsOnline || !minion.TryGetComponent(out NetworkEnemy networkEnemy) || !networkEnemy.IsSpawned) return true;
        return networkEnemy.IsCraftedByLocalPlayer;
    }

    /// <summary>The player a minion follows and protects: its crafter's player on the host, the local player otherwise.</summary>
    public static PlayerStats LeaderOf(LivingBeing minion)
    {
        if (minion != null && NetworkSession.IsOnline && NetworkManager.Singleton.IsServer
            && minion.TryGetComponent(out NetworkEnemy networkEnemy) && networkEnemy.IsSpawned)
        {
            NetworkObject crafterPlayer = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(networkEnemy.CrafterClientId);
            if (crafterPlayer != null && crafterPlayer.TryGetComponent(out PlayerStats crafterStats)) return crafterStats;
        }
        return PlayerStats.Instance;
    }

    /// <summary>The minions that share a leader with this minion, including itself.</summary>
    public static List<LivingBeing> FellowMinionsOf(LivingBeing minion)
    {
        List<LivingBeing> fellows = new();
        if (minion != null && NetworkSession.IsOnline && minion.TryGetComponent(out NetworkEnemy networkEnemy) && networkEnemy.IsSpawned)
        {
            foreach (NetworkEnemy other in spawnedMinions)
                if (other.CrafterClientId == networkEnemy.CrafterClientId) fellows.Add(other.minionStats);
            return fellows;
        }
        if (AlchemyHandler.Instance != null) fellows.AddRange(AlchemyHandler.Instance.activeMinions);
        return fellows;
    }

    // The host sets these before spawning, so they normally arrive with the minion; these cover a late arrival.
    private void OnCrafterChanged(ulong previous, ulong next)
    {
        if (IsMinion && IsCraftedByLocalPlayer && AlchemyHandler.Instance != null) AlchemyHandler.Instance.AddActiveMinion(minionStats);
        TryStartCrafting();
    }

    private void OnCraftRequestChanged(uint previous, uint next)
    {
        TryStartCrafting();
    }

    /// <summary>Crafter side: builds the minion the crafter asked for, once.</summary>
    private void TryStartCrafting()
    {
        if (craftStarted || !IsMinion || !IsCraftedByLocalPlayer || craftRequestId.Value == 0) return;
        craftStarted = true;
        StartCoroutine(FinishCraftingWhenReady());
    }

    private IEnumerator FinishCraftingWhenReady()
    {
        yield return null; // let the minion's Start run first
        if (AlchemyHandler.Instance != null) AlchemyHandler.Instance.CompleteNetworkCraft(minionStats, craftRequestId.Value);
    }

    #endregion

    #region Minion profile

    /// <summary>Crafter side: sends the minion's profile on whenever it changes.</summary>
    private void PushMinionProfile()
    {
        profilePushTimer -= Time.unscaledDeltaTime;
        if (profilePushTimer > 0f) return;
        profilePushTimer = ProfilePushInterval;

        MinionProfileData profile = MinionProfileData.Read(minionStats);
        if (lastProfile.HasValue && lastProfile.Value.Equals(profile)) return;
        lastProfile = profile;

        if (IsServer) MinionProfileClientRpc(profile);
        else SubmitMinionProfileServerRpc(profile);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitMinionProfileServerRpc(MinionProfileData profile, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != CrafterClientId) return;
        lastProfile = profile;
        ApplyMinionProfile(profile);
        MinionProfileClientRpc(profile);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMinionProfileServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!lastProfile.HasValue) return; // not crafted yet; the profile comes to everyone when it is
        ClientRpcParams toRequester = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
        };
        MinionProfileClientRpc(lastProfile.Value, toRequester);
    }

    [ClientRpc]
    private void MinionProfileClientRpc(MinionProfileData profile, ClientRpcParams rpcParams = default)
    {
        if (IsServer || IsCraftedByLocalPlayer) return;
        ApplyMinionProfile(profile);
    }

    private void ApplyMinionProfile(MinionProfileData profile)
    {
        profile.ApplyTo(minionStats);
    }

    /// <summary>Crafter side: fills the minion's hitpoints and power on the host, with the crafter's maximums.</summary>
    public void RequestRestoreResources()
    {
        if (!IsSpawned || IsServer || !IsMinion || !IsCraftedByLocalPlayer) return;
        lastProfile = MinionProfileData.Read(minionStats);
        RestoreResourcesServerRpc(lastProfile.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestoreResourcesServerRpc(MinionProfileData profile, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != CrafterClientId) return;
        lastProfile = profile;
        ApplyMinionProfile(profile);
        minionStats.RestoreResources();
        MinionProfileClientRpc(profile);
    }

    #endregion

    #region Minion commands, recycling and resurrection

    /// <summary>Crafter side: sends a command for this minion to the host, which runs its AI.</summary>
    public void SendCommand(MinionCommands command, Vector3 location, LivingBeing target)
    {
        if (!IsSpawned) return;
        NetworkObjectReference targetReference = default;
        if (target != null && target.TryGetComponent(out NetworkObject targetObject) && targetObject.IsSpawned) targetReference = targetObject;
        CommandServerRpc(command, location, targetReference);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CommandServerRpc(MinionCommands command, Vector3 location, NetworkObjectReference targetReference, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != CrafterClientId) return;

        LivingBeing target = null;
        if (targetReference.TryGet(out NetworkObject targetObject)) target = targetObject.GetComponent<LivingBeing>();
        if (command == MinionCommands.FocusTarget && target == null) return;

        CommandMinion.ApplyCommand(minionStats, command, location, target);
    }

    /// <summary>Crafter side: has the host end this minion after the crafter recycled it.</summary>
    public void RequestRecycle()
    {
        if (IsSpawned) RecycleServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RecycleServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != CrafterClientId) return;
        minionStats.Die();
    }

    /// <summary>Any player can resurrect a dead minion; the host does it, and everyone sees it.</summary>
    public void RequestResurrect()
    {
        if (IsSpawned) ResurrectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResurrectServerRpc()
    {
        if (minionStats.Dead) minionStats.Resurrect();
    }

    #endregion
}
