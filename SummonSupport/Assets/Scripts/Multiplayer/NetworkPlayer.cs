using System;
using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAbilityHandler))]
[RequireComponent(typeof(PlayerUIHandler))]
[RequireComponent(typeof(AbilityUI_Handler))]
public class NetworkPlayer : NetworkCombatant
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

    // The host's values are the real ones for current hitpoints and power.
    // Maximums and regeneration rates are the owner's: they come from its own level-ups and upgrades,
    // so the owner sends them to the host (SubmitResourceProfileServerRpc) instead of taking them from it.
    private readonly NetworkVariable<NetworkVitals> vitals = new();
    private readonly NetworkVariable<NetworkPower> power = new();

    public override void OnNetworkSpawn()
    {
        vitals.OnValueChanged += OnVitalsChanged;
        power.OnValueChanged += OnPowerChanged;
        if (!IsServer)
        {
            ApplyVitals(vitals.Value);
            ApplyPower(power.Value);
        }

        if (IsOwner)
        {
            AttachLocalPlayer();
            return;
        }

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerAbilityHandler>().enabled = false;
        GetComponent<PlayerUIHandler>().enabled = false;
        GetComponent<AbilityUI_Handler>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public override void OnNetworkDespawn()
    {
        vitals.OnValueChanged -= OnVitalsChanged;
        power.OnValueChanged -= OnPowerChanged;
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (IsServer) PushVitals();
        if (!IsOwner) return;

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
        if (next.dead && IsOwner && !stats.Dead) stats.Die();
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
