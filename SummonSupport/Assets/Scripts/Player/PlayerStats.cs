using System;
using System.Collections;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public class PlayerStats : LivingBeing
{
    public static PlayerStats Instance;

    [Header("Experience Info")]

    [SerializeField] public int CurrentLevel { private set; get; } = 0;
    [SerializeField] public float CurrentXP { private set; get; } = 0;
    [SerializeField] public float MaxXP { private set; get; } = 100;
    [field: SerializeField] public int SkillPoints { private set; get; } = 0;


    #region Ressurrection Variables
    [SerializeField] public float ResurrectTime { private set; get; } = 5f;
    [SerializeField] public float ResurrectRange { private set; get; } = 2f;
    private WaitForSeconds resurrectionIncrement = new WaitForSeconds(.5f);

    #endregion
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
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
        Debug.Log($"Gaining Xp in playerStats script. Current xp = {CurrentXP}");
        CurrentXP += defeatedEnemy.XP_OnDeath;
        if (CurrentXP >= MaxXP)
        {
            LevelUp();
        }
        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);
    }
    public void GainXP(int amount)
    {
        Debug.Log($"Gaining Xp in playerStats script. Current xp prior to gain s= {CurrentXP}");

        CurrentXP += amount;
        Debug.Log($"Gaining Xp in playerStats script. xp after gain = {CurrentXP}");

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
    }


    public override void Die()
    {
        //Logging.Info($"{Name} died");

        if (HasStatusEffect(StatusEffectType.ExplodeOnDeath)) ViciousDeathExplosion();
        SetDead(true);
        EventDeclarer.PlayerDead?.Invoke(true);
        //Destroy(gameObject);
    }


    public void ResurrectMinion(GameObject minion)
    {
        Debug.Log($" {Name} Wants to resurrect a minion.");
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
}
