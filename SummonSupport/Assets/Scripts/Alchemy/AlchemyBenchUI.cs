#region Imports

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using SummonSupportEvents;



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
    private VisualElement mainCraftingOptions;
    private VisualElement confirmClear;
    private VisualElement craftandUpgrade;
    private VisualElement elementSelection;
    private VisualElement alchemyInventory;
    #region Ability crafting / slotting
    private VisualElement craftAbilityButton;
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

    private bool elementsGenerated = false;

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
        craftandUpgrade = craftingUI.Q<VisualElement>("CraftandUpgrade");
        mainCraftingOptions = craftingUI.Q<VisualElement>("MainCraftingOptions");
        confirmClear = craftingUI.Q<VisualElement>("ConfirmClear");
        alchemyInventory = craftandUpgrade.Q<VisualElement>("AlchemyInventory");
        craftAbilityButton = mainCraftingOptions.Q<VisualElement>("ConcoctButton");
        instructions = craftingUI.Q<Label>("Instructions");
        elementSelection = craftingUI.Q<VisualElement>("ElementSelection");
        interactWindow.style.display = DisplayStyle.None;
        craftingUI.style.display = DisplayStyle.None;

        ShowBackground();
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
        if (Player.TryGetComponent<PlayerAbilityHandler>(out PlayerAbilityHandler abilityHandler))
            playerAbilityHandler = abilityHandler;
        else return;
        HideInteractionOption();
        PlayerUsingUI();
        SetInstructionsText("");
        ShowUI(craftingUI);
        Button abilitySlotButton = craftingUI.Q<Button>("AbilitySlotButton");
        Button craftButton = craftingUI.Q<Button>("CraftButton");
        Button upgradeButton = craftingUI.Q<Button>("UpgradeButton");
        Button recycleButton = craftingUI.Q<Button>("RecycleButton");
        Button quitButton = craftingUI.Q<Button>("QuitButton");

        craftAbilityButton.RegisterCallback<ClickEvent>(e => ShowAbilityCraftingOptions());
        abilitySlotButton.RegisterCallback<ClickEvent>(e => SlotAbilities());

        craftButton.RegisterCallback<ClickEvent>(e => ShowCraftingOptions());
        upgradeButton.RegisterCallback<ClickEvent>(e => HandleUpgradeDisplay());
        recycleButton.RegisterCallback<ClickEvent>(e => ShowRecycleOptions());
        quitButton.RegisterCallback<ClickEvent>(e => QuitAlchemyUI());

    }
    private void QuitAlchemyUI()
    {
        HideUI(craftingUI);
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

    private void ShowRecycleOptions()
    {
        confirmClear.Clear();
        alchemyInventory.Clear();
        ClearElementSelection();
        instructions.text = "Which Minion would you like to recycle for components?";
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
                ingredientInfo += $"{kvp.Key}: {kvp.Value}. ";
            }
            instructions.text = ingredientInfo;
        }
        if (!CheckUsingCores()) instructions.text += $" You must have at least one core in order craft a minion, otherwise it is no more than a motionless husk!";
    }


    #endregion

    #region Upgrading 
    private void HandleUpgradeDisplay()
    {
        confirmClear.Clear();
        alchemyInventory.Clear();
        ClearElementSelection();
        ShowUpgradableMinions();
        ShowUpgradeInfo();
        Button confirmButton = AddButtonToPanel("Confirm", confirmClear, 50, 5);
        Button clearButton = AddButtonToPanel("Clear Selection", confirmClear, 50, 5);
        confirmButton.RegisterCallback<ClickEvent>(e => HandleUpgradeMinion(minionToUpgrade));
        clearButton.RegisterCallback<ClickEvent>(e => ClearCraftingSelection());
        clearButton.RegisterCallback<ClickEvent>(e => HandleUpgradeDisplay());
    }
    private void HandleUpgradeMinion(GameObject minionToUpgrade)
    {
        alchemyHandler.UpgradeMinion(minionToUpgrade, selectedIngredients, selectedElements);
        AlchemyInventory.ExpendIngredients(selectedIngredients);
        ClearCraftingSelection();
        HandleUpgradeDisplay();
    }
    private void ShowUpgradableMinions()
    {
        foreach (GameObject minion in alchemyHandler.activeMinions)
        {
            Button minionButton = AddButtonToPanel($"Upgrade {minion.GetComponent<LivingBeing>().Name}", alchemyInventory, 45, 5);
            minionButton.RegisterCallback<ClickEvent>(e => SetMinionToUpgrade(minion));
        }
    }
    private void SetMinionToUpgrade(GameObject minion)
    {
        minionToUpgrade = minion;
        instructions.text = $"Select ingredients and element with which to upgrade {minion.GetComponent<LivingBeing>().Name}.";
        SpawnIngredientButtons();
        ShowElementToggles(elementSelection);
    }
    #endregion

    #region Crafting
    private bool CheckUsingCores()
    {
        var ingredients = selectedIngredients.Keys;
        if (ingredients.Contains(AlchemyLoot.WeakCores) || ingredients.Contains(AlchemyLoot.PowerfulCore) || ingredients.Contains(AlchemyLoot.WorkingCore) || ingredients.Contains(AlchemyLoot.HulkingCore)) return true;
        else return false;
    }

    private void Craft()
    {
        if (CheckUsingCores())
        {
            AlchemyInventory.ExpendIngredients(selectedIngredients);
            alchemyHandler.HandleCraftingResults(selectedIngredients, selectedElements);
            ClearCraftingSelection();
        }
    }
    private void ShowCraftingOptions()
    {
        ShowUI(confirmClear);
        confirmClear.Clear();
        alchemyInventory.Clear();
        ClearElementSelection();

        ShowCraftingInfo();
        SpawnIngredientButtons();
        ShowElementToggles(elementSelection);

        Button clearButton = AddButtonToPanel("Clear Selection", confirmClear, 35, 5);
        clearButton.RegisterCallback<ClickEvent>(e => ClearCraftingSelection());
        clearButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());
        clearButton.RegisterCallback<ClickEvent>(e => SpawnIngredientButtons());

        Button confirmButton = AddButtonToPanel("Confirm", confirmClear, 35, 5);
        confirmButton.RegisterCallback<ClickEvent>(e => Craft());
        confirmButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());

    }
    private void SpawnIngredientButtons()
    {
        alchemyInventory.Clear();
        foreach (KeyValuePair<AlchemyLoot, int> kvp in AlchemyInventory.ingredients)
        {
            if (kvp.Value != 0)
            {
                Button ingredientButton = AddButtonToPanel($"{kvp.Key} : {kvp.Value}", alchemyInventory, 40, 5);
                //ingredientButton.style.backgroundColor = new Color(123, 100, 35, 100);

                ingredientButton.RegisterCallback<ClickEvent>(e => AddIngredientToSelection(kvp.Key));
                ingredientButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());
                ingredientButton.RegisterCallback<ClickEvent>(e => DecrementIngredientButton(ingredientButton));
            }
        }
    }

    private void DecrementIngredientButton(Button ingredientButton)
    {
        if (ingredientButton.text.EndsWith(": 1")) alchemyInventory.Remove(ingredientButton);
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
                TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
                Toggle elementToggle = prefabContainer.Q<Toggle>("TogglePrefab");
                elementToggle.text = element.ToString();
                //elementToggle.style.backgroundColor = Color.green;

                panel.Add(elementToggle);
                elementToggle.RegisterCallback<ClickEvent>(e => ToggleSelectedElement(element));
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

    #region AbilityCrafting
    private void ShowAbilityCraftingOptions()
    {
        ShowUI(confirmClear);
        confirmClear.Clear();
        alchemyInventory.Clear();
        ClearElementSelection();
        ClearCraftingSelection();

        SetInstructionsText("Select an ability to learn for which you have sufficient cores.");
        SpawnCraftableAbilityButtons();
        SpawnCoreButtons(alchemyInventory);

        Button clearButton = AddButtonToPanel("Clear Selection", confirmClear, 35, 5);
        clearButton.RegisterCallback<ClickEvent>(e => SetInstructionsText("Select an ability to learn for which you have sufficient cores."));
        clearButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(null));

        Button confirmButton = AddButtonToPanel("Confirm", confirmClear, 35, 5);

        confirmButton.RegisterCallback<ClickEvent>(e => LearnAbilityIfSufficientResources());
    }
    private void SpawnCoreButtons(VisualElement visualElement)
    {
        foreach (KeyValuePair<AlchemyLoot, int> kvp in AlchemyInventory.ingredients)
        {
            if (kvp.Key == AlchemyLoot.WeakCores || kvp.Key == AlchemyLoot.WorkingCore || kvp.Key == AlchemyLoot.PowerfulCore || kvp.Key == AlchemyLoot.HulkingCore)
            {
                if (kvp.Value != 0)
                {
                    Button ingredientButton = AddButtonToPanel($"{kvp.Key} : {kvp.Value}", visualElement, 40, 5);
                    ingredientButton.RegisterCallback<ClickEvent>(e => AddIngredientToSelection(kvp.Key));
                    ingredientButton.RegisterCallback<ClickEvent>(e => ShowAbilityCraftingInfo());
                    ingredientButton.RegisterCallback<ClickEvent>(e => DecrementIngredientButton(ingredientButton));
                }
            }
        }
    }
    private void LearnAbilityIfSufficientResources()
    {
        if (AlchemyInventory.HasSufficientCorePower(selectedAbility, selectedIngredients))
        {
            SetInstructionsText($"You have concocted the ability {selectedAbility.name}!");
            EventDeclarer.PlayerLearnedAbility?.Invoke(selectedAbility);
        }
        else SetInstructionsText($"You do not have sufficient core resources to concoct {selectedAbility.name}.");
    }
    private void SpawnCraftableAbilityButtons()
    {
        //alchemyInventory.Clear();
        foreach (AbilityLibrary_SO.PlayerAbilitiesByLevel abilityLibraryEntry in AbilityLibrary.abilityLibrary.abilitiesByLevelEntries)
        {
            if (abilityLibraryEntry.Level <= PlayerStats.Instance.CurrentLevel)
            {
                foreach (Ability ability in abilityLibraryEntry.Abilities)
                {
                    Button abilityButton = AddButtonToPanel($"{ability.Name} : {Ability.GetCoreCraftingCost(ability)} Core Power", alchemyInventory, 50, 5);
                    abilityButton.RegisterCallback<ClickEvent>(e => ShowAbilityCraftingInfo());

                    abilityButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(ability));
                }
            }
        }
        //SetInstructionsText($"Confirm to learn the selected ability if you have sufficient core power. Currently you have {AlchemyInventory.GetCorePowerResource(selectedIngredients)} core power activated."))
    }

    private void ShowAbilityCraftingInfo()
    {
        if (selectedIngredients.Keys.Count == 0)
            instructions.text = "Select cores to concoct the desired ability.";
        else
        {
            string ingredientInfo = $"Selected ability: {selectedAbility.name}. Selected Cores: ";

            foreach (KeyValuePair<AlchemyLoot, int> kvp in selectedIngredients)
            {
                ingredientInfo += $"{kvp.Key}: {kvp.Value}. ";
            }
            ingredientInfo += $"This provides {AlchemyInventory.GetCorePowerResource(selectedIngredients)} core power.";
            instructions.text = ingredientInfo;
        }

    }

    private void SetSelectedAbility(Ability ability)
    {
        selectedAbility = ability;
    }

    #endregion

    #region Ability slot control

    private void SlotAbilities()
    {
        ShowUI(confirmClear);
        confirmClear.Clear();
        alchemyInventory.Clear();
        ClearElementSelection();


        SpawnAbilitySlotButtons();
        SpawnKnownAbilityButtons();

        SetInstructionsText("Select an ability and a slot.");

        Button clearButton = AddButtonToPanel("Clear Selection", confirmClear, 35, 5);
        clearButton.RegisterCallback<ClickEvent>(e => SetInstructionsText("Select an ability and a slot."));
        clearButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(null));

        Button confirmButton = AddButtonToPanel("Confirm", confirmClear, 35, 5);
        //Button setAbilitySlot = AddButtonToPanel("Set Ability Slot", confirmClear, 50, 5);

        confirmButton.RegisterCallback<ClickEvent>(e => EventDeclarer.SlotChanged?.Invoke(abilitySlot, selectedAbility));

    }

    private void SpawnAbilitySlotButtons()
    {
        for (int i = 0; i < 5; i++)
        {
            int slotIndex = i;
            Button slotButton = AddButtonToPanel($"Slot: {slotIndex + 1}", alchemyInventory, 20, 5);
            slotButton.RegisterCallback<ClickEvent>(e => SetSelectedSlot(slotIndex));
        }
    }
    private void SpawnKnownAbilityButtons()
    {
        if (playerAbilityHandler != null)
            foreach (Ability ability in playerAbilityHandler.Abilities)
            {
                Button abilityButton = AddButtonToPanel($"{ability.name}", alchemyInventory, 50, 5);
                abilityButton.RegisterCallback<ClickEvent>(e => SetSelectedAbility(ability));
                SetInstructionsText($"Slot selected: {abilitySlot}. Ability Selected for the slot: {selectedAbility}");

            }
        else throw new Exception("The players ability handler variable has not been set properly in the alchmey UI bench, or it does not exist");
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

        SetButtonSize(button, width, height);
        return button;
    }
    private void SetButtonSize(Button button, int width, int height)
    {
        button.style.width = Length.Percent(width);
        button.style.height = Length.Percent(height);
    }

    #region Flex /Hide UI
    private void ClearElementSelection()
    {
        elementSelection.Clear();
        elementsGenerated = false;
    }

    private void ShowBackground(bool show = false)
    {
        if (show)
        {
            mainCraftingOptions.style.backgroundImage = new StyleBackground(alchemyBackground.texture);
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
