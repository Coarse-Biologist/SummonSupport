using System;
using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAbilityHandler))]
[RequireComponent(typeof(PlayerUIHandler))]
[RequireComponent(typeof(AbilityUI_Handler))]
public partial class NetworkPlayer : NetworkCombatant
{
    [SerializeField] private string leftHandPath;
    [SerializeField] private string rightHandPath;

    private static readonly Element[] Elements = (Element[])Enum.GetValues(typeof(Element));
    private static readonly PhysicalType[] PhysicalTypes = (PhysicalType[])Enum.GetValues(typeof(PhysicalType));
    private const float CombatProfilePushInterval = 0.5f;
    private float pushTimer;
    private float[] lastAffinities;
    private float[] lastPhysicals;
    private Vector4? lastResourceProfile;

    // Host side: the pause menu is shared, so the host keeps whether it is open for everyone.
    private static bool sharedPaused;
    private static string sharedPausedBy;

    // The host's values are the real ones for current hitpoints and power.
    // Maximums and regeneration rates are the owner's: they come from its own level-ups and upgrades,
    // so the owner sends them to the host (SubmitResourceProfileServerRpc) instead of taking them from it.
    private readonly NetworkVariable<NetworkVitals> vitals = new();
    private readonly NetworkVariable<NetworkPower> power = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        vitals.OnValueChanged += OnVitalsChanged;
        power.OnValueChanged += OnPowerChanged;
        if (!IsServer)
        {
            ApplyVitals(vitals.Value);
            ApplyPower(power.Value);
        }

        if (IsServer)
        {
            if (IsOwner) sharedPaused = false; // the host's own player spawning starts a new session
            else if (sharedPaused) SharedPauseClientRpc(true, sharedPausedBy, ToOwner());
            if (!IsOwner)
            {
                SendOpenDoorsToOwner();
                SendQuestProgressToOwner();
            }
        }

        StartSaveSession();

        if (IsOwner)
        {
            AttachLocalPlayer();
            // Mods belong to everyone in the game, so a joining player takes the ones made before they joined.
            if (!IsServer) RequestAllModsServerRpc();
            return;
        }

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerAbilityHandler>().enabled = false;
        GetComponent<PlayerUIHandler>().enabled = false;
        GetComponent<AbilityUI_Handler>().enabled = false;
        GetComponent<InteractHandler>().enabled = false; // doors, benches and NPCs react to the local player only
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public override void OnNetworkDespawn()
    {
        vitals.OnValueChanged -= OnVitalsChanged;
        power.OnValueChanged -= OnPowerChanged;
        // If the one player still standing leaves, everyone left is down.
        if (IsServer && !IsOwner) CheckAllPlayersDown(this);
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (IsServer) PushVitals();
        if (!IsOwner) return;
        UpdateSaves();

        pushTimer -= Time.unscaledDeltaTime;
        if (pushTimer > 0f) return;
        pushTimer = CombatProfilePushInterval;

        PlayerStats stats = GetComponent<PlayerStats>();
        if (!IsServer) PushResourceProfile(stats);

        float[] affinities = Elements.Select(e => (float)stats.GetAffinity(e)).ToArray();
        float[] physicals = PhysicalTypes.Select(p => stats.PhysicalDict.TryGetValue(p, out var entry) ? entry.Get() : 0f).ToArray();

        if (lastAffinities != null && affinities.SequenceEqual(lastAffinities) && physicals.SequenceEqual(lastPhysicals)) return;

        lastAffinities = affinities;
        lastPhysicals = physicals;
        SubmitCombatProfileServerRpc(affinities, physicals);
    }

    private void PushVitals()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        NetworkVitals current = new NetworkVitals
        {
            current = stats.GetAttribute(AttributeType.CurrentHitpoints),
            max = stats.GetAttribute(AttributeType.MaxHitpoints),
            dead = stats.Dead
        };
        if (!current.SameAs(vitals.Value)) vitals.Value = current;

