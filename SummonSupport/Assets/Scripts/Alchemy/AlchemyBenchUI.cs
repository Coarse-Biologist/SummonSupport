#region Imports

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using SummonSupportEvents;
using static StatusEffectsLibrary;



#endregion

public class AlchemyBenchUI : MonoBehaviour, I_Interactable
{
    public static AlchemyBenchUI Instance { get; private set; }
    #region Class Variables

    #region Constants

    #region UI assets
    private VisualTreeAsset UIPrefabAssets;
    [SerializeField] Sprite alchemyBackground;

    #endregion

    private UIDocument ui;
    private VisualElement root;
    private VisualElement interactWindow;
    private VisualElement craftingUI;
    private VisualElement centerPanel;
    private VisualElement topRightPanel;
    private VisualElement bottomRightPanel;
    private VisualElement bottomLeftPanel;
    #region Ability crafting / slotting
    private Ability selectedAbility;
    private int abilitySlot;
    private PlayerAbilityHandler playerAbilityHandler;

    #endregion

    private Label instructions;
    public UnityEvent playerUsingUI = new UnityEvent();
    private AlchemyHandler alchemyHandler;

    #endregion

    #region Runtime Variables
    private Dictionary<AlchemyLoot, int> selectedIngredients = new Dictionary<AlchemyLoot, int>();
    private List<Element> selectedElements = new List<Element>();
    private GameObject minionToUpgrade;
    private GameObject minionToRecycle;

    #region ability modding
    private AbilityModHandler selectedModHandler;

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

    #endregion

    #endregion

    #region Setup

    //void OnEnable()
    //{
    //    EventDeclarer.TogglePauseGame?.AddListener();
    //}
    //void OnDisable()
    //{
    //    EventDeclarer.TogglePauseGame?.RemoveListener();
    //}

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
        bottomLeftPanel = craftingUI.Q<VisualElement>("BottomLeftPanel");
        //craftAbilityButton = mainCraftingOptions.Q<VisualElement>("ConcoctButton");

        instructions = craftingUI.Q<Label>("Instructions");
        bottomRightPanel = craftingUI.Q<VisualElement>("BottomRightPanel");
        interactWindow.style.display = DisplayStyle.None;
        craftingUI.style.display = DisplayStyle.None;

