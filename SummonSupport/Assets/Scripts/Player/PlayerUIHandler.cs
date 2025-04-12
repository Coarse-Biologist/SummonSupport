using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using SummonSupportEvents;

public class PlayerUIHandler : MonoBehaviour
{

    [SerializeField] public UIDocument uiDoc;
    private VisualElement root;
    private VisualElement minionHPBars;
    private VisualElement playerUI;

    [SerializeField] public List<GameObject> minions;// = new List<GameObject>();
    private Dictionary<GameObject, ProgressBar> minionHPDict = new Dictionary<GameObject, ProgressBar>();
    private AlchemyHandler alchemyHandler;


    void Awake()
    {
        alchemyHandler = FindFirstObjectByType<AlchemyHandler>();
        root = uiDoc.rootVisualElement;
        playerUI = root.Q<VisualElement>("MainUI");
        minionHPBars = playerUI.Q<VisualElement>("MinionBarSlots");
        //EventDeclarer.Initialize();
    }

    void OnEnable()
    {
        EventDeclarer.hpChanged.AddListener(SetMinionHP);
        alchemyHandler.newMinionAdded.AddListener(AddMinionHP);
    }

    void OnDisable()
    {
        EventDeclarer.hpChanged.RemoveListener(SetMinionHP);
        alchemyHandler.newMinionAdded.RemoveListener(AddMinionHP);

    }

    public void AddMinionHP(GameObject minion)
    {
        ProgressBar minionHP = new ProgressBar();
        minionHPDict.TryAdd(minion, minionHP);
        minionHP.title = $"{minion.GetComponent<LivingBeing>().Name} HP";
        minionHPBars.Add(minionHP);

    }
    private ProgressBar GetMinionsHPBar(GameObject minion)
    {
        return minionHPDict[minion];
    }

    public void SetMinionHP(GameObject minion)
    {
        ProgressBar hpBar = GetMinionsHPBar(minion);
        if (hpBar != null)
        {
            hpBar.value = minion.GetComponent<LivingBeing>().CurrentHP;
        }
    }
}