        NetworkPower currentPower = new NetworkPower
        {
            current = stats.GetAttribute(AttributeType.CurrentPower),
            max = stats.GetAttribute(AttributeType.MaxPower),
            lastHandledCastSequence = LastHandledCastSequence
        };
        if (!currentPower.SameAs(power.Value)) power.Value = currentPower;
    }

    private void OnVitalsChanged(NetworkVitals previous, NetworkVitals next)
    {
        if (!IsServer) ApplyVitals(next);
    }

    private void ApplyVitals(NetworkVitals next)
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        stats.ApplyReplicatedHitpoints(next.current, IsOwner ? (float?)null : next.max);
        // Every machine shows a downed player, so anyone nearby can resurrect them.
        if (next.dead && !stats.Dead) stats.Die();
        else if (!next.dead && stats.Dead) stats.ApplyReplicatedResurrection();
    }

    /// <summary>Any player or minion can resurrect a downed player; the host does it, and everyone sees it.</summary>
    public void RequestResurrect()
    {
        if (IsSpawned) ResurrectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResurrectServerRpc()
    {
        GetComponent<PlayerStats>().Resurrect();
    }

    /// <summary>The living player closest to a position: any connected player online, the local player offline. Null if all are down.</summary>
    public static PlayerStats NearestLivingPlayer(Vector3 position)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (!NetworkSession.IsOnline || networkManager == null || !networkManager.IsServer)
            return PlayerStats.Instance != null && !PlayerStats.Instance.Dead ? PlayerStats.Instance : null;

        PlayerStats nearest = null;
        float nearestSqr = float.MaxValue;
        foreach (NetworkClient client in networkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null || !client.PlayerObject.TryGetComponent(out PlayerStats stats) || stats.Dead) continue;
            float sqr = (stats.transform.position - position).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = stats;
            }
        }
        return nearest;
    }

    /// <summary>Host side: shows the death screen to everyone once every player in the session is down.</summary>
    public static void CheckAllPlayersDown(NetworkPlayer leaving = null)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsServer) return;

        NetworkPlayer messenger = null;
        foreach (NetworkClient client in networkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null || !client.PlayerObject.TryGetComponent(out NetworkPlayer player) || player == leaving) continue;
            if (!player.GetComponent<PlayerStats>().Dead) return;
            messenger ??= player;
        }
        if (messenger != null) messenger.AllPlayersDownClientRpc();
    }

    [ClientRpc]
    private void AllPlayersDownClientRpc()
    {
        EventDeclarer.PlayerDead?.Invoke(true);
    }

    private void OnPowerChanged(NetworkPower previous, NetworkPower next)
    {
        if (!IsServer) ApplyPower(next);
    }

    private void ApplyPower(NetworkPower next)
    {
        if (next.max <= 0) return; // the host hasn't sent this player's power yet
        PlayerStats stats = GetComponent<PlayerStats>();
        if (IsOwner)
        {
            // The owner already paid for casts the host hasn't handled yet; keep them paid.
            stats.ApplyReplicatedPower(next.current - PendingCastCost(next.lastHandledCastSequence), null);
        }
        else stats.ApplyReplicatedPower(next.current, next.max);
    }

    /// <summary>Owner side: sends the owner's maximums and regeneration rates to the host when they change.</summary>
    private void PushResourceProfile(PlayerStats stats)
    {
        Vector4 profile = ReadResourceProfile(stats);
        if (lastResourceProfile.HasValue && lastResourceProfile.Value == profile) return;
        lastResourceProfile = profile;
        SubmitResourceProfileServerRpc(profile);
    }

    private static Vector4 ReadResourceProfile(PlayerStats stats)
    {
        return new Vector4(
            stats.GetAttribute(AttributeType.MaxHitpoints),
            stats.GetAttribute(AttributeType.MaxPower),
            stats.HealthRegeneration,
            stats.PowerRegeneration);
    }

    [ServerRpc]
    private void SubmitResourceProfileServerRpc(Vector4 profile)
    {
        ApplyResourceProfile(profile);
    }

    private void ApplyResourceProfile(Vector4 profile)
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        if (profile.x > 0) stats.SetAttribute(AttributeType.MaxHitpoints, profile.x);
        if (profile.y > 0) stats.SetAttribute(AttributeType.MaxPower, profile.y);

        // Change the base rates by the difference, so temporary changes on the host (beam drain, heal and damage
        // over time) stay in the totals.
        float healthRegenChange = profile.z - stats.HealthRegeneration;
        if (healthRegenChange != 0) stats.ChangeHealthRegeneration(healthRegenChange);
        float powerRegenChange = profile.w - stats.PowerRegeneration;
        if (powerRegenChange != 0) stats.ChangePowerRegeneration(powerRegenChange);
    }

    /// <summary>
    /// Owner side: fills hitpoints and power on the host, for example after a level-up. Sends the owner's current
    /// maximums with it, so the host fills them up to the new values.
    /// </summary>
    public void RequestRestoreResources()
    {
        if (!IsSpawned || !IsOwner || IsServer) return;
        PlayerStats stats = GetComponent<PlayerStats>();
        lastResourceProfile = ReadResourceProfile(stats);
        RestoreResourcesServerRpc(lastResourceProfile.Value);
    }

    [ServerRpc]
    private void RestoreResourcesServerRpc(Vector4 profile)
    {
        ApplyResourceProfile(profile);
        GetComponent<PlayerStats>().RestoreResources();
    }

    [ServerRpc]
    public void SubmitCombatProfileServerRpc(float[] affinities, float[] physicals)
    {
        if (affinities.Length != Elements.Length || physicals.Length != PhysicalTypes.Length) return;

        PlayerStats stats = GetComponent<PlayerStats>();
        for (int i = 0; i < Elements.Length; i++)
            if (Elements[i] != Element.None && stats.Affinities.ContainsKey(Elements[i])) stats.SetAffinity(Elements[i], affinities[i]);

        for (int i = 0; i < PhysicalTypes.Length; i++)
            if (PhysicalTypes[i] != PhysicalType.None && stats.PhysicalDict.ContainsKey(PhysicalTypes[i])) stats.SetPhysicalResist(PhysicalTypes[i], physicals[i]);
    }

    [ServerRpc]
    public void SubmitAttributeModServerRpc(int abilityIndex, int modType, float changeValue)
    {
        Ability ability = AbilityRegistry.AbilityAt(abilityIndex);
        if (ability == null) return;

        AbilityModHandler.Instance.ModAttributeByType(ability, (AbilityModTypes)modType, changeValue);
        BroadcastAttributeModClientRpc(abilityIndex, modType, changeValue);
    }

    [ClientRpc]
    public void BroadcastAttributeModClientRpc(int abilityIndex, int modType, float changeValue)
    {
        if (IsServer) return;

        Ability ability = AbilityRegistry.AbilityAt(abilityIndex);
        if (ability == null) return;
        AbilityModHandler.Instance.ModAttributeByType(ability, (AbilityModTypes)modType, changeValue);
    }

    [ServerRpc]
    public void SubmitStatusModServerRpc(int abilityIndex, int effectIndex)
    {
        Ability ability = AbilityRegistry.AbilityAt(abilityIndex);
        StatusEffects effect = AbilityRegistry.StatusEffectAt(effectIndex);
        if (ability == null || effect == null) return;

        AbilityModHandler.Instance.AddStatusEffectToAbility(ability, effect);
        BroadcastStatusModClientRpc(abilityIndex, effectIndex);
    }

    [ClientRpc]
    public void BroadcastStatusModClientRpc(int abilityIndex, int effectIndex)
    {
        if (IsServer) return;

        Ability ability = AbilityRegistry.AbilityAt(abilityIndex);
        StatusEffects effect = AbilityRegistry.StatusEffectAt(effectIndex);
        if (ability == null || effect == null) return;
        AbilityModHandler.Instance.AddStatusEffectToAbility(ability, effect);
    }

    /// <summary>Owner side: shows this player's dash dust to everyone else.</summary>
    public void ShareDashDust(Vector3 position, Quaternion rotation)
    {
        if (!IsSpawned || !IsOwner) return;
        if (IsServer) DashDustClientRpc(position, rotation);
        else DashDustServerRpc(position, rotation);
    }

    [ServerRpc]
    private void DashDustServerRpc(Vector3 position, Quaternion rotation)
    {
        GetComponent<PlayerMovement>().SpawnDashDust(position, rotation);
        DashDustClientRpc(position, rotation);
    }

    [ClientRpc]
    private void DashDustClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsServer || IsOwner) return;
        GetComponent<PlayerMovement>().SpawnDashDust(position, rotation);
    }

    /// <summary>Owner side: asks the host for a dropped loot object this player touched.</summary>
    public void RequestLoot(uint lootId)
    {
        if (!IsSpawned || !IsOwner) return;
        if (IsServer) HandleLootRequest(lootId);
        else RequestLootServerRpc(lootId);
    }

    [ServerRpc]
    private void RequestLootServerRpc(uint lootId)
    {
        HandleLootRequest(lootId);
    }

    private void HandleLootRequest(uint lootId)
    {
        // The first player to ask gets it; later requests for the same loot find it gone.
        if (LootSpawnHandler.Instance == null || !LootSpawnHandler.Instance.IsLootAvailable(lootId)) return;
        LootSpawnHandler.Instance.CollectLoot(lootId);
        LootCollectedClientRpc(lootId);
    }

    /// <summary>Picked-up loot goes into every player's own inventory.</summary>
    [ClientRpc]
    private void LootCollectedClientRpc(uint lootId)
    {
        if (IsServer || LootSpawnHandler.Instance == null) return;
        LootSpawnHandler.Instance.CollectLoot(lootId);
    }

    [ServerRpc]
    private void RequestAllModsServerRpc()
    {
        if (AbilityModHandler.Instance == null) return;
        NetworkAbilityMods.CollectAllMods(out AbilityAttributeModData[] attributeMods, out AbilityStatusModData[] statusMods);

        ClientRpcParams toOwner = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        };
        AllModsClientRpc(attributeMods, statusMods, toOwner);
    }

    [ClientRpc]
    private void AllModsClientRpc(AbilityAttributeModData[] attributeMods, AbilityStatusModData[] statusMods, ClientRpcParams rpcParams = default)
    {
        if (IsServer || !IsOwner || AbilityModHandler.Instance == null) return;
        NetworkAbilityMods.ApplyAllMods(attributeMods, statusMods);
    }

    /// <summary>Owner side: asks the host to spawn a minion this player crafted (see AlchemyHandler).</summary>
    public void RequestCraftMinion(Vector3 spawnPosition, uint requestId)
    {
        if (!IsSpawned || !IsOwner) return;
        if (IsServer) SpawnCraftedMinion(spawnPosition, requestId);
        else RequestCraftMinionServerRpc(spawnPosition, requestId);
    }

    [ServerRpc]
    private void RequestCraftMinionServerRpc(Vector3 spawnPosition, uint requestId)
    {
        SpawnCraftedMinion(spawnPosition, requestId);
    }

    private void SpawnCraftedMinion(Vector3 spawnPosition, uint requestId)
    {
        if (AlchemyHandler.Instance == null) return;
        AlchemyHandler.Instance.SpawnNetworkedMinion(spawnPosition, OwnerClientId, requestId);
    }

    /// <summary>Owner side: opens or closes the pause menu for everyone, through the host.</summary>
    public void RequestSharedPause(bool open)
    {
        if (!IsSpawned || !IsOwner) return;
        if (IsServer) SetSharedPause(open);
        else RequestSharedPauseServerRpc(open);
    }

    [ServerRpc]
    private void RequestSharedPauseServerRpc(bool open)
    {
        SetSharedPause(open);
    }

    private void SetSharedPause(bool open)
    {
        sharedPaused = open;
        sharedPausedBy = GetComponent<PlayerStats>().Name;
        SharedPauseClientRpc(open, sharedPausedBy);
    }

    [ClientRpc]
    private void SharedPauseClientRpc(bool open, string pausedBy, ClientRpcParams rpcParams = default)
    {
        PauseGameHandler.ApplyPauseMenu(open, pausedBy);
    }

    private ClientRpcParams ToOwner()
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        };
    }

    public void GrantXp(int amount, ulong clientId)
    {
        ClientRpcParams targetOwner = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        GrantXpClientRpc(amount, targetOwner);
    }

    [ClientRpc]
    public void GrantXpClientRpc(int amount, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        GetComponent<PlayerStats>().GainXP(amount);
    }

    /// <summary>
    /// Makes this the local player: camera, post-processing volume trigger, player UI and alchemy bench hands.
    /// Online it runs when the owner's player spawns; offline SetupManager calls it for the player it creates.
    /// </summary>
    public void AttachLocalPlayer()
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
