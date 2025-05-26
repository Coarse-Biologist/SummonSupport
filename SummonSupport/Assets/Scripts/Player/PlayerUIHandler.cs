using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using SummonSupportEvents;
using System;
using System.Text.RegularExpressions;

public class PlayerUIHandler : MonoBehaviour
{
    public static PlayerUIHandler Instance { get; private set; }
    UIDocument uiDoc;
    VisualTreeAsset UIPrefabAssets;


    PlayerStats playerStats;
    VisualElement root;
    VisualElement minionHPBars;
    VisualElement playerUI;
    VisualElement resourceBarsContainer;
    ProgressBar hpProgressBar;
    GroupBox healthBarGroupBox;
    GroupBox powerBarGroupBox;
    ProgressBar powerProgressBar;
    ProgressBar playerXP_Bar;
    ResourceBar hpBar;
    ResourceBar powerBar;



    [SerializeField] public List<GameObject> minions;// = new List<GameObject>();
    Dictionary<LivingBeing, ProgressBar> HPDict = new Dictionary<LivingBeing, ProgressBar>();
    AlchemyHandler alchemyHandler;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        EventDeclarer.attributeChanged.AddListener(UpdateResourceBar);
        EventDeclarer.regenerationChanged.AddListener(UpdateRegenerationArrows);
        EventDeclarer.maxAttributeChanged.AddListener(UpdateMaxValueResourceBar);
        if (AlchemyHandler.Instance != null)
            AlchemyHandler.Instance.newMinionAdded.AddListener(AddMinionHP);
        EventDeclarer.minionDied.AddListener(RemoveMinionHP);
    }

    void OnDisable()
    {
        EventDeclarer.attributeChanged.RemoveListener(UpdateResourceBar);
        EventDeclarer.regenerationChanged.RemoveListener(UpdateRegenerationArrows);
        EventDeclarer.maxAttributeChanged.RemoveListener(UpdateMaxValueResourceBar);
        if (AlchemyHandler.Instance != null)
            AlchemyHandler.Instance.newMinionAdded.RemoveListener(AddMinionHP);
        EventDeclarer.minionDied.RemoveListener(RemoveMinionHP);
    }

    void Start()
    {
        SetUpUI();
        UpdateMaxValueResourceBar(playerStats, AttributeType.CurrentHitpoints);
        UpdateRegenerationArrows(playerStats, AttributeType.CurrentHitpoints);
        UpdateRegenerationArrows(playerStats, AttributeType.CurrentPower);
        UpdateResourceBar(playerStats, AttributeType.CurrentHitpoints);
        UpdateResourceBar(playerStats, AttributeType.CurrentPower);
        UpdateResourceBar(playerStats, AttributeType.CurrentXP);

    }

    private void SetUpUI()
    {
        uiDoc = UI_DocHandler.Instance.ui;
        UIPrefabAssets = UI_DocHandler.Instance.UIPrefabAssets;
        root = uiDoc.rootVisualElement;
        playerUI = root.Q<VisualElement>("MainUI");

        resourceBarsContainer = playerUI.Q<VisualElement>("ResourceBars");
        hpProgressBar = resourceBarsContainer.Q<ProgressBar>("HealthBar");
        powerProgressBar = resourceBarsContainer.Q<ProgressBar>("PowerBar");
        playerXP_Bar = resourceBarsContainer.Q<ProgressBar>("XP_Bar");
        InitializeResourceBarElements();

        minionHPBars = playerUI.Q<VisualElement>("MinionBarSlots");
        var craftingUI = root.Q<VisualElement>("CraftingUI");
        if (craftingUI != null)
            craftingUI.style.display = DisplayStyle.None;


        playerUI.style.opacity = 100f;
        craftingUI.style.opacity = 100f;
        playerStats = PlayerStats.Instance;
    }

    void InitializeResourceBarElements()
    {
        hpBar       = new(hpProgressBar);
        powerBar    = new(powerProgressBar);
    }

    void UpdateMaxValueResourceBar(LivingBeing creature, AttributeType attributeType)
    {
        hpProgressBar.highValue = playerStats.GetAttribute(AttributeType.MaxHitpoints);
        powerProgressBar.highValue = playerStats.GetAttribute(AttributeType.MaxPower);
        playerXP_Bar.highValue = playerStats.MaxXP;
    }

    public void AddMinionHP(LivingBeing livingBeing)
    {
        TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
        ProgressBar minionHP = prefabContainer.Q<ProgressBar>("HealthbarPrefab");

        if (minionHP == null) minionHP = new ProgressBar();
        float hp = livingBeing.GetAttribute(AttributeType.CurrentHitpoints);
        HPDict.TryAdd(livingBeing, minionHP);
        minionHP.title = $"{livingBeing.Name} HP: {hp}";
        minionHP.highValue = hp;
        minionHP.value = hp;
        if (minionHPBars == null) Logging.Error("minion HP visual element doesnt exist");
        minionHPBars.Add(minionHP);
        minionHP.RegisterCallback<ClickEvent>(evt => OnMinionSelect(minionHP));
    }
    //void OnMinionHPSelect(ClickEvent evt, ProgressBar minionHP)
    //{
    //    Logging.Info($"MinionHP bar {minionHP} clicked");
    //    //minionHP.AddToClassList("glow");
    //    minionHP.style.borderLeftColor = Color.green;
    //    MinionStats selectedMinion = GetLivingBeingHPBar(minionHP);
    //    EventDeclarer.SetActiveMinion?.Invoke(selectedMinion);
    //    CommandMinion.SetSelectedMinion(selectedMinion.gameObject);
    //}

    public void RemoveMinionHP(LivingBeing livingBeing)
    {
        ProgressBar minionHP = GetLivingBeingHPBar(livingBeing);
        if (minionHP != null)
        {
            HPDict.Remove(livingBeing);
            minionHPBars.Remove(minionHP);
        }
    }
    private LivingBeing GetLivingBeingFromHPBar(ProgressBar minionHP)
    {
        LivingBeing minion = null;
        foreach (KeyValuePair<LivingBeing, ProgressBar> kvp in HPDict)
            if (HPDict[kvp.Key] == minionHP)
            {
                minion = kvp.Key;
                return minion;
            }
        return minion;
    }

    private ProgressBar GetLivingBeingHPBar(LivingBeing minion)
    {
        return HPDict[minion];
    }
    private void OnMinionSelect(ProgressBar minionHP)
    {
        LivingBeing minionStats = GetLivingBeingFromHPBar(minionHP);
        if (minionHP.style.borderBottomColor != Color.yellow)
        {
            minionHP.style.borderBottomColor = Color.yellow;
            minionHP.style.borderRightColor = Color.yellow;
            minionHP.style.borderLeftColor = Color.yellow;
            minionHP.style.borderTopColor = Color.yellow;
        }
        else
        {
            minionHP.style.borderBottomColor = Color.clear;
            minionHP.style.borderRightColor = Color.clear;
            minionHP.style.borderLeftColor = Color.clear;
            minionHP.style.borderTopColor = Color.clear;
        }
        //.ToggleInClassList("selected");
        Logging.Info("Button Clicked");
        CommandMinion.SetSelectedMinion(minionStats.gameObject);
    }

    public void UpdateRegenerationArrows(LivingBeing livingBeing, AttributeType attributeType)
    {
        if (livingBeing.CharacterTag == CharacterTag.Player)
        {
            if (attributeType == AttributeType.CurrentHitpoints)
                hpBar.SetArrows(livingBeing.HealthRegenArrows);
            if (attributeType == AttributeType.CurrentPower)
                powerBar.SetArrows(livingBeing.PowerRegenArrows);
        }
    }
    public void UpdateResourceBar(LivingBeing livingBeing, AttributeType attributeType)
    {

        if (livingBeing.CharacterTag == CharacterTag.Player)
        {
            SetPlayerAttribute(attributeType);
        }
        else if (livingBeing.CharacterTag == CharacterTag.Minion)
        {
            SetMinionHealthBar(livingBeing);
        }
    }
    private void SetMinionHealthBar(LivingBeing livingBeing)
    {

        ProgressBar hpBar = GetLivingBeingHPBar(livingBeing);
        if (hpBar != null)
        {
            float hp = livingBeing.GetAttribute(AttributeType.CurrentHitpoints);
            hpBar.value = livingBeing.CurrentHP;
            hpBar.title = $"{livingBeing.Name} HP: {hp}";
        }
        else Logging.Info($"There was no hp bar for the living being {livingBeing.name}");

    }

    private void SetPlayerAttribute(AttributeType attributeType)
    {
        if (attributeType == AttributeType.CurrentHitpoints)
        {
            float value = playerStats.GetAttribute(attributeType);
            hpProgressBar.value = value;
            hpBar.SetValue(value);
        }
        if (attributeType == AttributeType.CurrentPower)
        {
            float value = playerStats.GetAttribute(attributeType);
            powerProgressBar.value = value;
            powerBar.SetValue(value);
        }
        if (attributeType == AttributeType.CurrentXP)
        {
            playerXP_Bar.value = playerStats.CurrentXP;
        }
    }
}
