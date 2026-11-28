#region Imports

using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;
using SummonSupportEvents;
using static StatusEffectsLibrary;
using SS_Structs;



#endregion

public class AlchemyBenchUI : MonoBehaviour, I_Interactable
{
    public static AlchemyBenchUI Instance { get; private set; }
    #region Class Variables

    #region Constants

    #region UI assets
    private VisualTreeAsset UIPrefabAssets;
    [SerializeField] Sprite alchemyBackground;
    [Tooltip("Elemental knowledge needed before an element can be selected for crafting minions.")]
    [SerializeField] private int minKnowledgeForElementToggle = 50;

    #endregion

    private UIDocument ui;
    private VisualElement root;
    private VisualElement interactWindow;
    private VisualElement craftingUI;
    private VisualElement centerPanel;
    private VisualElement topRightPanel;
    private VisualElement bottomRightPanel;
    private ScrollView bottomLeftPanel;
    #region Ability crafting / slotting
    private Ability selectedAbility;
    private Ability selectedMinionAbility;
    private DropdownField dropdownField;

    private List<string> SlotNames = new List<string>() { "1", "2", "3", "Q", "E", "F" };
    private int abilitySlot;
    private PlayerAbilityHandler playerAbilityHandler;

    #endregion

    //private Label instructions;
    private Label moreInfoContainer;

    private AlchemyHandler alchemyHandler;

    #endregion

    #region Runtime Variables
    private Dictionary<CraftingPotential, int> selectedCraftingPotential = new()
    {
        {CraftingPotential.OrganMass, 0 },
        {CraftingPotential.CorePower, 0 },
        {CraftingPotential.EtherDensity, 0 },
    };

    //private Dictionary<AlchemyLoot, int> selectedIngredients = new();
    private List<Element> selectedElements = new List<Element>();
    private MinionStats minionToUpgrade;
    private MinionStats minionToRecycle;

    #region ability modding
    private AbilityModHandler ModHandler;

    private AbilityModTypes selectedModType = AbilityModTypes.None;
    private StatusEffects selectedStatusEffect = null;

    private List<Button> SpawnedButtons = new();

    #region player upgrades

    public static Dictionary<LevelRewards, int> selectedPlayerUpgrades = new()
        {
        {LevelRewards.MaximumHealth, 0},
        {LevelRewards.MaximumPower, 0},
        {LevelRewards.HealthRegeneration, 0},
        {LevelRewards.PowerRegeneration, 0},
        {LevelRewards.ElementalAffinity, 0},
        {LevelRewards.AbilitySlot, 0},
        {LevelRewards.TotalControlllableMinions, 0},
        };
    private int selectedUpgradeCost = 0;

    #endregion

    #endregion

    private bool elementsGenerated = false;

    // For B / Circle: back from a sub-screen to the bench's main screen, and from there away from the bench.
    private bool inUse = false;
    private bool onDefaultScreen = false;

    #endregion

    #endregion

    #region Setup



    void Awake()
    {
        Instance = this;
        alchemyHandler = GetComponent<AlchemyHandler>();
    }

    void Start()
    {
        ui = UI_DocHandler.Instance.ui;
        UIPrefabAssets = UI_DocHandler.Instance.UIPrefabAssets;

        root = ui.rootVisualElement;
        interactWindow = root.Q<VisualElement>("Interact");
        craftingUI = root.Q<VisualElement>("CraftingUI");
        centerPanel = craftingUI.Q<VisualElement>("CenterPanel");
        topRightPanel = craftingUI.Q<VisualElement>("TopRightPanel");
        bottomLeftPanel = craftingUI.Q<ScrollView>("BottomLeftPanel");
        //craftAbilityButton = mainCraftingOptions.Q<VisualElement>("ConcoctButton");

        //instructions = craftingUI.Q<Label>("Instructions");
        bottomRightPanel = craftingUI.Q<VisualElement>("BottomRightPanel");
        moreInfoContainer = bottomRightPanel.Q<Label>("CraftingResults");
        interactWindow.style.display = DisplayStyle.None;
        craftingUI.style.display = DisplayStyle.None;

        ModHandler = AbilityModHandler.Instance;

        if (ModHandler == null)
        {
            Debug.Log("No modhandler found");
        }


        //ShowBackground();
    }

    void Update()
    {
        if (!inUse || !MenuNavigation.CancelPressedThisFrame()) return;
        if (onDefaultScreen) QuitAlchemyUI();
        else ShowDefaultScreen();
    }
    #endregion

    #region Trigger effects

