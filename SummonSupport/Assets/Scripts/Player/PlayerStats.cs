using System;
using System.Collections;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerStats : LivingBeing
{
    public static PlayerStats Instance;

    [Header("Experience Info")]

    [SerializeField] public int CurrentLevel { private set; get; } = 1;
    [SerializeField] public float CurrentXP { private set; get; } = 0;
    [SerializeField] public float MaxXP { private set; get; } = 100;
    [field: SerializeField] public int SkillPoints { private set; get; } = 100;


    #region Ressurrection Variables
    [SerializeField] public float ResurrectTime { private set; get; } = 5f;
    [SerializeField] public float ResurrectRange { private set; get; } = 2f;
    private WaitForSeconds resurrectionIncrement = new WaitForSeconds(.5f);

    #endregion
    [field: SerializeField] public int TotalControlllableMinions { private set; get; } = 2;
    [field: SerializeField] public int AbilitySlots { private set; get; } = 2;
    [field: SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    public PlayerUIHandler UiHandler { private set; get; }
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
    private void GainXP(LivingBeing defeatedEnemy)
    {
        CurrentXP += defeatedEnemy.XP_OnDeath;
        if (CurrentXP >= MaxXP)
        {
            LevelUp();
        }
        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);
    }
    public void AddControllableMinions(int changeValue)
    {
        TotalControlllableMinions = Math.Max(0, TotalControlllableMinions + changeValue);
        Debug.Log($"Changing total controllable minion number");

    }
    public void AddAbilitySlot(int changeValue)
    {
        AbilitySlots = Math.Max(0, AbilitySlots + changeValue);
    }
    public void GainXP(int amount)
    {

        CurrentXP += amount * 30;

        if (CurrentXP >= MaxXP)
        {
            LevelUp();
        }
        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);
    }

    private void LevelUp()
    {
        CurrentLevel += 1;
        CurrentXP -= MaxXP;
        MaxXP *= 2;
        SkillPoints++;
        EventDeclarer.PlayerLevelUp?.Invoke(LevelUpHandler.GetLevelRewardString(CurrentLevel));
        LevelUpHandler.GetLevelRewards(CurrentLevel);
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
                    ChangeAffinity(GetHighestAffinity(), 10);
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
                        ChangeAffinity(GetHighestAffinity(), 10 * reward.Value);
                        break;
                    default:
                        Debug.LogWarning($"There is no behavior implimented for the level up reward {reward}");
                        break;
                }
            }
        }
    }


    public override void Die()
    {
        SetDead(true);
        Invoke("DelayedDeath", 2f);
    }

    private void DelayedDeath()
    {
        EventDeclarer.PlayerDead?.Invoke(true);

    }


    public void ResurrectMinion(GameObject minion)
    {
        StartCoroutine(CheckResurrection(minion));
    }

    private IEnumerator CheckResurrection(GameObject minion)
    {
        bool resSucceeding = true;
        float timeWaited = 0;
        float distance;
        while (resSucceeding)
        {
            yield return resurrectionIncrement;
            timeWaited += .5f;
            distance = (minion.transform.position - gameObject.transform.position).magnitude;
            if (distance >= ResurrectRange)
            {
                Debug.Log($"Distance to minion = {distance}. setting res succeeding to false.");
                resSucceeding = false;
            }
            if (timeWaited >= ResurrectTime)
            {
                Debug.Log("Breaking loop because res time has been successfully waited");
                if (minion.TryGetComponent<MinionStats>(out MinionStats minionStats)) minionStats.Resurrect();
                break;
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


}
