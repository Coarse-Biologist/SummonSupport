using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using SummonSupportEvents;
using System;

public class PlayerUIHandler : MonoBehaviour
{
    public static PlayerUIHandler Instance { get; private set; }
    private UIDocument uiDoc;
    private VisualTreeAsset UIPrefabAssets;


    PlayerStats playerStats;
    private VisualElement root;
    private VisualElement minionHPBars;
    private VisualElement playerUI;
    private VisualElement resourceBarsContainer;
    private ProgressBar playerHealthBar;
    private ProgressBar playerPowerBar;
    private ProgressBar playerXP_Bar;



    [SerializeField] public List<GameObject> minions;// = new List<GameObject>();
    private Dictionary<LivingBeing, ProgressBar> HPDict = new Dictionary<LivingBeing, ProgressBar>();
    private AlchemyHandler alchemyHandler;
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
        EventDeclarer.maxAttributeChanged.AddListener(UpdateMaxValueResourceBar);
        if (AlchemyHandler.Instance != null)
            AlchemyHandler.Instance.newMinionAdded.AddListener(AddMinionHP);
        EventDeclarer.minionDied.AddListener(RemoveMinionHP);
    }

    void OnDisable()
    {
        EventDeclarer.attributeChanged.RemoveListener(UpdateResourceBar);
        EventDeclarer.maxAttributeChanged.RemoveListener(UpdateMaxValueResourceBar);
        if (AlchemyHandler.Instance != null)
            AlchemyHandler.Instance.newMinionAdded.RemoveListener(AddMinionHP);
        EventDeclarer.minionDied.RemoveListener(RemoveMinionHP);
    }

    void Start()
    {
        SetUpUI();
        UpdateMaxValueResourceBar(playerStats, AttributeType.CurrentHitpoints);
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
        playerHealthBar = resourceBarsContainer.Q<ProgressBar>("HealthBar");
        playerPowerBar = resourceBarsContainer.Q<ProgressBar>("PowerBar");
        playerXP_Bar = resourceBarsContainer.Q<ProgressBar>("XP_Bar");



        minionHPBars = playerUI.Q<VisualElement>("MinionBarSlots");
        var craftingUI = root.Q<VisualElement>("CraftingUI");
        if (craftingUI != null)
            craftingUI.style.display = DisplayStyle.None;


        playerUI.style.opacity = 100f;
        craftingUI.style.opacity = 100f;
        playerStats = PlayerStats.Instance;
    }

    void UpdateMaxValueResourceBar(LivingBeing creature, AttributeType attributeType)
    {
        playerHealthBar.highValue = playerStats.GetAttribute(AttributeType.MaxHitpoints);
        playerPowerBar.highValue = playerStats.GetAttribute(AttributeType.MaxPower);
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
    }
    public void RemoveMinionHP(LivingBeing livingBeing)
    {
        ProgressBar minionHP = GetLivingBeingHPBar(livingBeing);
        if (minionHP != null)
        {
            HPDict.Remove(livingBeing);
            minionHPBars.Remove(minionHP);
        }

    }

    private ProgressBar GetLivingBeingHPBar(LivingBeing minion)
    {
        return HPDict[minion];
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
            playerHealthBar.value = playerStats.GetAttribute(attributeType);
        if (attributeType == AttributeType.CurrentPower)
            playerPowerBar.value = playerStats.GetAttribute(attributeType);
        if (attributeType == AttributeType.CurrentXP)
        {
            playerXP_Bar.value = playerStats.CurrentXP;
        }
    }
}
