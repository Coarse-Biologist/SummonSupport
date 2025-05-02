using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.UIElements;


public class AbilityUI_Handler : MonoBehaviour
{
    #region class variables
    [SerializeField] public UIDocument uiDoc;
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
        Setup();
    }

    void OnEnable()
    {
        EventDeclarer.SlotChanged?.AddListener(SetAbilitySlot);
    }
    void OnDisable()
    {
        EventDeclarer.SlotChanged?.RemoveListener(SetAbilitySlot);
    }
    private void GetAllProgressBars()
    {
        root = uiDoc.rootVisualElement;

        VisualElement leftSlots = root.Q<VisualElement>("AbilitySlotsL");
        VisualElement rightSlots = root.Q<VisualElement>("AbilitySlotsR");


        ProgressBarL1 = rightSlots.Q<ProgressBar>("Ability1");
        ProgressBarL2 = rightSlots.Q<ProgressBar>("Ability2");
        ProgressBarL3 = rightSlots.Q<ProgressBar>("Ability3");
        ProgressBarL4 = rightSlots.Q<ProgressBar>("Ability4");
        ProgressBarL5 = rightSlots.Q<ProgressBar>("Ability5");
        ProgressBarL6 = rightSlots.Q<ProgressBar>("Ability6");
    }

    private void InitializeProgressBarDict()
    {
        ProgressBarDict.Add(1, ProgressBarL1);
        ProgressBarDict.Add(2, ProgressBarL2);
        ProgressBarDict.Add(3, ProgressBarL3);
        ProgressBarDict.Add(4, ProgressBarL4);
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
            //
        }
    }

    #endregion

}
