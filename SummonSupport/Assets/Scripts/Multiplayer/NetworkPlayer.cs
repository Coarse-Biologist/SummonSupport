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

    private readonly NetworkVariable<NetworkVitals> vitals = new();

    public override void OnNetworkSpawn()
    {
        vitals.OnValueChanged += OnVitalsChanged;
        if (!IsServer) ApplyVitals(vitals.Value);

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
    }

    private void OnVitalsChanged(NetworkVitals previous, NetworkVitals next)
    {
        if (!IsServer) ApplyVitals(next);
    }

    private void ApplyVitals(NetworkVitals next)
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        stats.ApplyReplicatedHitpoints(next.current, next.max);
        if (next.dead && IsOwner && !stats.Dead) stats.Die();
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
