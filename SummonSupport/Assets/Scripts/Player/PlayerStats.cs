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
    [field: SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    public PlayerUIHandler UiHandler { private set; get; }

    #region player speciic transforms;
    [field: SerializeField] public Transform HandTransform { private set; get; }
    [field: SerializeField] public List<Transform> AbilityPotionTransformList { private set; get; }


    #endregion

    #region start game object life
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
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

    public void AddControllableMinions(int changeValue)
    {
        TotalControlllableMinions = Math.Max(0, TotalControlllableMinions + changeValue);
        Debug.Log($"Changing total controllable minion number"); //#TODO check  to see if this is being implimented at the crafting station
    }

    #region xp and level 
    private void GainXP(LivingBeing defeatedEnemy)
    {
        GainXP((int)defeatedEnemy.XP_OnDeath);
    }
    public void GainXP(int amount)
    {
        int XpToGain = amount;
        Debug.Log($"Xp to gain = {XpToGain}");
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

        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);
    }
    public void SetXp(int xp)
    {
        CurrentXP = xp;
        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);

    }
    public void SetMaxXp(int xp)
    {
        MaxXP = xp;
        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);

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
                    ChangeAffinity(GetHighestAffinity(out float value), 10);
                    break;
                default:
                    Debug.LogWarning($"There is no behavior implimented for the level up reward {reward}");
                    break;
            }
        }
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
                        ChangeAffinity(GetHighestAffinity(out float value), 10 * reward.Value);
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
    public override void Die()
    {
        SetDead(true);
        Invoke("DelayedDeath", 2f);
    }

    private void DelayedDeath()
    {
        EventDeclarer.PlayerDead?.Invoke(true);

    }


    public void ResurrectMinion(GameObject minion, I_InteractMinionResurrect interactable)
    {
        StartCoroutine(CheckResurrection(minion, interactable));
    }
    #endregion
    private IEnumerator CheckResurrection(GameObject minion, I_InteractMinionResurrect interactable)
    {
        Debug.Log($"Starting resurrection of {minion.name}");
        if (!minion.TryGetComponent(out MinionStats minionStats))
        {
            Debug.LogWarning("Trying to resurrect a minion without minion stats component. cancelling res");
            yield break;
        }
        bool resurrecting = true;
        float timeWaited = 0;
        float distance;
        while (resurrecting)
        {
            yield return resurrectionIncrement;
            timeWaited += .5f;
            distance = (minion.transform.position - gameObject.transform.position).magnitude;
            if (distance >= ResurrectRange)
            {
                Debug.Log($"Distance to minion = {distance}. setting res succeeding to false.");
                interactable.SetResurrecting(false);
                resurrecting = false;
            }
            if (timeWaited >= ResurrectTime)
            {
                Debug.Log("Breaking loop because res time has been successfully waited");
                minionStats.Resurrect();
                interactable.SetResurrecting(false);
                resurrecting = false;

            }
        }
    }

    public override void HandleUIAttrDisplay(AttributeType attributeType, float newValue)
    {
        UiHandler.UpdateResourceBar(this, attributeType);
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
