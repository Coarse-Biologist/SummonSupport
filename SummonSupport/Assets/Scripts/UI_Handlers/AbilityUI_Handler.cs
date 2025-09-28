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


        ProgressBarL1 = leftSlots.Q<ProgressBar>("AbilitySlotL1");
        ProgressBarL2 = leftSlots.Q<ProgressBar>("AbilitySlotL2");
        ProgressBarL3 = leftSlots.Q<ProgressBar>("AbilitySlotL3");
        ProgressBarL4 = leftSlots.Q<ProgressBar>("AbilitySlotL4");
        ProgressBarL5 = leftSlots.Q<ProgressBar>("AbilitySlotL5");
        ProgressBarL6 = leftSlots.Q<ProgressBar>("AbilitySlotL6");
    }

    private void InitializeProgressBarDict()
    {
        ProgressBarDict.Add(0, ProgressBarL1);
        ProgressBarDict.Add(1, ProgressBarL2);
        ProgressBarDict.Add(2, ProgressBarL3);
        ProgressBarDict.Add(3, ProgressBarL4);
        ProgressBarDict.Add(4, ProgressBarL5);
        ProgressBarDict.Add(5, ProgressBarL6);

        abilityProgressBarDict.Add(0, null);
        abilityProgressBarDict.Add(1, null);
        abilityProgressBarDict.Add(2, null);
        abilityProgressBarDict.Add(3, null);
        abilityProgressBarDict.Add(4, null);
        abilityProgressBarDict.Add(5, null);
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
        if (slotIndex <= abilityProgressBarDict.Keys.Count) // check if the index is usable
        {
            abilityProgressBarDict[slotIndex] = ability; // set ability to the slot
        }
        else Debug.Log("OOFJKJWCNKCNOWJOWCO!!!!!!!!!!");
        SetAbilityIcon(slotIndex, ability);
    }

    private void SetAbilityIcon(int slotIndex, Ability ability)
    {
        if (ProgressBarDict.TryGetValue(slotIndex, out ProgressBar bar))
        {
            bar.title = ability.name;
            bar.style.backgroundImage = new StyleBackground(ability.Icon);
        }
    }


    private void AbilityUsed(int slotIndex)
    {
        ProgressBar abilityProgressBar = ProgressBarDict[slotIndex];
        if (abilityProgressBar == null) return;
        Ability ability = abilityProgressBarDict[slotIndex];
        if (ability == null) return;
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
