using System.Collections.Generic;
using SummonSupportEvents;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseGameHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool paused { set; private get; } = false;

    private UIDocument ui;

    public VisualTreeAsset UIPrefabAssets { get; private set; }

    private VisualElement root;
    private VisualElement PauseMenu;
    private VisualElement ButtonMenu;

    private Button ResumeButton;
    private Button RestartButton;
    private Button InventoryButton;
    private Button StatsButton;

    private Label InfoElement;
    private VisualElement MainUI;
    private VisualElement DialogueUI;
    private VisualElement PlayerOptions;
    private int statIndex = 0;
    private int invIndex = 0;


    private Button QuitButton;
    void OnEnable()
    {
        EventDeclarer.TogglePauseGame?.AddListener(ToggleGamePause);
        EventDeclarer.PlayerDead?.AddListener(DeathPause);
        EventDeclarer.PlayerLevelUp?.AddListener(LevelUpPause);



    }
    void OnDisable()
    {
        EventDeclarer.TogglePauseGame?.RemoveListener(ToggleGamePause);
        EventDeclarer.PlayerDead?.RemoveListener(DeathPause);
        EventDeclarer.PlayerLevelUp?.RemoveListener(LevelUpPause);


    }

    void Start()
    {
        ui = UI_DocHandler.Instance.ui;
        UIPrefabAssets = UI_DocHandler.Instance.UIPrefabAssets;
        root = ui.rootVisualElement;
        PauseMenu = root.Q<VisualElement>("PauseMenu");
        //ButtonMenu = PauseMenu.Q<VisualElement>("ButtonMenu");
        ResumeButton = PauseMenu.Q<Button>("Resume");
        InventoryButton = PauseMenu.Q<Button>("Inventory");
        RestartButton = PauseMenu.Q<Button>("Restart");
        QuitButton = PauseMenu.Q<Button>("Quit");
        StatsButton = PauseMenu.Q<Button>("ShowStats");
        
        MainUI = root.Q<VisualElement>("MainUI");
        Debug.Log($"main ui: {PauseMenu}");

        DialogueUI = MainUI.Q<VisualElement>("Dialogue");
        Debug.Log($"dialogue ui: {DialogueUI}");
        PlayerOptions = MainUI.Q<VisualElement>("PlayerOptions");
        Debug.Log($"playeroptions : {PlayerOptions}");



        //InfoScrollElement = PauseMenu.Q<ScrollView>("ScrollView");
        //Debug.Log($"InfoScrollElement: {InfoScrollElement}");

        InfoElement = PlayerOptions.Q<Label>("Info");
        Debug.Log($"InfoElement: {InfoElement}");

        ResumeButton.RegisterCallback<ClickEvent>(e => Resume());
        RestartButton.RegisterCallback<ClickEvent>(e => Restart());

        StatsButton.RegisterCallback<ClickEvent>(e => ChangeStatIndex());
        InventoryButton.RegisterCallback<ClickEvent>(e => ShowInventory());
        InfoElement.text = "";

    }
    private void ToggleGamePause()
    {
        paused = !paused;

        if (paused)
        {
            Pause();
        }
        else
        {
            Resume();
        }

    }


    private void Pause()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = true;

        PauseMenu.SetEnabled(true);
        PauseMenu.style.display = DisplayStyle.Flex;

        ResumeButton.style.display = DisplayStyle.Flex;
        ResumeButton.style.width = StyleKeyword.Auto;
        InfoElement.style.display = DisplayStyle.Flex;
        InventoryButton.style.display = DisplayStyle.Flex;
        QuitButton.style.display = DisplayStyle.Flex;
        RestartButton.style.display = DisplayStyle.Flex;


        Time.timeScale = 0f;
    }

    private void Resume()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = false;
        PlayerOptions.style.display = DisplayStyle.None;

        PauseMenu.SetEnabled(false);
        PauseMenu.style.display = DisplayStyle.None;

        Time.timeScale = 1f;
    }
    private void Restart()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = false;
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    private void DeathPause(bool dead)
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = true;
        PauseMenu.SetEnabled(true);
        PauseMenu.style.display = DisplayStyle.Flex;
        QuitButton.style.display = DisplayStyle.Flex;
        RestartButton.style.display = DisplayStyle.Flex;
        ResumeButton.style.display = DisplayStyle.None;
        InventoryButton.style.display = DisplayStyle.None;
        //InfoScrollElement.style.display = DisplayStyle.Flex;

        InfoElement.text = "You have fallen asleep in death \n";
        ShowGameStats();

        Time.timeScale = 0f;
    }

    private void LevelUpPause(List<string> ImprovedAttributesList)
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = true;

        PauseMenu.SetEnabled(true);
        PauseMenu.style.display = DisplayStyle.Flex;
        ResumeButton.text = "Continue";
        ResumeButton.style.display = DisplayStyle.Flex;
        InventoryButton.style.display = DisplayStyle.None;
        QuitButton.style.display = DisplayStyle.None;
        RestartButton.style.display = DisplayStyle.None;
        ResumeButton.style.width = Length.Percent(30);

        InfoElement.style.display = DisplayStyle.Flex;

        InfoElement.text = $"Level {PlayerStats.Instance.CurrentLevel}! \n";

        foreach (string rewardDescription in ImprovedAttributesList)
        {
            InfoElement.text += $"{rewardDescription} \n";
        }


        Time.timeScale = 0f;
    }
    private void ShowPlayerstats()
    {
        InfoElement.text = $"{PlayerStats.Instance.Name} stats:\n {PlayerStats.Instance.GetLivingBeingStats()}";
    }

    private void ShowGameStats()
    {
        string questStats = QuestHandler.Instance.GetQuestCompletionStats();
        Debug.Log($"{questStats}");
        InfoElement.text += $"{questStats}\n";
    }
    private void ChangeStatIndex()
    {
        ShowStats();
        statIndex++;
        Debug.Log($"stat index: {statIndex}");

    }
    private void ShowStats()
    {
        PlayerOptions.style.display = DisplayStyle.Flex;

        int minionsNum = AlchemyHandler.Instance.activeMinions.Count;
        if (statIndex == 0)
        {
            ShowPlayerstats();
        }
        else
        {
            if (minionsNum >= statIndex)
            {
                LivingBeing minion = AlchemyHandler.Instance.activeMinions[statIndex - 1];
                string minionStats = minion.GetLivingBeingStats();
                InfoElement.text = $"{minion.Name} stats:\n {minionStats}\n";
            }
            else
            {
                statIndex = 0;
                ShowPlayerstats();
            }
        }
    }

    private void ShowInventory()
    {
        PlayerOptions.style.display = DisplayStyle.Flex;

        if (invIndex == 0)
        {
            string InventoryString = AlchemyInventory.GetAlchemyInventory();
            InfoElement.text = $"Alchemy Inventory:\n {InventoryString}\n";
        }
        else if (invIndex == 1)
        {
            string InventoryString = AlchemyInventory.GetElementalKnowlegdeString();
            InfoElement.text = $"Your Elemental Knowledge:\n {InventoryString}\n";
        }
        else if (invIndex == 2)
        {
            string InventoryString = AlchemyInventory.GetKnownToolsString();
            InfoElement.text = $"Known Alchemy Tools:\n {InventoryString}\n";
        }
        invIndex++;
        if (invIndex > 2)
        {
            invIndex = 0;
        }
    }
}