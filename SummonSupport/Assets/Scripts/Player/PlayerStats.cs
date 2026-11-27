using System;
using System.Collections;
using System.Collections.Generic;
using SummonSupportEvents;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerStats : LivingBeing
{
    public static PlayerStats Instance;

    [Header("Experience Info")]

    #region Experience Variables
    [field: SerializeField] public int CurrentLevel { private set; get; } = 1;
    [field: SerializeField] public float CurrentXP { private set; get; } = 0;
    [field: SerializeField] public float MaxXP { private set; get; } = 100;
    [field: SerializeField] public int SkillPoints { private set; get; } = 100;

    #endregion

    #region Ressurrection Variables
    [SerializeField] public float ResurrectTime { private set; get; } = 5f;
    [SerializeField] public float ResurrectRange { private set; get; } = 2f;
    private WaitForSeconds resurrectionIncrement = new WaitForSeconds(.5f);

    #endregion
    [field: SerializeField] public int TotalControlllableMinions { private set; get; } = 2;
    public PlayerUIHandler UiHandler { private set; get; }

    #region player speciic transforms;
    [field: SerializeField] public Transform HandTransform { private set; get; }
    [field: SerializeField] public List<Transform> AbilityPotionTransformList { private set; get; }


    #endregion

    #region start game object life
    protected override void Awake()
    {
        base.Awake();
        if (!NetworkSession.IsOnline) Instance = this;
        UiHandler = GetComponent<PlayerUIHandler>();
    }

    void OnEnable()
    {
        EventDeclarer.EnemyDefeated?.AddListener(GainXP);
    }
    void OnDisable()
    {
        EventDeclarer.EnemyDefeated?.RemoveListener(GainXP);

    }
    #endregion

    public override void RestoreResources()
    {
        base.RestoreResources();
        // Online, the host's hitpoints and power are the real ones, so it has to fill them too.
        if (NetworkSession.IsOnline && TryGetComponent(out NetworkPlayer networkPlayer)) networkPlayer.RequestRestoreResources();
    }

        public void AddControllableMinions(int changeValue)
    {
        TotalControlllableMinions = Math.Max(0, TotalControlllableMinions + changeValue);
        Debug.Log($"Changing total controllable minion number"); //#TODO check  to see if this is being implimented at the crafting station
    }

    #region xp and level 
    private void GainXP(LivingBeing defeatedEnemy)
    {
        if (NetworkSession.IsOnline) return;
        GainXP((int)defeatedEnemy.XP_OnDeath);
    }
    public void GainXP(int amount)
    {
        int XpToGain = amount;
        //Debug.Log($"Xp to gain = {XpToGain}");
        while (XpToGain > 0) // While we have enough XP to level up
        {
            CurrentXP += XpToGain;
            if (CurrentXP > MaxXP)
            {
                XpToGain = (int)(CurrentXP - MaxXP);
                CurrentXP = MaxXP;
            }
            else XpToGain = 0;

            if (CurrentXP == MaxXP) LevelUp();
        }

        if (PlayerUIHandler.Instance != null) PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);
    }
    public void SetXp(int xp)
    {
        CurrentXP = xp;
        if (PlayerUIHandler.Instance != null) PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);

    }
    public void SetMaxXp(int xp)
    {
        MaxXP = xp;
        if (PlayerUIHandler.Instance != null) PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);

    }

    private void LevelUp()
    {
        CurrentLevel += 1;
        CurrentXP = 0;
        MaxXP *= 2;
        SkillPoints++;
        EventDeclarer.PlayerLevelUp?.Invoke(LevelUpHandler.GetLevelRewardString(CurrentLevel));
        LevelUpHandler.GetLevelRewards(CurrentLevel);
    }
    public void SetLevel(int level)
    {
        CurrentLevel = level;
    }
    public void SetSkillPoints(int value)
    {
        SkillPoints = value;
    }
    public void ExpendSkillPoints(int value = 1)
    {
        SkillPoints -= value;
    }

    public void GainLevelRewards(List<LevelRewards> rewards)
    {
        foreach (LevelRewards reward in rewards)
        {
            switch (reward)
            {
                case LevelRewards.SkillPoint:
                    SkillPoints++;
                    break;
                case LevelRewards.MaximumHealth:
                    ChangeAttribute(AttributeType.MaxHitpoints, GetAttribute(AttributeType.MaxHitpoints) / 100);
                    RestoreResources();
                    break;
                case LevelRewards.MaximumPower:
                    ChangeAttribute(AttributeType.MaxPower, GetAttribute(AttributeType.MaxHitpoints) / 100);
                    RestoreResources();
                    break;
                case LevelRewards.HealthRegeneration:
                    ChangeHealthRegeneration(1);
                    break;
                case LevelRewards.PowerRegeneration:
                    ChangePowerRegeneration(1);
                    break;
                case LevelRewards.TotalControlllableMinions:
                    AddControllableMinions(1);
                    break;
                case LevelRewards.ElementalAffinity:
                    ChangeAffinity(GetAffinityRewardElement(), 10);
                    break;
                default:
                    Debug.LogWarning($"There is no behavior implimented for the level up reward {reward}");
                    break;
            }
        }
    }
    /// <summary>
    /// The element the affinity level-up reward goes to: the player's strongest affinity, or with no affinity yet,
    /// the element they know most about, or with no knowledge either, a random element.
    /// </summary>
    private Element GetAffinityRewardElement()
    {
        Element element = GetHighestAffinity(out float _);
        if (element != Element.None) return element;

        int bestKnowledge = 0;
        foreach (KeyValuePair<Element, int> kvp in AlchemyInventory.knowledgeDict)
        {
            if (kvp.Value > bestKnowledge && Affinities.ContainsKey(kvp.Key))
            {
                bestKnowledge = kvp.Value;
                element = kvp.Key;
            }
        }
        if (element != Element.None) return element;

        List<Element> elements = new();
        foreach (Element candidate in Affinities.Keys)
            if (candidate != Element.None) elements.Add(candidate);
        return elements.Count > 0 ? elements[UnityEngine.Random.Range(0, elements.Count)] : Element.None;
    }

    public void GainLevelRewards(Dictionary<LevelRewards, int> rewards)
    {
        foreach (var reward in rewards)
        {
            if (rewards[reward.Key] != 0)
            {
                switch (reward.Key)
                {
                    case LevelRewards.SkillPoint:
                        SkillPoints++;
                        break;
                    case LevelRewards.MaximumHealth:
                        ChangeAttribute(AttributeType.MaxHitpoints, GetAttribute(AttributeType.MaxHitpoints) * reward.Value / 100);
                        RestoreResources();
                        break;
                    case LevelRewards.MaximumPower:
                        ChangeAttribute(AttributeType.MaxPower, GetAttribute(AttributeType.MaxHitpoints) * reward.Value / 100);
                        RestoreResources();
                        break;
                    case LevelRewards.HealthRegeneration:
                        ChangeHealthRegeneration(1 * reward.Value);
                        break;
                    case LevelRewards.PowerRegeneration:
                        ChangePowerRegeneration(1 * reward.Value);
                        break;
                    case LevelRewards.TotalControlllableMinions:
                        AddControllableMinions(1 * reward.Value);
                        break;
                    case LevelRewards.ElementalAffinity:
                        ChangeAffinity(GetAffinityRewardElement(), 10 * reward.Value);
                        break;
                    default:
                        Debug.LogWarning($"There is no behavior implimented for the level up reward {reward}");
                        break;
                }
            }
        }
    }
    #endregion

    #region death and resurrection handling
    // A player who falls is downed: any living player or a commanded minion can resurrect them
    // (I_InteractResurrect). The death screen only comes once every player is down.
    public override void Die()
    {
        if (Dead) return;
        SetDead(true);
        abilityHandler.StopAllToggledAbilities();
        SetDowned(true);
        if (this == Instance && NetworkSession.IsOnline && FloatingInfoHandler.Instance != null)
            FloatingInfoHandler.Instance.DisplayGoldenLetters("You have fallen. An ally or a minion can resurrect you.", 4f);
        if (NetworkSession.IsAuthority) Invoke(nameof(DelayedDeath), 2f);
    }

    private void DelayedDeath()
    {
        if (!Dead) return;
        if (NetworkSession.IsOnline) NetworkPlayer.CheckAllPlayersDown();
        else EventDeclarer.PlayerDead?.Invoke(true);
    }

    /// <summary>Brings a downed player back. Online the host does it, and every machine sees it (see NetworkPlayer).</summary>
    public void Resurrect()
    {
        if (!Dead) return;
        if (!NetworkSession.IsAuthority)
        {
            if (TryGetComponent(out NetworkPlayer networkPlayer)) networkPlayer.RequestResurrect();
            return;
        }
        CancelInvoke(nameof(DelayedDeath));
        SetDead(false);
        SetDowned(false);
        SetAttribute(AttributeType.CurrentHitpoints, Mathf.Max(1, GetAttribute(AttributeType.MaxHitpoints) / 2));
    }

    /// <summary>Client side: shows a resurrection the host made. The host also sends the new hitpoints.</summary>
    public void ApplyReplicatedResurrection()
    {
        SetDead(false);
        SetDowned(false);
    }

    private const float DownedInteractRadius = 2.5f;
    private SphereCollider downedTrigger;

    // A downed player gets a trigger around them, so the living can reach them to resurrect them.
    private void SetDowned(bool downed)
    {
        if (downed)
        {
            if (!TryGetComponent(out I_InteractResurrect _)) gameObject.AddComponent<I_InteractResurrect>();
            if (downedTrigger == null)
            {
                downedTrigger = gameObject.AddComponent<SphereCollider>();
                downedTrigger.isTrigger = true;
                downedTrigger.radius = DownedInteractRadius;
            }
        }
        else
        {
            if (TryGetComponent(out I_InteractResurrect resurrectScript)) Destroy(resurrectScript);
            if (downedTrigger != null) Destroy(downedTrigger);
            downedTrigger = null;
        }
    }


    #endregion

    public override void HandleUIAttrDisplay(AttributeType attributeType, float newValue)
    {
        if (UiHandler != null && UiHandler.isActiveAndEnabled) UiHandler.UpdateResourceBar(this, attributeType);
        switch (attributeType)
        {
            case AttributeType.MaxHitpoints:
                resourceBarInterface?.SetHealthBarMaxValue(GetAttribute(attributeType));
                break;

            case AttributeType.CurrentHitpoints:
                resourceBarInterface?.SetHealthBarValue(GetAttribute(attributeType));
                break;

            case AttributeType.MaxPower:
                resourceBarInterface?.SetPowerBarMaxValue(GetAttribute(attributeType));
                break;

            case AttributeType.CurrentPower:
                resourceBarInterface?.SetPowerBarValue(GetAttribute(attributeType));
                break;
            default:
                break;

        }
    }
    public override string GetLivingBeingStats()
    {

        string statString = "";
        statString += $"Max hitpoints: {GetAttribute(AttributeType.MaxHitpoints)}\n";
        statString += $"Max Power: {GetAttribute(AttributeType.MaxPower)}\n";
        statString += $"Health regeneration: {TotalHealthRegeneration}\n";
        statString += $"Power regeneration: {TotalPowerRegeneration}\n";

        statString += GetComponent<AbilityHandler>().GetKnownAbilitiesString();
        foreach (var kvp in Affinities)
        {
            if (kvp.Value.Get() > 0)
                statString += $"{kvp.Key} affinity: {kvp.Value.Get()}\n";
        }

        return statString;

    }


}
