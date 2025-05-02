using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using SummonSupportEvents;
using System;

public class PlayerUIHandler : MonoBehaviour
{
    public static PlayerUIHandler Instance { get; private set; }
    [SerializeField] public UIDocument uiDoc;


    LivingBeing playerStats;
    private VisualElement root;
    private VisualElement minionHPBars;
    private VisualElement playerUI;
    private VisualElement resourceBarsContainer;
    private ProgressBar playerHealthBar;
    private ProgressBar playerPowerBar;


    [SerializeField] public List<GameObject> minions;// = new List<GameObject>();
    private Dictionary<LivingBeing, ProgressBar> HPDict = new Dictionary<LivingBeing, ProgressBar>();
    private AlchemyHandler alchemyHandler;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        root = uiDoc.rootVisualElement;
        playerUI = root.Q<VisualElement>("MainUI");

        resourceBarsContainer = playerUI.Q<VisualElement>("ResourceBars");
        playerHealthBar = resourceBarsContainer.Q<ProgressBar>("HealthBar");
        playerPowerBar = resourceBarsContainer.Q<ProgressBar>("PowerBar");


        minionHPBars = playerUI.Q<VisualElement>("MinionBarSlots");
        var craftingUI = root.Q<VisualElement>("CraftingUI");
        if (craftingUI != null)
            craftingUI.style.display = DisplayStyle.None;

        playerUI.SetEnabled(true);
        craftingUI.SetEnabled(true);
        playerUI.style.opacity = 100f;
        craftingUI.style.opacity = 100f;
        playerStats = gameObject.GetComponent<LivingBeing>();
    }
    void Start()
    {
        UpdateMaxValueResourceBar();
    }

    void UpdateMaxValueResourceBar()
    {
        playerHealthBar.highValue = playerStats.GetAttribute(AttributeType.MaxHitpoints);
        playerPowerBar.highValue = playerStats.GetAttribute(AttributeType.MaxPower);
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

    public void AddMinionHP(LivingBeing livingBeing)
    {
        ProgressBar minionHP = new ProgressBar();
        float hp = livingBeing.GetAttribute(AttributeType.CurrentHitpoints);
        HPDict.TryAdd(livingBeing, minionHP);
        minionHP.title = $"{livingBeing.Name} HP: {hp}";
        minionHP.highValue = hp;
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
        else Logging.Error("You are trying to delete a none inion as though it were a minion");
    }

    private ProgressBar GetLivingBeingHPBar(LivingBeing minion)
    {
        return HPDict[minion];
    }

    public void UpdateResourceBar(LivingBeing livingBeing, AttributeType attributeType)
    {

        if (livingBeing.gameObject.CompareTag("Player"))
        {
            SetPlayerAttribute(livingBeing, attributeType);
        }
        else if (livingBeing.gameObject.CompareTag("Minion"))
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
    }

    private void SetPlayerAttribute(LivingBeing livingBeing, AttributeType attributeType)
    {

        if (attributeType == AttributeType.CurrentHitpoints)
            playerHealthBar.value = livingBeing.GetAttribute(attributeType);
        if (attributeType == AttributeType.CurrentPower)
            playerPowerBar.value = livingBeing.GetAttribute(attributeType);
    }
}
