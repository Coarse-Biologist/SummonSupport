using System.Collections;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.UIElements;


public class AbilityUI_Handler : MonoBehaviour
{
    public AbilityUI_Handler Instance { private set; get; }

    #region class variables
    [SerializeField]
    public UIDocument uiDoc;
    private VisualElement root;
    private Dictionary<int, ProgressBar> ProgressBarDict = new();
    private Dictionary<int, Ability> abilityProgressBarDict = new();

    #region all ProgressBars

    private ProgressBar ProgressBarL1;
    private ProgressBar ProgressBarL2;
    private ProgressBar ProgressBarL3;
    private ProgressBar ProgressBarL4;
    private ProgressBar ProgressBarL5;
    private ProgressBar ProgressBarL6;

    #endregion

    #endregion

    #region SetUp
    public void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
        }

        Setup();
    }

    void OnEnable()
    {
        EventDeclarer.SlotChanged?.AddListener(SetAbilitySlot);
        EventDeclarer.AbilityUsed?.AddListener(AbilityUsed);

    }
    void OnDisable()
    {
        EventDeclarer.SlotChanged?.RemoveListener(SetAbilitySlot);
        EventDeclarer.AbilityUsed?.RemoveListener(AbilityUsed);

    }
    private void GetAllProgressBars()
    {
        root = uiDoc.rootVisualElement;

        VisualElement leftSlots = root.Q<VisualElement>("AbilitySlotsL");
        VisualElement rightSlots = root.Q<VisualElement>("AbilitySlotsR");


        ProgressBarL1 = leftSlots.Q<ProgressBar>("Ability1");
        ProgressBarL2 = leftSlots.Q<ProgressBar>("Ability2");
        ProgressBarL3 = leftSlots.Q<ProgressBar>("Ability3");
        ProgressBarL4 = leftSlots.Q<ProgressBar>("Ability4");
        ProgressBarL5 = leftSlots.Q<ProgressBar>("Ability5");
        ProgressBarL6 = leftSlots.Q<ProgressBar>("Ability6");
    }

    private void InitializeProgressBarDict()
    {
        ProgressBarDict.Add(1, ProgressBarL1);
        ProgressBarDict.Add(2, ProgressBarL2);
        ProgressBarDict.Add(3, ProgressBarL3);
        ProgressBarDict.Add(4, ProgressBarL4);
        ProgressBarDict.Add(5, ProgressBarL5);
        ProgressBarDict.Add(6, ProgressBarL6);

    }



    private void Setup()
    {
        GetAllProgressBars();
        InitializeProgressBarDict();
    }
    #endregion

    #region Ability slot changed invoke response
    public void SetAbilitySlot(int slotIndex, Ability ability)
    {
        if (slotIndex <= abilityProgressBarDict.Keys.Count)
            abilityProgressBarDict[slotIndex] = ability;
        SetAbilityIcon(slotIndex, ability);
    }

    private void SetAbilityIcon(int slotIndex, Ability ability)
    {
        if (ProgressBarDict.TryGetValue(slotIndex, out ProgressBar bar))
        {
            VisualElement background = bar.Q("unity-progress-bar__background");
            background.style.backgroundImage = new StyleBackground(ability.Icon);
        }
    }


    private void AbilityUsed(int slotIndex)
    {
        ProgressBar abilityProgressBar = ProgressBarDict[slotIndex];
        Ability ability = abilityProgressBarDict[slotIndex];
        float cooldown = ability.Cooldown;
        Logging.Info($"Ability {ability.name} with cooldown {cooldown} has been used.");

        abilityProgressBar.highValue = cooldown;
        abilityProgressBar.value = cooldown;


        StartCoroutine(DrainCooldown(slotIndex, cooldown));
    }

    private IEnumerator DrainCooldown(int slotIndex, float duration)
    {
        ProgressBar progressBar = ProgressBarDict[slotIndex];
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            progressBar.value = Mathf.Max(progressBar.highValue - elapsed, 0);
        }

        progressBar.value = 0;
    }


    #endregion

}
