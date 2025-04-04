#region Imports

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;
using Alchemy;
using System;
using System.Linq;
using System.Text.RegularExpressions;


#endregion

public class AlchemyBenchUI : MonoBehaviour
{
    #region Class Variables

    #region Constants
    [SerializeField] UIDocument ui;
    private VisualElement root;
    private VisualElement interactWindow;
    private Label interactLabel;
    private VisualElement craftingUI;
    private VisualElement confirmClear;
    private VisualElement craftandUpgrade;
    private VisualElement elementSelection;
    private VisualElement alchemyInventory;
    private Label instructions;
    public UnityEvent playerUsingUI;
    private AlchemyHandler alchemyHandler;

    #endregion

    #region Runtime Variables
    private Dictionary<AlchemyLoot, int> selectedIngredients = new Dictionary<AlchemyLoot, int>();
    private List<Elements> selectedElements = new List<Elements>();
    private GameObject minionToUpgrade;

    private bool crafting = false;
    private bool upgrading = false;
    private bool recycling = false;
    private bool elementsGenerated = false;

    #endregion

    #endregion

    #region Setup
    void Awake()
    {
        alchemyHandler = GetComponent<AlchemyHandler>();
    }
    void Start()
    {
        root = ui.rootVisualElement;
        interactWindow = root.Q<VisualElement>("Interact");
        craftingUI = root.Q<VisualElement>("CraftingUI");
        craftandUpgrade = craftingUI.Q<VisualElement>("CraftandUpgrade");
        confirmClear = craftingUI.Q<VisualElement>("ConfirmClear");
        alchemyInventory = craftandUpgrade.Q<VisualElement>("AlchemyInventory");
        instructions = craftandUpgrade.Q<Label>("Instructions");
        interactLabel = interactWindow.Q<Label>("InteractLabel");
        elementSelection = craftingUI.Q<VisualElement>("ElementSelection");
        interactWindow.style.display = DisplayStyle.None;
        craftingUI.style.display = DisplayStyle.None;
    }
    #endregion

    #region Trigger effects
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            interactWindow.style.display = DisplayStyle.Flex;
            interactLabel.text = "Press Tab to Interact";
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            InteractWithWorkBench();
            PlayerUsingUI();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            interactWindow.style.display = DisplayStyle.None;
            craftingUI.style.display = DisplayStyle.None;
        }
    }

    #endregion

    #region Begin and end workbench use
    private void PlayerUsingUI()
    {
        playerUsingUI?.Invoke();
    }
    private void InteractWithWorkBench()
    {
        PlayerUsingUI();
        ShowUI(craftingUI);
        Button craftButton = craftingUI.Q<Button>("CraftButton");
        Button upgradeButton = craftingUI.Q<Button>("UpgradeButton");
        Button recycleButton = craftingUI.Q<Button>("RecycleButton");
        Button quitButton = craftingUI.Q<Button>("QuitButton");

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
        instructions.text = "Which Minion would you like to recycle for components?";
    }
    private void ShowCraftingInfo()
    {
        if (selectedIngredients.Keys.Count == 0)
            instructions.text = "Combine Cores, Ether and Body Parts to make minons of selected Elemental Affinity";
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


    #endregion

    #region Upgrading 
    private void HandleUpgradeDisplay()
    {
        confirmClear.Clear();
        alchemyInventory.Clear();
        elementSelection.Clear();
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
        Logging.Info($"Number of active minions = {alchemyHandler.activeMinions.Count}");
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

    private void Craft()
    {
        AlchemyInventory.ExpendIngredients(selectedIngredients);
        alchemyHandler.HandleCraftingResults(selectedIngredients, selectedElements);
        ClearCraftingSelection();
        //instructions.text = results;
    }
    private void ShowCraftingOptions()
    {
        ShowUI(confirmClear);
        confirmClear.Clear();
        alchemyInventory.Clear();
        elementSelection.Clear();

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
                ingredientButton.RegisterCallback<ClickEvent>(e => AddIngredientToSelection(kvp.Key));
                ingredientButton.RegisterCallback<ClickEvent>(e => ShowCraftingInfo());
                ingredientButton.RegisterCallback<ClickEvent>(e => DecrementIngredientButton(ingredientButton));
            }
        }
    }

    private void DecrementIngredientButton(Button ingredientButton)
    {
        if (ingredientButton.text.Contains("1")) alchemyInventory.Remove(ingredientButton);
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
            List<Elements> elementsList = Enum.GetValues(typeof(Elements)).Cast<Elements>().ToList();
            foreach (Elements element in elementsList)
            {
                Toggle elementToggle = new Toggle { text = element.ToString() };
                panel.Add(elementToggle);
                elementToggle.RegisterCallback<ClickEvent>(e => ToggleSelectedElement(element));
            }
        }

    }

    private void ClearCraftingSelection()
    {
        selectedElements = new List<Elements>();
        selectedIngredients = new Dictionary<AlchemyLoot, int>();
    }
    private void ToggleSelectedElement(Elements element)
    {
        if (!selectedElements.Contains(element)) selectedElements.Add(element);
        else selectedElements.Remove(element);
    }
    private void AddIngredientToSelection(AlchemyLoot ingredient)
    {

        if (selectedIngredients.TryGetValue(ingredient, out int amountSelected)) // if the key exists
        {
            if (amountSelected < AlchemyInventory.ingredients[ingredient]) //if not already equal or more have been selected
            {
                selectedIngredients[ingredient]++;
                Logging.Info($"{ingredient} is being added to the selectedIngredietns list. Currrent value = {selectedIngredients[ingredient]}");
            }
            Logging.Info($"the current amount selected({amountSelected}) is already equal to the total you have available");
        }
        else selectedIngredients.Add(ingredient, 1);
    }

    #endregion

    #region General UI functions
    private Button AddButtonToPanel(string buttonText, VisualElement panel, int width, int height)
    {
        Button button = new Button { text = buttonText };
        panel.Add(button);
        SetButtonSize(button, width, height);
        return button;
    }
    private void SetButtonSize(Button button, int width, int height)
    {
        button.style.width = Length.Percent(width); // limit size of payment
        button.style.height = Length.Percent(height);
    }

    #region Flex /Hide UI

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
