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
    private Dictionary<int, Ability> abilityIntProgressBarDict = new();


    #region all ProgressBars

    private ProgressBar ProgressBarL1;
    private ProgressBar ProgressBarL2;
    private ProgressBar ProgressBarL3;
    private ProgressBar ProgressBarL4;
    private ProgressBar ProgressBarL5;
    private ProgressBar ProgressBarL6;

    private AbilityModHandler modHandler;
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
    void Start()
    {
        if (PlayerStats.Instance.TryGetComponent(out AbilityModHandler handler))
            modHandler = handler;
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

        abilityIntProgressBarDict.Add(0, null);
        abilityIntProgressBarDict.Add(1, null);
        abilityIntProgressBarDict.Add(2, null);
        abilityIntProgressBarDict.Add(3, null);
        abilityIntProgressBarDict.Add(4, null);
        abilityIntProgressBarDict.Add(5, null);

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

        if (ability == null) return;
        else if (slotIndex <= abilityIntProgressBarDict.Keys.Count) // check if the index is usable
        {
            abilityIntProgressBarDict[slotIndex] = ability; // set ability to the slot
            SetAbilityIcon(slotIndex, ability);
        }
    }

    private void SetAbilityIcon(int slotIndex, Ability ability)
    {
        if (ability == null)
            return;

        if (ProgressBarDict.TryGetValue(slotIndex, out ProgressBar bar))
        {
            bar.title = ability.Name;
            bar.style.backgroundImage = new StyleBackground(ability.Icon);
        }

    }


    private void AbilityUsed(int index)
    {
        ProgressBar abilityProgressBar = ProgressBarDict[index];
        Ability ability = abilityIntProgressBarDict[index];
        if (abilityProgressBar == null) return;
        if (ability == null) return;
        float cooldown = ability.Cooldown + modHandler.GetModAttributeByType(ability, AbilityModTypes.Cooldown);
        //Logging.Info($"Ability {ability.name} with cooldown {cooldown} has been used.");

        abilityProgressBar.highValue = cooldown;
        abilityProgressBar.value = cooldown;


        StartCoroutine(DrainCooldown(index, cooldown));
    }

    private IEnumerator DrainCooldown(int index, float duration)
    {
        ProgressBar progressBar = ProgressBarDict[index];
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