        //ShowBackground();
    }
    #endregion

    #region Trigger effects

    public void ShowInteractionOption()
    {
        InteractCanvasHandler.Instance.ShowInteractionOption(transform.position, "Tab to use alchemy bench  ");
    }

    public void HideInteractionOption()
    {
        InteractCanvasHandler.Instance.HideInteractionOption();
    }

    #endregion

    #region Begin and end workbench use
    private void PlayerUsingUI()
    {
        playerUsingUI?.Invoke();
    }
    public void Interact(GameObject Player)//WithWorkBench()
    {
        if (Player.TryGetComponent(out PlayerAbilityHandler abilityHandler))
            playerAbilityHandler = abilityHandler;
        else return;
        Time.timeScale = 0f;

        HideInteractionOption();
        PlayerUsingUI();
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

        playerManagementButton.RegisterCallback<ClickEvent>(e => ShowPlayerManagementOptions());
        abilityManagementOptions.RegisterCallback<ClickEvent>(e => ShowAbilityManagementOptions());
        minionManagementOptions.RegisterCallback<ClickEvent>(e => ShowMinionManagementOptions());
        quitButton.RegisterCallback<ClickEvent>(e => QuitAlchemyUI());
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

        backButton.RegisterCallback<ClickEvent>(e => ShowDefaultScreen());
        confirmButton.RegisterCallback<ClickEvent>(e => PlayerStats.Instance.GainLevelRewards(selectedPlayerUpgrades));
        clearButton.RegisterCallback<ClickEvent>(e => ClearSelectedPlayerUpgrades());


        ShowPlayerUpgradeOptions();

    }

    private void ShowPlayerUpgradeOptions()
    {
        foreach (LevelRewards reward in LevelUpHandler.RewardsCostDict.Keys)
        {
            Button button = AddButtonToPanel(LevelUpHandler.RewardsDescriptionDict[reward], bottomLeftPanel, 50, 5);
            button.RegisterCallback<ClickEvent>(e => TryAddSelectedPlayerUpgrade(reward));
            button.RegisterCallback<ClickEvent>(e => SetPlayerUpgradeInfo());
        }
    }

    private void TryAddSelectedPlayerUpgrade(LevelRewards reward)
    {
        int price = LevelUpHandler.RewardsCostDict[reward];

        if (selectedUpgradeCost + price < PlayerStats.Instance.SkillPoints)
        {
            selectedUpgradeCost += price;
            selectedPlayerUpgrades[reward] += 1;

        }
        else Debug.Log($"Insufficient skill-points for the upgrade {reward}");
    }

    private void ClearSelectedPlayerUpgrades()
    {
        foreach (KeyValuePair<LevelRewards, int> kvp in selectedPlayerUpgrades)
        {
            selectedPlayerUpgrades[kvp.Key] = 0;
        }
        SetInstructionsText($"Use skill-points to purchase upgrades. \nYou have {PlayerStats.Instance.SkillPoints} skill-points.");
    }

    private void SetPlayerUpgradeInfo()
    {
        Debug.Log($"trying to add text info");

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
    private void UpgradePlayer()
    {

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


        craftMinionsButton.RegisterCallback<ClickEvent>(e => ShowCraftingOptions());
        minionManagementOptions.RegisterCallback<ClickEvent>(e => HandleUpgradeDisplay());
        recycleButton.RegisterCallback<ClickEvent>(e => ShowRecycleOptions());
        backButton.RegisterCallback<ClickEvent>(e => ShowDefaultScreen());

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

        craftButton.RegisterCallback<ClickEvent>(e => ShowAbilityCraftingOptions());
        abilitySlotButton.RegisterCallback<ClickEvent>(e => SlotAbilities());
        abilityModButton.RegisterCallback<ClickEvent>(e => ShowAbilityModOptions());
        backButton.RegisterCallback<ClickEvent>(e => ShowDefaultScreen());
    }
    private void QuitAlchemyUI()
    {
        ClearAllPanels();
        ResetVars();
        HideUI(craftingUI);
        Time.timeScale = 1f;

        PlayerUsingUI();
    }

    #endregion

    #region Show Info in instructions panel
    private void ShowUpgradeInfo()
    {
        if (selectedIngredients.Keys.Count == 0)
            instructions.text = "Which Minion would you like to upgrade?";
        else
        {
            string ingredientInfo = "Selected Ingredients: ";

            foreach (KeyValuePair<AlchemyLoot, int> kvp in selectedIngredients)
            {
                ingredientInfo += $"{kvp.Key}: {kvp.Value}. ";
            }
            instructions.text = ingredientInfo;
        }
    }


    private void ShowCraftingInfo()
    {
        if (selectedIngredients.Keys.Count == 0)
            instructions.text = "Combine Cores, Ether and Body Parts to make minons of selected Elemental Affinity.";
        else
        {
            string ingredientInfo = "Selected Ingredients: ";

            foreach (KeyValuePair<AlchemyLoot, int> kvp in selectedIngredients)
            {
                ingredientInfo += $"{AlchemyInventory.GetAlchemyLootString(kvp.Key)}: {kvp.Value}. ";
            }
            instructions.text = ingredientInfo;
        }
        if (!CheckUsingCoresandOrgans()) instructions.text += $" You must use cores and organs in order to craft a minion.";

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
        confirmButton.RegisterCallback<ClickEvent>(e => HandleUpgradeMinion(minionToUpgrade));
        clearButton.RegisterCallback<ClickEvent>(e => ClearCraftingSelection());
        clearButton.RegisterCallback<ClickEvent>(e => HandleUpgradeDisplay());
    }
    private void HandleUpgradeMinion(GameObject minionToUpgrade)
    {
        if (minionToUpgrade == null) return;
        alchemyHandler.UpgradeMinion(minionToUpgrade, selectedIngredients, selectedElements);
        AlchemyInventory.ExpendIngredients(selectedIngredients);
        ClearCraftingSelection();
        HandleUpgradeDisplay();
    }
    private void ShowUpgradableMinions()
    {
        foreach (GameObject minion in alchemyHandler.activeMinions)
        {
            Button minionButton = AddButtonToPanel($"Upgrade {minion.GetComponent<LivingBeing>().Name}", bottomLeftPanel, 45, 5);
            minionButton.RegisterCallback<ClickEvent>(e => SetMinionToUpgrade(minion));
        }
    }
    private void SetMinionToUpgrade(GameObject minion)
    {
        minionToUpgrade = minion;
        instructions.text = $"Select ingredients and element with which to upgrade {minion.GetComponent<LivingBeing>().Name}.";
        SpawnIngredientButtons();
        ShowElementToggles(bottomRightPanel);
    }
    #endregion

    #region Modify Abilities

    private void ShowAbilityModOptions()
    {
        ClearAllPanels();
        ResetVars();

        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);
        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);

        clearButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(null));
        confirmButton.RegisterCallback<ClickEvent>(e => AttemptModification(selectedModType));
        confirmButton.RegisterCallback<ClickEvent>(e => DisplayAbilityModOptions(selectedAbility));

        SetInstructionsText("Select an ability to modify.");
        DisplayAllModableAbilities();
    }

    private void DisplayAllModableAbilities()
    {

        ShowUI(bottomLeftPanel);
        if (PlayerStats.Instance.TryGetComponent<AbilityHandler>(out AbilityHandler abilityHandler) && PlayerStats.Instance.TryGetComponent(out AbilityModHandler playerModHandler))
            foreach (Ability ability in abilityHandler.Abilities)
            {
                ///Ability potentiallySelectedAbility = ability;
                Button button = AddButtonToPanel($"{ability.Name}", bottomLeftPanel, 40, 5);
                button.RegisterCallback<ClickEvent>(e => SetSelectedAbility(ability));
                button.RegisterCallback<ClickEvent>(e => SetSelectedModHandler(playerModHandler));
                button.RegisterCallback<ClickEvent>(e => DisplayAbilityModOptions(ability));
            }
        foreach (GameObject minion in alchemyHandler.activeMinions)
        {
            if (minion.TryGetComponent(out CreatureAbilityHandler creatureAbilityHandler) && minion.TryGetComponent(out AbilityModHandler modHandler))
                foreach (Ability ability in creatureAbilityHandler.Abilities)
                {
                    Ability potentiallySelectedAbility = ability;
                    Button button = AddButtonToPanel($"{ability.Name}", bottomLeftPanel, 20, 5);
                    button.RegisterCallback<ClickEvent>(e => SetSelectedModHandler(modHandler));
                    button.RegisterCallback<ClickEvent>(e => SetSelectedAbility(potentiallySelectedAbility));
                    button.RegisterCallback<ClickEvent>(e => DisplayAbilityModOptions(potentiallySelectedAbility));
                }
        }
    }
    private void DisplayAbilityModOptions(Ability ability)
    {
        if (selectedAbility == null) return;
        ClearPanel(bottomRightPanel);
        SetInstructionsText($"Select an attribute for the ability which you would like to upgrade.");
        foreach (AbilityModTypes modableAttribute in selectedModHandler.GetModableAttributes(ability))
        {
            Button button = AddButtonToPanel(AbilityModHandler.GetCleanEnumString(modableAttribute), bottomRightPanel, 40, 10);
            button.RegisterCallback<ClickEvent>(e => SetSelectedModAttribute(modableAttribute));
        }
        Button statuseffectButton = AddButtonToPanel(AbilityModHandler.GetCleanEnumString(AbilityModTypes.StatusEffect), bottomRightPanel, 40, 10);
        statuseffectButton.RegisterCallback<ClickEvent>(e => ShowStatusEffectOptionScreen());

    }
    private void ShowStatusEffectOptionScreen()
    {
        ClearPanel(bottomRightPanel);
        StatusEffectsLibrary library = AbilityLibrary.GetStatusEffectLibrary();
        if (library != null)
        {
            foreach (EffectEntry entry in library.entries)
            {
                Button button = AddButtonToPanel(AbilityModHandler.GetCleanEnumString(entry.Type), bottomRightPanel, 40, 10);
                button.RegisterCallback<ClickEvent>(e => SetSelectedModAttribute(entry.Effect));
            }
        }
    }
    private void SetSelectedModAttribute(AbilityModTypes modType)
    {
        selectedModType = modType;
        selectedStatusEffect = null;
        SetInstructionsText($"The {AbilityModHandler.GetCleanEnumString(modType)} of {selectedAbility.Name} can be improved for {AbilityModHandler.GetModCost(modType)} core power. You currently have {AlchemyInventory.GetCorePowerResource(AlchemyInventory.ingredients)}");

    }
    private void SetSelectedModAttribute(StatusEffects modType)
    {
        selectedStatusEffect = modType;
        selectedModType = AbilityModTypes.None;
        SetInstructionsText($"The {AbilityModHandler.GetCleanEnumString(modType)} of {selectedAbility.Name} can be improved for {AbilityModHandler.GetModCost(modType.EffectType)} core power. You currently have {AlchemyInventory.GetCorePowerResource(AlchemyInventory.ingredients)}");

    }
    private void AttemptModification(AbilityModTypes modAttribute)
    {
        if (selectedAbility == null) return;
        if (selectedModHandler == null) return;
        if (selectedModType == AbilityModTypes.None && selectedStatusEffect == null) return;
        Debug.Log($"selected mod = {selectedModType}. selected status effect = {selectedStatusEffect}");

        if (selectedModType != AbilityModTypes.None)
        {
            var boughtPrice = AlchemyInventory.BuyCraftingPowerWithCores(AbilityModHandler.GetModCost(selectedModType));
            if (boughtPrice.bought)
            {
                selectedModHandler.ModAttributeByType(selectedAbility, selectedModType, AbilityModHandler.GetModIncrementValue(selectedModType));
                SetInstructionsText($"You have modified the {AbilityModHandler.GetCleanEnumString(selectedModType)} of {selectedAbility.Name} by {AbilityModHandler.GetModIncrementValue(selectedModType)} at the cost of {boughtPrice.price} core power.");
            }
        }
        else if (selectedStatusEffect != null)
        {
            var boughtPrice = AlchemyInventory.BuyCraftingPowerWithCores(AbilityModHandler.GetModCost(selectedStatusEffect.EffectType));
            if (boughtPrice.bought)
            {
                selectedModHandler.AddStatusEffectToAbility(selectedAbility, selectedStatusEffect);
                SetInstructionsText($"You have modified the {AbilityModHandler.GetCleanEnumString(selectedModType)} of {selectedAbility.Name} by {AbilityModHandler.GetModIncrementValue(selectedModType)} at the cost of {boughtPrice.price} core power.");
            }
        }
    }
    private void SetSelectedModHandler(AbilityModHandler modHandler)
    {
        selectedModHandler = modHandler;
    }

    #endregion

    #region Crafting
    private bool CheckUsingCoresandOrgans()
    {
        string ingredients = "";
        foreach (var loot in selectedIngredients.Keys)
            ingredients += loot.ToString();
        if (ingredients.Contains("Core") && ingredients.Contains("Organs")) return true;
        else return false;
    }

    private void Craft()
    {
        if (CheckUsingCoresandOrgans())
        {
            alchemyHandler.HandleCraftingResults(selectedIngredients, selectedElements);
            ClearCraftingSelection();
            ShowCraftingOptions();
        }

    }
    private void ShowCraftingOptions()
    {
        ClearAllPanels();
        ResetVars();
        if (CommandMinion.activeMinions.Count() == PlayerStats.Instance.TotalControlllableMinions)
        {
            instructions.text = "You already have your maximum number of minions. Upgrade your current minions, recycle and replace them, or acquire more minion slots.";
        }
        else
        {
            ShowCraftingInfo();
            SpawnIngredientButtons();
            ShowElementToggles(bottomRightPanel);
            Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);

            Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);
            clearButton.RegisterCallback<ClickEvent>(e => ClearCraftingSelection());
            clearButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());
            clearButton.RegisterCallback<ClickEvent>(e => SpawnIngredientButtons());

            confirmButton.RegisterCallback<ClickEvent>(e => Craft());
            confirmButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());
        }
    }
    private void SpawnIngredientButtons()
    {
        bottomLeftPanel.Clear();
        foreach (KeyValuePair<AlchemyLoot, int> kvp in AlchemyInventory.ingredients)
        {
            if (kvp.Value != 0)
            {
                Button ingredientButton = AddButtonToPanel($"{AlchemyInventory.GetAlchemyLootString(kvp.Key)} : {kvp.Value}", bottomLeftPanel, 40, 5);
                ingredientButton.RegisterCallback<ClickEvent>(e => AddIngredientToSelection(kvp.Key));
                ingredientButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());
                ingredientButton.RegisterCallback<ClickEvent>(e => DecrementIngredientButton(ingredientButton));
            }
        }
    }

    private void DecrementIngredientButton(Button ingredientButton)
    {
        if (ingredientButton.text.EndsWith(": 1")) bottomLeftPanel.Remove(ingredientButton);
        else
            ingredientButton.text = Regex.Replace(ingredientButton.text, @"\d+", match =>
            {
                int number = int.Parse(match.Value); // Convert found number to int
                return (number - 1).ToString(); // Decrement and replace
            });
    }
    private void ShowElementToggles(VisualElement panel)
    {
        ShowUI(panel);
        if (!elementsGenerated)
        {
            elementsGenerated = true;
            List<Element> elementsList = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
            foreach (Element element in elementsList)
            {
                if (AlchemyInventory.GetElementalKnowledge(element) >= 50)
                {
                    TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
                    Toggle elementToggle = prefabContainer.Q<Toggle>("TogglePrefab");
                    elementToggle.text = element.ToString();
                    elementToggle.style.width = Length.Percent(30);
                    elementToggle.style.height = Length.Percent(10);
                    //elementToggle.style.backgroundColor = Color.green;

                    panel.Add(elementToggle);
                    elementToggle.RegisterCallback<ClickEvent>(e => ToggleSelectedElement(element));
                }
            }
        }
    }

    private void ClearCraftingSelection()
    {
        selectedElements = new List<Element>();
        selectedIngredients = new Dictionary<AlchemyLoot, int>();
    }
    private void ToggleSelectedElement(Element element)
    {
        if (!selectedElements.Contains(element)) selectedElements.Add(element);
        else selectedElements.Remove(element);
    }
    private void AddIngredientToSelection(AlchemyLoot ingredient)
    {
        if (selectedIngredients.TryGetValue(ingredient, out int amountSelected)) // if the key exists
        {
            if (amountSelected < AlchemyInventory.ingredients[ingredient]) //if not already equal or more have been selected
                selectedIngredients[ingredient]++;
        }
        else selectedIngredients.Add(ingredient, 1);
    }

    #endregion

    #region Recycling
    private void ShowRecycleOptions()
    {
        ClearAllPanels();
        ResetVars();

        instructions.text = "Which Minion would you like to recycle for components?";
        ShowRecyclableMinions();
    }
    private void ShowRecyclableMinions()
    {
        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);

        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);
        confirmButton.RegisterCallback<ClickEvent>(e => AlchemyHandler.HandleMinionRecycling(minionToRecycle));
        clearButton.RegisterCallback<ClickEvent>(e => SetMinionToRecycle(null));
        foreach (GameObject minion in alchemyHandler.activeMinions)
        {
            Debug.Log("Minion should be shown in alchemy inventory panel");
            Button minionButton = AddButtonToPanel($"Recycle {minion.GetComponent<LivingBeing>().Name}", bottomLeftPanel, 45, 5);
            minionButton.RegisterCallback<ClickEvent>(e => SetMinionToRecycle(minion));
        }
    }

    private void SetMinionToRecycle(GameObject minion)
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
        clearButton.RegisterCallback<ClickEvent>(e => SetInstructionsText("Select an ability to learn for which you have sufficient cores."));
        clearButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(null));


        confirmButton.RegisterCallback<ClickEvent>(e => LearnAbilityIfSufficientResources());
    }


    private void LearnAbilityIfSufficientResources()
    {
        if (selectedAbility == null) return;

        var boughtPrice = AlchemyInventory.BuyCraftingPowerWithCores(Ability.GetCoreCraftingCost(selectedAbility));
        if (boughtPrice.bought)
        {
            SetInstructionsText($"You have concocted the ability {selectedAbility.name} with {boughtPrice.price} core power!");
            EventDeclarer.PlayerLearnedAbility?.Invoke(selectedAbility);
        }
        else SetInstructionsText($"You do not have sufficient core resources to concoct {selectedAbility.name}.");

    }
    private void SpawnCraftableAbilityButtons()
    {

        foreach (AbilityLibrary_SO.PlayerAbilitiesByLevel abilityLibraryEntry in AbilityLibrary.abilityLibrary.abilitiesByLevelEntries)
        {
            if (abilityLibraryEntry.Level <= PlayerStats.Instance.CurrentLevel)
            {
                foreach (Ability ability in abilityLibraryEntry.Abilities)
                {
                    Button abilityButton = AddButtonToPanel($"{ability.Name} : {Ability.GetCoreCraftingCost(ability)} Core Power", bottomLeftPanel, 70, 5);
                    Debug.Log("made it this far2");

                    abilityButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(ability));
                    abilityButton.RegisterCallback<ClickEvent>(e => ShowAbilityCraftingInfo());

                }
            }
        }
        //SetInstructionsText($"Confirm to learn the selected ability if you have sufficient core power. Currently you have {AlchemyInventory.GetCorePowerResource(selectedIngredients)} core power activated."))
    }

    private void ShowAbilityCraftingInfo()
    {

        if (selectedAbility == null)
            instructions.text = "Select an ability which you would like to concoct.";
        else if (selectedAbility != null)
        {
            instructions.text = $"Confirm to concoct {selectedAbility.Name} for {Ability.GetCoreCraftingCost(selectedAbility)} core power.";
        }
        instructions.text += $" You have a total of {AlchemyInventory.GetCorePowerResource(AlchemyInventory.ingredients)} core power.";

    }

    private void SetSelectedAbility(Ability ability)
    {
        selectedAbility = ability;
    }

    #endregion

    #region Ability slot control

    private void SlotAbilities()
    {
        ShowUI(topRightPanel);
        ClearAllPanels();
        ResetVars();


        SpawnAbilitySlotButtons();
        SpawnKnownAbilityButtons();

        SetInstructionsText("Select an ability and a slot.");
        Button confirmButton = AddButtonToPanel("Confirm", topRightPanel, 50, 5);

        Button clearButton = AddButtonToPanel("Clear Selection", topRightPanel, 50, 5);
        clearButton.RegisterCallback<ClickEvent>(e => SetInstructionsText("Select an ability and a slot."));
        clearButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(null));

        //Button setAbilitySlot = AddButtonToPanel("Set Ability Slot", confirmClear, 50, 5);

        confirmButton.RegisterCallback<ClickEvent>(e => HandleAbilityandSlotSelected(abilitySlot, selectedAbility));

    }
    private void HandleAbilityandSlotSelected(int abilitySlot, Ability selectedAbility)
    {
        if (selectedAbility is DashAbility) PlayerAbilityHandler.SetDashAbility((DashAbility)selectedAbility);
        else EventDeclarer.SlotChanged?.Invoke(abilitySlot, selectedAbility);

    }
    private void SpawnAbilitySlotButtons()
    {
        for (int i = 0; i < 5; i++)
        {
            int slotIndex = i;
            Button slotButton = AddButtonToPanel($"Slot: {slotIndex + 1}", bottomLeftPanel, 20, 5);
            slotButton.RegisterCallback<ClickEvent>(e => SetSelectedSlot(slotIndex));
        }
    }
    private void SpawnKnownAbilityButtons()
    {
        if (playerAbilityHandler != null)
            foreach (Ability ability in playerAbilityHandler.Abilities)
            {
                Button abilityButton = AddButtonToPanel($"{ability.name}", bottomLeftPanel, 50, 5);
                abilityButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(ability));
                abilityButton.RegisterCallback<ClickEvent>(e => SetAbilitySlottingInstructions(ability));

            }
        else throw new Exception("The players ability handler variable has not been set properly in the alchmey UI bench, or it does not exist");
        //SetInstructionsText($"Slot selected: {abilitySlot}. Ability Selected for the slot: {selectedAbility.Name}");

    }
    private void SetAbilitySlottingInstructions(Ability ability)
    {
        if (ability is DashAbility) instructions.text = $"Confirm if you would like {ability.Name} to be used upon dashing.";

        else instructions.text = $"Select the slot in which you would like to store the ability {ability.Name}.";
    }
    private void SetSelectedSlot(int slotIndex)
    {
        abilitySlot = slotIndex;
        SetInstructionsText($"Slot selected: {abilitySlot}. Ability Selected for the slot: {selectedAbility}");
    }
    #endregion

    #region General UI functions
    private Button AddButtonToPanel(string buttonText, VisualElement panel, int width, int height)
    {
        TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
        Button button = prefabContainer.Q<Button>("ButtonPrefab");
        button.text = buttonText;

        panel.Add(button);
        SpawnedButtons.Add(button);

        SetButtonSize(button, width, height);
        return button;
    }
    private void SetButtonSize(Button button, int width, int height)
    {
        button.style.width = Length.Percent(width);
        button.style.height = Length.Percent(height);
    }

    #region Flex /Hide UI

    private void ResetVars()
    {
        SetSelectedAbility(null);
        selectedElements.Clear();
        selectedIngredients.Clear();
        selectedModType = AbilityModTypes.None;
        selectedModHandler = null;

    }

    private void ClearAllPanels(bool andTopLeft = false)
    {
        if (andTopLeft) centerPanel.Clear();
        topRightPanel.Clear();
        bottomLeftPanel.Clear();
        bottomRightPanel.Clear();
    }

    private void ShowBackground(bool show = false)
    {
        if (show)
        {
            centerPanel.style.backgroundImage = new StyleBackground(alchemyBackground.texture);
        }

    }
    private void SetInstructionsText(string words)
    {
        instructions.text = words;
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


//private void SpawnCoreButtons(VisualElement visualElement)
//{
//    foreach (KeyValuePair<AlchemyLoot, int> kvp in AlchemyInventory.ingredients)
//    {
//        if (kvp.Key == AlchemyLoot.WeakCore || kvp.Key == AlchemyLoot.WorkingCore || kvp.Key == AlchemyLoot.PowerfulCore || kvp.Key == AlchemyLoot.HulkingCore)
//        {
//            if (kvp.Value != 0)
//            {
//                Button ingredientButton = AddButtonToPanel($"{kvp.Key} : {kvp.Value}", visualElement, 40, 5);
//                ingredientButton.RegisterCallback<ClickEvent>(e => AddIngredientToSelection(kvp.Key));
//                ingredientButton.RegisterCallback<ClickEvent>(e => ShowAbilityCraftingInfo());
//                ingredientButton.RegisterCallback<ClickEvent>(e => DecrementIngredientButton(ingredientButton));
//            }
//        }
//    }
//}
//private void LearnAbilityIfSufficientResources()
//{
//    if (selectedAbility == null || selectedIngredients.Count() == 0) return;
//    if (AlchemyInventory.HasSufficientCorePower(selectedAbility, selectedIngredients))
//    {
//        SetInstructionsText($"You have concocted the ability {selectedAbility.name}!");
//        EventDeclarer.PlayerLearnedAbility?.Invoke(selectedAbility);
//    }
//    else SetInstructionsText($"You do not have sufficient core resources to concoct {selectedAbility.name}.");
//}

//{
//    string ingredientInfo = $"Selected ability: {selectedAbility.name}. Selected Cores: ";
//
//    foreach (KeyValuePair<AlchemyLoot, int> kvp in selectedIngredients)
//    {
//        ingredientInfo += $"{kvp.Key}: {kvp.Value}. ";
//    }
//    ingredientInfo += $"This provides {AlchemyInventory.GetCorePowerResource(selectedIngredients)} core power.";
//    instructions.text = ingredientInfo;
//}