    public void ShowInteractionOption()
    {
        FloatingInfoHandler.Instance.ShowInteractionOption(new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), "Z to use alchemy bench");
    }

    public void HideInteractionOption()
    {
        FloatingInfoHandler.Instance.HideInteractionOption();
    }

    #endregion

    #region Begin and end workbench use
    private void TogglePlayerUsingUI(bool isUsing, AnimationControllerScript anim = null)
    {
        EventDeclarer.UsingUI?.Invoke();

        if (isUsing)
        {
            EventDeclarer.PauseGame?.Invoke();

            if (anim != null)
            {
                anim.SetUpdateMode(AnimatorUpdateMode.UnscaledTime);
                //Debug.Log("Changing animation? but probably time scale is 0");
                anim.ChangeAnimation("Crafting");
            }
            else Debug.Log("No animation script found");
        }
        else
        {
            EventDeclarer.UnpauseGame?.Invoke();

            if (anim != null)
            {
                anim.SetUpdateMode(AnimatorUpdateMode.Normal);
                anim.gameObject.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            }
        }
    }
    private void UseAlchemyObjectInteraction(bool On)
    {
        if (TryGetComponent(out AlchemyObjectInteraction itemInteraction))
        {
            if (On)
                itemInteraction.Interact(gameObject);
            else itemInteraction.HideInteractionOption();
        }

    }
    public void Interact(GameObject Player)//WithWorkBench()
    {

        UseAlchemyObjectInteraction(true);

        UnityEngine.Cursor.lockState = CursorLockMode.None;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = true;

        if (Player.TryGetComponent(out PlayerAbilityHandler abilityHandler))
            playerAbilityHandler = abilityHandler;
        else return;
        Time.timeScale = 0f;
        inUse = true;

        HideInteractionOption();
        TogglePlayerUsingUI(true, Player.GetComponent<AnimationControllerScript>());
        ShowUI(craftingUI);
        ShowDefaultScreen();


    }
    private void ShowDefaultScreen()
    {

        ClearAllPanels(true);
        ResetVars();
        SetInstructionsText("Manage minions or abilities here at your alchemy station");
        Button playerManagementButton = AddButtonToPanel("Player", centerPanel, 50, 5);
        Button abilityManagementOptions = AddButtonToPanel("Abilities", centerPanel, 50, 5);
        Button minionManagementOptions = AddButtonToPanel("Minions", centerPanel, 50, 5);
        Button quitButton = AddButtonToPanel("Quit", centerPanel, 50, 5);

        playerManagementButton.clicked += () => ShowPlayerManagementOptions();
        abilityManagementOptions.clicked += () => ShowAbilityManagementOptions();
        minionManagementOptions.clicked += () => ShowMinionManagementOptions();
        quitButton.clicked += () => QuitAlchemyUI();
        onDefaultScreen = true;
    }
    #region player upgrade handling
    private void ShowPlayerManagementOptions()
    {
        ClearAllPanels(true);
        ResetVars();

        SetInstructionsText($"Use skill-points to purchase upgrades. \nYou have {PlayerStats.Instance.SkillPoints} skill-points.");
        Button backButton = AddButtonToPanel("Back", centerPanel, 50, 5);

        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);
        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);

        backButton.clicked += () => ShowDefaultScreen();
        confirmButton.clicked += () => HandleplayerUpgradeAttempt();
        clearButton.clicked += () => ClearSelectedPlayerUpgrades();


        ShowPlayerUpgradeOptions();

    }

    private void ShowPlayerUpgradeOptions()
    {
        foreach (LevelRewards reward in LevelUpHandler.RewardsCostDict.Keys)
        {
            Button button = AddButtonToPanel(LevelUpHandler.RewardsDescriptionDict[reward], bottomLeftPanel, 50, 5);
            button.clicked += () => TryAddSelectedPlayerUpgrade(reward);
            button.clicked += () => SetPlayerUpgradeInfo();
        }
    }
    private void HandleplayerUpgradeAttempt()
    {
        PlayerStats.Instance.ExpendSkillPoints(selectedUpgradeCost);
        SetInstructionsText($"Upgrade successful! \nYou have {PlayerStats.Instance.SkillPoints} skill-points.");
        PlayerStats.Instance.GainLevelRewards(selectedPlayerUpgrades);
        ClearSelectedPlayerUpgrades();
    }

    private void TryAddSelectedPlayerUpgrade(LevelRewards reward)
    {
        int price = LevelUpHandler.RewardsCostDict[reward];

        if (selectedUpgradeCost + price <= PlayerStats.Instance.SkillPoints)
        {
            selectedUpgradeCost += price;
            selectedPlayerUpgrades[reward] += 1;
        }
        else Debug.Log($"Insufficient skill-points for the upgrade {reward}");
    }

    private void ClearSelectedPlayerUpgrades()
    {
        selectedUpgradeCost = 0;
        foreach (LevelRewards key in selectedPlayerUpgrades.Keys.ToList())
        {
            selectedPlayerUpgrades[key] = 0;
        }
    }

    private void SetPlayerUpgradeInfo()
    {
        //Debug.Log($"trying to add text info");

        string text = $"Select upgrades. Remaining Skill-Points: {PlayerStats.Instance.SkillPoints - selectedUpgradeCost}. Currently selected upgrades:";
        foreach (var rewardKvp in selectedPlayerUpgrades)
        {

            if (rewardKvp.Value != 0)
            {
                text += $"\n{LevelUpHandler.RewardsDescriptionDict[rewardKvp.Key]} x {rewardKvp.Value}";
            }
        }
        SetInstructionsText(text);
    }


    #endregion

    private void ShowMinionManagementOptions()
    {
        ClearAllPanels(true);
        ResetVars();

        SetInstructionsText("Craft, upgrade or recycle minions for parts.");

        Button craftMinionsButton = AddButtonToPanel("Craft", centerPanel, 50, 5);
        Button minionManagementOptions = AddButtonToPanel("Upgrade", centerPanel, 50, 5);

        Button recycleButton = AddButtonToPanel("Recycle", centerPanel, 50, 5);
        Button backButton = AddButtonToPanel("Back", centerPanel, 50, 5);


        craftMinionsButton.clicked += () => ShowCraftingOptions();
        minionManagementOptions.clicked += () => HandleUpgradeDisplay();
        recycleButton.clicked += () => ShowRecycleOptions();
        backButton.clicked += () => ShowDefaultScreen();


    }

    private void ShowAbilityManagementOptions()
    {
        ClearAllPanels(true);
        ResetVars();

        SetInstructionsText("Would you like to concoct new abilities, modify them, or control their use slots?");

        Button craftButton = AddButtonToPanel("Concoct", centerPanel, 50, 5);
        Button abilityModButton = AddButtonToPanel("Modify", centerPanel, 50, 5);
        Button abilitySlotButton = AddButtonToPanel("Set Slot", centerPanel, 50, 5);
        Button backButton = AddButtonToPanel("Back", centerPanel, 50, 5);

        craftButton.clicked += () => ShowAbilityCraftingOptions();
        abilitySlotButton.clicked += () => SlotAbilities();
        abilityModButton.clicked += () => ShowAbilityModOptions();
        backButton.clicked += () => ShowDefaultScreen();
    }
    private void QuitAlchemyUI()
    {
        inUse = false;
        ClearAllPanels();
        ResetVars();
        HideUI(craftingUI);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = false;
        UseAlchemyObjectInteraction(false);

        TogglePlayerUsingUI(false);
    }

    #endregion

    #region Show Info in instructions panel
    private void ShowUpgradeInfo()
    {
        SetInstructionsText("Which Minion would you like to upgrade?");
        string ingredientInfo = "\nAllocated Crafting Potential: ";

        foreach (KeyValuePair<CraftingPotential, int> kvp in selectedCraftingPotential)
        {
            if (kvp.Value > 0)
                ingredientInfo += $"\n{kvp.Key}: {kvp.Value}. ";
        }
        SetInstructionsText(ingredientInfo, true);

    }


    private void ShowCraftingInfo()
    {
        string craftingInfo = "";
        SetInstructionsText("Combine Cores, Ether and Body Parts to make minons of selected Elemental Affinity.");
        if (!CheckUsingCoresandOrgans()) SetInstructionsText($"\nYou must use cores and organs in order to craft a minion.", true);

        craftingInfo += "\nSelected Ingredients: ";

        foreach (KeyValuePair<CraftingPotential, int> kvp in selectedCraftingPotential)
        {
            if (kvp.Value > 0)
                craftingInfo += $"{AlchemyInventory.GetCraftingPotentialString()}: {kvp.Value}. ";
        }
        SetInstructionsText(craftingInfo, true);


    }


    #endregion

    #region Upgrading Minions
    private void HandleUpgradeDisplay()
    {
        ClearAllPanels();
        ResetVars();
        ShowUpgradableMinions();
        ShowUpgradeInfo();
        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);
        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);
        confirmButton.clicked += () => HandleUpgradeMinion(minionToUpgrade);
        clearButton.clicked += () => ClearCraftingSelection();
        clearButton.clicked += () => HandleUpgradeDisplay();
    }
    private void HandleUpgradeMinion(MinionStats minionToUpgrade)
    {
        if (minionToUpgrade == null) return;
        int maxEtherUse = 0;
        foreach (Element element in selectedElements)
        {
            maxEtherUse += 200 - minionToUpgrade.GetAffinity(element);
        }
        selectedCraftingPotential[CraftingPotential.EtherDensity] = Math.Min(maxEtherUse, selectedCraftingPotential[CraftingPotential.EtherDensity]);

        alchemyHandler.UpgradeMinion(minionToUpgrade, selectedCraftingPotential, selectedElements);
        AlchemyInventory.ExpendCraftingPotential(selectedCraftingPotential);
        ClearCraftingSelection();
        HandleUpgradeDisplay();
    }
    private void ShowUpgradableMinions()
    {
        foreach (MinionStats minion in alchemyHandler.activeMinions)
        {
            Button minionButton = AddButtonToPanel($"Upgrade {minion.Name}", bottomLeftPanel, 40, 5);
            minionButton.clicked += () => SetMinionToUpgrade(minion);
        }
    }
    private void SetMinionToUpgrade(MinionStats minion)
    {
        minionToUpgrade = minion;
        SetInstructionsText($"Select ingredients and element with which to upgrade {minion.Name}.");
        SpawnCraftingPotentialButtons();
        ShowElementToggles(bottomLeftPanel);
    }
    #endregion

    #region Modify Abilities

    private void ShowAbilityModOptions()
    {
        ClearAllPanels();
        ResetVars();

        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);
        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);

        clearButton.clicked += () => SetSelectedAbility(null);
        confirmButton.clicked += () => AttemptModification(selectedModType);
        confirmButton.clicked += () => DisplayAbilityModOptions(selectedAbility);

        //SetInstructionsText("Select an ability to modify.");
        DisplayAllModableAbilities();
    }

    private void DisplayAllModableAbilities()
    {
        ShowUI(bottomLeftPanel);
        foreach (Ability ability in playerAbilityHandler.Instance.Abilities)
        {
            ///Ability potentiallySelectedAbility = ability;
            Button button = AddButtonToPanel($"{ability.Name}", bottomLeftPanel, 30, 5);
            button.clicked += () => SetSelectedAbility(ability);
            button.clicked += () => DisplayAbilityModOptions(ability);
        }
        foreach (LivingBeing minion in alchemyHandler.activeMinions)
        {
            if (minion.TryGetComponent(out CreatureAbilityHandler creatureAbilityHandler))
                foreach (Ability ability in creatureAbilityHandler.Abilities)
                {
                    Ability potentiallySelectedAbility = ability;
                    Button button = AddButtonToPanel($"{ability.Name}", bottomLeftPanel, 30, 5);
                    button.clicked += () => SetSelectedAbility(potentiallySelectedAbility);
                    button.clicked += () => DisplayAbilityModOptions(potentiallySelectedAbility);

                }
        }
    }
    private void DisplayAbilityModOptions(Ability ability)
    {
        if (selectedAbility == null) return;
        ClearPanel(bottomLeftPanel);
        SetInstructionsText($"Select an attribute for the ability which you would like to upgrade.");
        if (selectedAbility != null && selectedModType != AbilityModTypes.None)
            SetInstructionsText($"Confirm to add a mod ({GeneralFunctions.GetCleanEnumString(selectedModType)}) {selectedAbility.Name}");

        foreach (AbilityModTypes modableAttribute in ModHandler.GetModableAttributes(ability))
        {
            Button button = AddButtonToPanel(GeneralFunctions.GetCleanEnumString(modableAttribute), bottomLeftPanel, 30, 5);
            button.clicked += () => SetSelectedModAttribute(modableAttribute);
            button.clicked += () => SetInstructionsText($"Would you like to improve the {selectedModType} to {selectedAbility.Name}?");

        }
        Button statusEffectButton = AddButtonToPanel(GeneralFunctions.GetCleanEnumString(AbilityModTypes.StatusEffect), bottomLeftPanel, 30, 5);
        statusEffectButton.clicked += () => ShowStatusEffectOptionScreen();

    }
    private void ShowStatusEffectOptionScreen()
    {
        ClearPanel(bottomLeftPanel);
        StatusEffectType releventStatusEffect = StatusEffectsLibrary.GetStatusEffect(selectedAbility); //#TODO and what if the ability has no element?

        Button button = AddButtonToPanel(GeneralFunctions.GetCleanEnumString(releventStatusEffect), bottomLeftPanel, 30, 10);
        button.clicked += () => SetSelectedModAttribute(AbilityModTypes.StatusEffect, releventStatusEffect);

    }
    private void SetSelectedModAttribute(AbilityModTypes modType, StatusEffectType status = StatusEffectType.None)
    {
        selectedStatusEffect = null;

        selectedModType = modType;
        if (status != StatusEffectType.None)
        {
            selectedStatusEffect = AbilityLibrary.GetStatusEffects(selectedAbility.ElementTypes[0], status);
            //Debug.Log($"Selected status effect is being set to {selectedStatusEffect} for ability self");
            SetInstructionsText($"The {GeneralFunctions.GetCleanEnumString(modType)} of {selectedAbility.Name} can be improved for {AbilityModHandler.GetModCost(modType)} core power. You currently have {AlchemyInventory.AvailableCraftingPotential[CraftingPotential.CorePower]}");

        }

    }

    private void AttemptModification(AbilityModTypes modAttribute)
    {
        if (selectedAbility == null) return;
        if (ModHandler == null) return;
        if (selectedModType == AbilityModTypes.None && selectedStatusEffect == null) return;
        // Debug.Log($"selected mod = {selectedModType}. selected status effect = {selectedStatusEffect}");

        if (selectedModType != AbilityModTypes.None)
        {
            int price = AbilityModHandler.GetModCost(selectedModType);

            if (AlchemyInventory.AvailableCraftingPotential[CraftingPotential.CorePower] >= price)
            {
                AlchemyInventory.ExpendCraftingPotential(CraftingPotential.CorePower, price);
                NetworkAbilityMods.ModAttribute(selectedAbility, selectedModType, AbilityModHandler.GetModIncrementValue(selectedModType));
                if (selectedStatusEffect != null) NetworkAbilityMods.AddStatusEffect(selectedAbility, selectedStatusEffect);
                SetInstructionsText($"You have modified the {GeneralFunctions.GetCleanEnumString(selectedModType)} of {selectedAbility.Name} by {AbilityModHandler.GetModIncrementValue(selectedModType)} at the cost of {price} core power.");
            }
        }
        else if (selectedStatusEffect != null)
        {
            int thePrice = AbilityModHandler.GetModCost(AbilityModTypes.StatusEffect);
            if (AlchemyInventory.AvailableCraftingPotential[CraftingPotential.CorePower] >= AbilityModHandler.GetModCost(AbilityModTypes.StatusEffect))
            {
                NetworkAbilityMods.AddStatusEffect(selectedAbility, selectedStatusEffect);
                SetInstructionsText($"You have modified the {GeneralFunctions.GetCleanEnumString(selectedModType)} of {selectedAbility.Name} by {AbilityModHandler.GetModIncrementValue(selectedModType)} at the cost of {thePrice} core power.");
            }
        }
    }

    #endregion

    #region Crafting
    private bool CheckUsingCoresandOrgans()
    {
        return selectedCraftingPotential[CraftingPotential.OrganMass] > 0 && selectedCraftingPotential[CraftingPotential.CorePower] > 0;
    }

    private void Craft()
    {
        if (CheckUsingCoresandOrgans())
        {
            SetInstructionsText(alchemyHandler.HandleCraftingResults(selectedCraftingPotential, selectedElements));
            ClearCraftingSelection();
            ShowCraftingOptions();
        }

    }
    private void ShowCraftingOptions()
    {
        ClearAllPanels();
        ResetVars();
        if (AlchemyHandler.Instance.activeMinions.Count() == PlayerStats.Instance.TotalControlllableMinions)
        {
            SetInstructionsText("You already have your maximum number of minions. Upgrade your current minions, recycle and replace them, or acquire more minion slots.");
        }
        else
        {
            ShowCraftingInfo();
            SpawnCraftingPotentialButtons();
            //SpawnIngredientButtons();
            ShowElementToggles(bottomLeftPanel);
            Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);

            Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);
            clearButton.clicked += () => ClearCraftingSelection();
            clearButton.clicked += () => ShowCraftingInfo();
            clearButton.clicked += () => SpawnCraftingPotentialButtons();

            confirmButton.clicked += () => Craft();
            //confirmButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());
        }
    }
    #region Work In Progress with crafting potential
    private void SpawnCraftingPotentialButtons() // #TODO // make this flow with the other stuff
    {
        bottomLeftPanel.Clear();
        if (AlchemyInventory.AvailableCraftingPotential == null) throw new Exception("Crafting potential dict has not been initialized.");

        foreach (KeyValuePair<CraftingPotential, int> kvp in AlchemyInventory.AvailableCraftingPotential)
        {
            Button craftingPotentialButton = AddButtonToPanel($"{GeneralFunctions.GetCleanEnumString(kvp.Key)} : {kvp.Value}", bottomLeftPanel, 40, 5);
            craftingPotentialButton.clicked += () => UpdateSelectedCraftingPotentialInfo(craftingPotentialButton, kvp.Key);
            //craftingPotentialButton.RegisterCallback<ClickEvent>(e => OnCraftingPotentialAdded(craftingPotentialButton, kvp.Key));

        }
    }
    private void UpdateSelectedCraftingPotentialInfo(Button button, CraftingPotential potential)
    {
        if (selectedCraftingPotential[potential] + 10 > AlchemyInventory.AvailableCraftingPotential[potential]) return; // do nothing if adding 10 would be more than is available

        selectedCraftingPotential[potential] += 10;

        int newAvailableCP = AlchemyInventory.AvailableCraftingPotential[potential] - selectedCraftingPotential[potential]; // calc new available
        AlterButtonText(button, $"{GeneralFunctions.GetCleanEnumString(potential)} : {selectedCraftingPotential[potential]} / {AlchemyInventory.AvailableCraftingPotential[potential]} "); //display new available

        string craftinPotentialInfo = "Currently selected crafting potential:";
        foreach (KeyValuePair<CraftingPotential, int> kvp in selectedCraftingPotential)
        {
            craftinPotentialInfo += $"\n{GeneralFunctions.GetCleanEnumString(kvp.Key)} : {kvp.Value}";
        }

        SetInstructionsText(craftinPotentialInfo);
    }

    private void AlterButtonText(Button button, string newString)
    {
        button.text = newString;
    }
    #endregion



    private void ShowElementToggles(VisualElement panel)
    {
        ShowUI(panel);
        if (!elementsGenerated)
        {
            elementsGenerated = true;
            List<Element> elementsList = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();

            foreach (Element element in elementsList)
            {
                if (element != Element.None && AlchemyInventory.GetElementalKnowledge(element) >= minKnowledgeForElementToggle)
                {
                    TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();

                    Toggle elementToggle = prefabContainer.Q<Toggle>("TogglePrefab");
                    Debug.Log($"{elementToggle} found for {element}?");
                    elementToggle.AddToClassList("ElementToggle");
                    elementToggle.focusable = true; // the prefab turns it off, which leaves a controller unable to reach it
                    elementToggle.text = element.ToString();
                    elementToggle.style.width = Length.Percent(20);
                    elementToggle.style.height = Length.Percent(5);
                    //elementToggle.style.backgroundColor = Color.green;

                    panel.Add(elementToggle);
                    // ValueChanged also fires when a controller submits the toggle; ClickEvent only fires for the pointer.
                    elementToggle.RegisterValueChangedCallback(e => ToggleSelectedElement(element));
                }
            }
        }
    }

    private void ClearCraftingSelection()
    {
        selectedElements = new List<Element>();
        selectedCraftingPotential[CraftingPotential.OrganMass] = 0;
        selectedCraftingPotential[CraftingPotential.CorePower] = 0;
        selectedCraftingPotential[CraftingPotential.EtherDensity] = 0;


    }
    private void ToggleSelectedElement(Element element)
    {
        if (!selectedElements.Contains(element)) selectedElements.Add(element);
        else selectedElements.Remove(element);
    }


    #endregion

    #region Recycling
    private void ShowRecycleOptions()
    {
        ClearAllPanels();
        ResetVars();

        SetInstructionsText("Which Minion would you like to recycle for components?");
        ShowRecyclableMinions();
    }
    private void ShowRecyclableMinions()
    {
        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);

        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);
        confirmButton.clicked += () => AlchemyHandler.HandleMinionRecycling(minionToRecycle);
        clearButton.clicked += () => SetMinionToRecycle(null);
        foreach (MinionStats minion in alchemyHandler.activeMinions)
        {
            Debug.Log("Minion should be shown in alchemy inventory panel");
            Button minionButton = AddButtonToPanel($"Recycle {minion.Name}", bottomLeftPanel, 45, 5);
            minionButton.clicked += () => SetMinionToRecycle(minion);
        }
    }

    private void SetMinionToRecycle(MinionStats minion)
    {
        minionToRecycle = minion;
    }

    #endregion

    #region AbilityCrafting
    private void ShowAbilityCraftingOptions()
    {

        ClearAllPanels();
        ResetVars();

        SetInstructionsText("Select an ability to learn for which you have sufficient cores.");
        SpawnCraftableAbilityButtons();

        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);

        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);
        clearButton.clicked += () => SetInstructionsText("Select an ability to learn for which you have sufficient cores.");
        clearButton.clicked += () => SetSelectedAbility(null);


        confirmButton.clicked += () => LearnAbilityIfSufficientResources();
    }


    private void LearnAbilityIfSufficientResources()
    {
        if (selectedAbility == null) return;

        int price = Ability.GetCoreCraftingCost(selectedAbility);
        if (AlchemyInventory.AvailableCraftingPotential[CraftingPotential.CorePower] >= price)
        {
            AlchemyInventory.ExpendCraftingPotential(CraftingPotential.CorePower, price);

            SetInstructionsText($"You have concocted the ability {selectedAbility.name} with {price} core power!");
            EventDeclarer.PlayerLearnedAbility?.Invoke(selectedAbility);
            foreach (LivingBeing minion in AlchemyHandler.Instance.activeMinions)
            {
                if (minion.GetAffinity(selectedMinionAbility.ElementTypes[0]) > 50) // #TODO could potentially have no elements
                {
                    minion.GetComponent<AbilityHandler>().LearnAbility(selectedMinionAbility);
                }
            }
        }
        else SetInstructionsText($"You do not have sufficient core resources to concoct {selectedAbility.Name}.");


    }
    private void SpawnCraftableAbilityButtons()
    {
        SpawnCraftablePlayerAbilities();

        List<Element> minionElements = GetListMinionElements();

        SpawnCraftableMinionAbilityButtons(minionElements);

        //SetInstructionsText($"Confirm to learn the selected ability if you have sufficient core power. Currently you have {AlchemyInventory.GetCorePowerResource(selectedIngredients)} core power activated."))
    }
    private void SpawnCraftableMinionAbilityButtons(List<Element> minionElements)
    {
        foreach (AbilityLibrary_SO.ElementCategories category in AbilityLibrary.abilityLibrary.ElementalEntries)
        {
            if (minionElements.Contains(category.Element))
            {
                foreach (Ability ability in category.Abilities)
                {
                    Button abilityButton = AddButtonToPanel($"{ability.Name}", bottomLeftPanel, 70, 5);
                    //Debug.Log("made it this far2");

                    abilityButton.clicked += () => SetSelectedMinionAbility(ability);
                    abilityButton.clicked += () => ShowAbilityCraftingInfo();
                    abilityButton.RegisterCallback<PointerEnterEvent>(e => SetInstructionsText(ability.DisplayAbilityInfo()));
                }
            }
        }
    }
    private void SpawnCraftablePlayerAbilities()
    {
        TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
        foreach (Ability a in SetupManager.Instance.AllHealAbilities)
        {
            Button abilityButton = AddButtonToPanel($"{a.Name}", bottomLeftPanel, 10, 5);
            //Debug.Log("made it this far2");

            abilityButton.clicked += () => SetSelectedAbility(a);
            abilityButton.clicked += () => ShowAbilityCraftingInfo();
        }
        foreach (Ability ability in AbilityLibrary.abilityLibrary.GetElementalAbilitiesBelowLevel(LevelUpHandler.CraftingPowerScalar, AlchemyInventory.GetHighKnowledges()))
        {
            Button abilityButton = AddButtonToPanel($"{ability.Name}", bottomLeftPanel, 10, 5);
            //Debug.Log("made it this far2");

            abilityButton.clicked += () => SetSelectedAbility(ability);
            abilityButton.clicked += () => ShowAbilityCraftingInfo();

        }
    }
    private List<Element> GetListMinionElements()
    {
        List<Element> minionElements = new();
        foreach (LivingBeing minion in alchemyHandler.activeMinions)
        {
            Element potentialElement = minion.GetHighestAffinity(out float value, 50);
            if (potentialElement != Element.None && !minionElements.Contains(potentialElement))
            {
                minionElements.Add(potentialElement);
            }
        }
        return minionElements;
    }
    private void ShowAbilityCraftingInfo()
    {

        if (selectedAbility == null)
            SetInstructionsText("Select an ability which you would like to concoct. You have a {AlchemyInventory.GetCraftingPotential(CraftingPotential.CorePower)} core power.");
        else if (selectedAbility != null)
        {
            SetInstructionsText($"{selectedAbility.DisplayAbilityInfo()} \n Confirm to concoct {selectedAbility.Name} for {Ability.GetCoreCraftingCost(selectedAbility)} core power.");
        }
        SetInstructionsText($"You have {AlchemyInventory.GetCraftingPotential(CraftingPotential.CorePower)} core power.", true);

    }

    private void SetSelectedAbility(Ability ability)
    {
        selectedAbility = ability;
    }
    private void SetSelectedMinionAbility(Ability ability)
    {
        selectedMinionAbility = ability;
    }

    #endregion

    #region Ability slot control
    private void SlotAbilities()
    {
        ShowUI(topRightPanel);
        ClearAllPanels();
        ResetVars();

        SetInstructionsText("Select whose abilities you would like to slot.");
        Button slotPlayerAbilitiesButton = AddButtonToPanel("Player Abilities", bottomLeftPanel, 50, 5);
        slotPlayerAbilitiesButton.clicked += () => SlotLivingBeingAbilities(PlayerStats.Instance);

        foreach (MinionStats minion in AlchemyHandler.Instance.activeMinions)
        {
            Button button = AddButtonToPanel($"{minion.Name}", bottomLeftPanel, 50, 5);
            button.clicked += () => SlotLivingBeingAbilities(minion);
        }


        Button backButton = AddButtonToPanel("Back", topRightPanel, 50, 5);
        backButton.clicked += () => ShowDefaultScreen();

    }

    private void SlotLivingBeingAbilities(LivingBeing livingBeing)
    {
        ShowUI(topRightPanel);
        ClearAllPanels();
        ResetVars();
        //selectedAbilitySlotBeing = livingBeing;

        SpawnAbilitySlotButtons(livingBeing);
        SpawnKnownAbilityButtons();

        SetInstructionsText("Select an ability and a slot.");
        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);

        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);

        clearButton.clicked += () => SetInstructionsText("Select an ability and a slot.");
        clearButton.clicked += () => SetSelectedAbility(null);

        //Button setAbilitySlot = AddButtonToPanel("Set Ability Slot", confirmClear, 50, 5);

        confirmButton.clicked += () => HandleAbilityandSlotSelected(abilitySlot, selectedAbility, livingBeing);

    }
    private void HandleAbilityandSlotSelected(int abilitySlot, Ability selectedAbility, LivingBeing livingBeing)
    {
        if (selectedAbility == null) return;
        if (livingBeing == PlayerStats.Instance)
        {
            if (selectedAbility is DashAbility) PlayerAbilityHandler.SetDashAbility((DashAbility)selectedAbility);
            else EventDeclarer.SlotChanged?.Invoke(abilitySlot, selectedAbility);
        }
        else if (livingBeing.abilityHandler is CreatureAbilityHandler creatureAHandler) creatureAHandler.SlotAbility(selectedAbility, abilitySlot);

    }
    private void SpawnAbilitySlotButtons(LivingBeing livingBeing)
    {
        int maxSlots = 0;
        if (livingBeing.CharacterTag == CharacterTag.Player)
        {
            maxSlots = 6;
            for (int i = 0; i < maxSlots; i++)
            {
                int slotNum = i;
                Button slotButton = AddButtonToPanel($"Slot: {SlotNames[i]}", bottomLeftPanel, 20, 5);
                slotButton.clicked += () => SetSelectedSlot(slotNum);

            }
        }
        else foreach (SlottedAbilities slottedAbilities in livingBeing.abilityHandler.SlottedAbilities)
        {
            int slotNum = slottedAbilities.slot;
            Button slotButton = AddButtonToPanel($"Slot: {SlotNames[slotNum]}", bottomLeftPanel, 20, 5);
            slotButton.clicked += () => SetSelectedSlot(slotNum);
        }
    }
    private void SpawnKnownAbilityButtons()
    {
        if (playerAbilityHandler != null)
            foreach (Ability ability in playerAbilityHandler.Abilities) //null reference 
            {
                if (ability != null)
                {
                    Button abilityButton = AddButtonToPanel($"{ability.Name}", bottomLeftPanel, 50, 5);
                    abilityButton.clicked += () => SetSelectedAbility(ability);
                    abilityButton.clicked += () => SetAbilitySlottingInstructions(ability);
                }

            }
        else throw new Exception("The players ability handler variable has not been set properly in the alchmey UI bench, or it does not exist");
        //SetInstructionsText($"Slot selected: {abilitySlot}. Ability Selected for the slot: {selectedAbility.Name}");

    }
    private void SetAbilitySlottingInstructions(Ability ability)
    {
        if (ability is DashAbility) SetInstructionsText($"Confirm if you would like {ability.Name} to be used upon dashing.");

        else SetInstructionsText($"Select the slot in which you would like to store the ability {ability.Name}.");
    }
    private void SetSelectedSlot(int slotIndex)
    {
        abilitySlot = slotIndex;
        SetInstructionsText($"Slot selected: {abilitySlot}. Ability Selected for the slot: {selectedAbility}");
    }
    #endregion

    #region General UI functions
    public Button AddButtonToPanel(string buttonText, VisualElement panel, float width, float height)
    {
        TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
        Button button = prefabContainer.Q<Button>("ButtonPrefab");
        button.text = buttonText;

        panel.Add(button);
        SpawnedButtons.Add(button);
        int calculatedWidth = Math.Min(buttonText.Length * (int)width, 50);
        SetButtonSize(button, calculatedWidth, height);
        return button;
    }
    private void SetButtonSize(Button button, float width, float height)
    {
        button.style.width = Length.Percent(width);
        button.style.height = Length.Percent(height);
    }

    #region Flex /Hide UI

    private void ResetVars()
    {
        SetSelectedAbility(null);
        elementsGenerated = false;
        selectedElements.Clear();
        selectedModType = AbilityModTypes.None;
        selectedCraftingPotential[CraftingPotential.OrganMass] = 0;
        selectedCraftingPotential[CraftingPotential.CorePower] = 0;
        selectedCraftingPotential[CraftingPotential.EtherDensity] = 0;
    }



    private void ClearAllPanels(bool andTopLeft = false)
    {
        if (andTopLeft)
        {
            centerPanel.Clear();
            onDefaultScreen = false;
        }
        topRightPanel.Clear();
        bottomLeftPanel.Clear();
        foreach (var child in bottomRightPanel.Children().ToList())
        {
            if (child is not Label)
                child.RemoveFromHierarchy();
        }

    }

    private void SetInstructionsText(string words, bool add = false)
    {
        moreInfoContainer.style.display = DisplayStyle.Flex;

        if (add) moreInfoContainer.text += words;
        else
            moreInfoContainer.text = words;
    }


    private void HideUI(VisualElement element)
    {
        element.style.display = DisplayStyle.None;
    }
    private void ShowUI(VisualElement element)
    {
        element.style.display = DisplayStyle.Flex;
    }

    private void ClearPanel(VisualElement panel)
    {
        List<VisualElement> toRemove = new List<VisualElement>();

        foreach (var child in panel.Children())
        {

            toRemove.Add(child);

        }
        foreach (var child in toRemove)
        {
            panel.Remove(child);
        }
    }
    //TODO: clear panel / visual element 

    #endregion
    #endregion
}


