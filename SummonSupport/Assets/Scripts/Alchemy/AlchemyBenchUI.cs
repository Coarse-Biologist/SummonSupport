using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;
using Alchemy;
using System;
using System.Linq;

public class AlchemyBenchUI : MonoBehaviour
{
    #region Class Variables
    [SerializeField] UIDocument ui;
    private VisualElement root;
    private VisualElement interactWindow;
    private Label interactLabel;
    private VisualElement craftingUI;
    private VisualElement craftandUpgrade;
    private VisualElement elementSelection;
    private Label instructions;
    public UnityEvent playerUsingUI;
    private Dictionary<AlchemyLoot, int> selectedIngredients = new Dictionary<AlchemyLoot, int>();
    private List<Elements> selectedElements = new List<Elements>();
    private AlchemyHandler alchemyHandler;

    #endregion
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
        instructions = craftandUpgrade.Q<Label>("Instructions");
        interactLabel = interactWindow.Q<Label>("InteractLabel");
        elementSelection = craftingUI.Q<VisualElement>("ElementSelection");
        interactWindow.style.display = DisplayStyle.None;
        craftingUI.style.display = DisplayStyle.None;
    }
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

    #region begin and end workbench use
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
        upgradeButton.RegisterCallback<ClickEvent>(e => ShowUpgradeOptions());
        recycleButton.RegisterCallback<ClickEvent>(e => ShowRecycleOptions());
        quitButton.RegisterCallback<ClickEvent>(e => QuitAlchemyUI());

    }
    private void QuitAlchemyUI()
    {
        HideUI(craftingUI);
        PlayerUsingUI();
    }

    #endregion

    #region Buttons Clicked
    private void ShowCraftingOptions()
    {
        instructions.text = "Combine Cores, Ether and Body Parts to make minons of selected Elemental Affinity";
        foreach (KeyValuePair<AlchemyLoot, int> kvp in AlchemyInventory.ingredients)
        {
            if (kvp.Value != 0)
            {
                Button ingredientButton = new Button { text = $"{kvp.Key} : {kvp.Value}" }; // Set name of buttons
                craftandUpgrade.Add(ingredientButton); // add button to container
                SetButtonSize(ingredientButton, 20, 5);
                ingredientButton.RegisterCallback<ClickEvent>(e => AddIngredientToSelection(kvp.Key)); // add event for button
            }
        }
        ShowElementToggles(elementSelection);
        Button confirmButton = new Button { text = "Confirm Selection" };
        craftandUpgrade.Add(confirmButton);
        SetButtonSize(confirmButton, 20, 5);
        confirmButton.RegisterCallback<ClickEvent>(e => alchemyHandler.CalculateCraftingResults(selectedIngredients, selectedElements));

    }
    private void ShowElementToggles(VisualElement panel)
    {
        List<Elements> elementsList = Enum.GetValues(typeof(Elements)).Cast<Elements>().ToList();

        foreach (Elements element in elementsList)
        {
            Toggle elementToggle = new Toggle { text = element.ToString() };
            panel.Add(elementToggle);
            elementToggle.RegisterCallback<ClickEvent>(e => ToggleSelectedElement(element));
        }
    }
    private void SetButtonSize(Button button, int width, int height)
    {
        button.style.width = Length.Percent(width); // limit size of payment
        button.style.height = Length.Percent(height);
    }
    private void ShowUpgradeOptions()
    {
        instructions.text = "Which Minion would you like to upgrade?";
    }
    private void ShowRecycleOptions()
    {
        instructions.text = "Which Minion would you like to recycle for components?";
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

    #region Flex /Hide UI

    private void HideUI(VisualElement element)
    {
        element.style.display = DisplayStyle.None;
    }
    private void ShowUI(VisualElement element)
    {
        element.style.display = DisplayStyle.Flex;
    }

    #endregion

}
