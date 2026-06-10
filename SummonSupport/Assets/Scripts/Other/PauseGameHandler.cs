using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using SummonSupportEvents;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class PauseGameHandler : MonoBehaviour
{
    public static PauseGameHandler Instance { private set; get; }
    public static bool paused { private set; get; } = false;
    public static bool RecentlyPaused { private set; get; } = false;
    public static WaitForSeconds RecentlyPausedDuration { private set; get; } = new WaitForSeconds(.1f);


    private static UIDocument ui;

    private static VisualTreeAsset UIPrefabAssets { get; set; }

    private static VisualElement root;
    private static VisualElement PauseMenu;
    private static Button ResumeButton;
    private static Button SettingsButton;
    private static Button SaveButton;

    private static Button LoadButton;

    private static Button RestartButton;
    private static Button InventoryButton;
    private static Button StatsButton;

    private static Label InfoElement;
    private static VisualElement MainUI;
    private static VisualElement QuestInfoContainer;
    private static Label QuestInfoLabel;
    private static VisualElement PlayerOptions;
    private static int statIndex = 0;
    private static int invIndex = 0;


    private static Button QuitButton;
    void OnEnable()
    {
        //Debug.Log("Enabling pause game handler ");
        EventDeclarer.PauseGame?.AddListener(Pause);
        EventDeclarer.UnpauseGame?.AddListener(Resume);

        EventDeclarer.ShowPauseScreen?.AddListener(ShowPauseScreen);
        EventDeclarer.HidePauseScreen?.AddListener(HidePauseScreen);

        EventDeclarer.PlayerDead?.AddListener(DeathPause);
        EventDeclarer.PlayerLevelUp?.AddListener(LevelUpPause);

    }
    void OnDisable()
    {
        EventDeclarer.PauseGame?.RemoveListener(Pause);
        EventDeclarer.UnpauseGame?.RemoveListener(Resume);

        EventDeclarer.ShowPauseScreen?.RemoveListener(ShowPauseScreen);
        EventDeclarer.HidePauseScreen?.RemoveListener(HidePauseScreen);

        EventDeclarer.PlayerDead?.RemoveListener(DeathPause);
        EventDeclarer.PlayerLevelUp?.RemoveListener(LevelUpPause);


    }


    void Start()
    {
        //Debug.Log("Enabling pause game handler ");

        if (Instance == null) Instance = this;
        else Destroy(this);

        ui = UI_DocHandler.Instance.ui;
        UIPrefabAssets = UI_DocHandler.Instance.UIPrefabAssets;
        root = ui.rootVisualElement;

        #region get buttons from UI editor
        PauseMenu = root.Q<VisualElement>("PauseMenu");
        ResumeButton = PauseMenu.Q<Button>("Resume");
        InventoryButton = PauseMenu.Q<Button>("Inventory");
        RestartButton = PauseMenu.Q<Button>("Restart");
        QuitButton = PauseMenu.Q<Button>("Quit");
        StatsButton = PauseMenu.Q<Button>("ShowStats");
        SettingsButton = PauseMenu.Q<Button>("Settings");
        MainUI = root.Q<VisualElement>("MainUI");
        QuestInfoContainer = MainUI.Q<VisualElement>("QuestInfoContainer");
        PlayerOptions = MainUI.Q<VisualElement>("PlayerOptions");
        InfoElement = PlayerOptions.Q<Label>("Info");
        QuestInfoLabel = QuestInfoContainer.Q<Label>("QuestInfoLabel");
        #endregion

        #region  register buton callbacks
        ResumeButton.RegisterCallback<ClickEvent>(e => Resume());
        ResumeButton.RegisterCallback<ClickEvent>(e => EventDeclarer.UnpauseGame?.Invoke());
        ResumeButton.RegisterCallback<ClickEvent>(e => EventDeclarer.HidePauseScreen?.Invoke());


        RestartButton.RegisterCallback<ClickEvent>(e => Restart());
        SettingsButton.RegisterCallback<ClickEvent>(e => ShowSettingsOptions());


        StatsButton.RegisterCallback<ClickEvent>(e => ChangeStatIndex());

        InventoryButton.RegisterCallback<ClickEvent>(e => ShowInventory());

        StatsButton.RegisterCallback<ClickEvent>(e => ClearInfoElement());
        InventoryButton.RegisterCallback<ClickEvent>(e => ClearInfoElement());
        QuitButton.RegisterCallback<ClickEvent>(e => QuitToMainMenu());


        #endregion

        InfoElement.text = "";

    }

    #region Pause and Resume Methods
    static public void ShowPauseScreen()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = true;
        QuestInfoContainer.style.display = DisplayStyle.None;
        ClearInfoElement();

        PauseMenu.style.opacity = 0f;
        PauseMenu.style.display = DisplayStyle.Flex;
        PauseMenu.SetEnabled(true);


        ResumeButton.style.display = DisplayStyle.Flex;
        ResumeButton.style.width = StyleKeyword.Auto;
        InfoElement.style.display = DisplayStyle.Flex;
        InventoryButton.style.display = DisplayStyle.Flex;
        QuitButton.style.display = DisplayStyle.Flex;
        RestartButton.style.display = DisplayStyle.Flex;

        EaseInOpacity(PauseMenu, 350);
        EaseInOpacity(PlayerOptions, 350);
    }
    static public void Pause()
    {
        paused = true;
        RecentlyPaused = true;

        PauseTime();


    }
    public static void PauseTime()
    {
        Time.timeScale = 0f;
    }
    public static void ResumeTime()
    {
        Time.timeScale = 1f;
    }
    private static void EaseInOpacity(VisualElement element, int duration)
    {
        element.experimental.animation.Start(0f, 1f, duration, (e, v) => e.style.opacity = v)
            .Ease(Easing.OutCubic);

        PauseMenu.experimental.animation
        .Start(
            new Vector3(0.8f, 0.8f, 1f),
            Vector3.one,
            300,
            (e, v) => e.style.scale = new Scale(v)
        )
        .Ease(Easing.OutBack);
    }
    private static void EaseOutOpacity(VisualElement element, int duration)
    {
        element.experimental.animation.Start(1f, 0f, duration, (e, v) => e.style.opacity = v)
            .Ease(Easing.OutCubic)
            .OnCompleted(() => PauseMenu.style.display = DisplayStyle.None);

    }
    private static void ClearInfoElement()
    {
        InfoElement.Clear();
    }
    public static void HidePauseScreen()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = false;
        PlayerOptions.style.display = DisplayStyle.None;
        QuestInfoContainer.style.display = DisplayStyle.Flex;

        PauseMenu.SetEnabled(false);

        EaseOutOpacity(PauseMenu, 350);
        EaseOutOpacity(PlayerOptions, 350);
    }

    private static void Resume()
    {
        paused = false;

        Instance.StartCoroutine(Instance.NotRecentlyPaused());
        ResumeTime();
    }


    public IEnumerator NotRecentlyPaused()
    {
        yield return RecentlyPausedDuration;
        RecentlyPaused = false;
    }


    #region level up pause
    private static void LevelUpPause(List<string> ImprovedAttributesList)
    {
        //UnityEngine.Cursor.lockState = CursorLockMode.None;   // Locks the cursor to the center of the screen
        //UnityEngine.Cursor.visible = true;

        //PauseMenu.style.opacity = 0f;
        //PlayerOptions.style.opacity = 0f;
        //PauseMenu.SetEnabled(true);
        //PauseMenu.style.display = DisplayStyle.Flex;
        //
        //ResumeButton.text = "Continue";
        //ResumeButton.style.display = DisplayStyle.Flex;
        //InventoryButton.style.display = DisplayStyle.Flex;
        //QuitButton.style.display = DisplayStyle.Flex;
        //RestartButton.style.display = DisplayStyle.Flex;
        //PlayerOptions.style.display = DisplayStyle.Flex;
        //
        //InfoElement.style.display = DisplayStyle.Flex;
        //EaseInOpacity(PauseMenu, 350);
        //EaseInOpacity(PlayerOptions, 350);

        ShowPauseScreen();

        PauseTime();

        InfoElement.text = $"Level {PlayerStats.Instance.CurrentLevel}! \n";

        foreach (string rewardDescription in ImprovedAttributesList)
        {
            InfoElement.text += $"{rewardDescription} \n";
        }
    }
    #endregion

    #region death pause
    private static void DeathPause(bool dead)
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = true;

        PauseMenu.SetEnabled(true);
        PauseMenu.style.display = DisplayStyle.Flex;
        PauseMenu.style.opacity = 0f;

        QuitButton.style.display = DisplayStyle.Flex;
        RestartButton.style.display = DisplayStyle.Flex;
        ResumeButton.style.display = DisplayStyle.None;
        InventoryButton.style.display = DisplayStyle.None;
        //InfoScrollElement.style.display = DisplayStyle.Flex;

        InfoElement.text = "You have fallen asleep in death \n";
        EaseInOpacity(PauseMenu, 350);
        ShowGameStats();

        Time.timeScale = 0f;
    }

    #endregion
    #endregion

    #region restart
    private static void Restart()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = false;
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region quit to main menu
    private static void QuitToMainMenu()
    {
        SceneManager.LoadSceneAsync("StartScreen");
        Time.timeScale = 1f;

    }
    #endregion

    #region Show stats and inventory

    private static void ShowPlayerstats()
    {
        InfoElement.text = $"{PlayerStats.Instance.Name} stats:\n {PlayerStats.Instance.GetLivingBeingStats()}";
    }

    private static void ShowGameStats()
    {
        string questStats = QuestHandler.GetQuestCompletionStats();
        Debug.Log($"{questStats}");
        InfoElement.text += $"{questStats}\n";
    }
    private static void ChangeStatIndex()
    {
        ShowStats();
        statIndex++;
        Debug.Log($"stat index: {statIndex}");

    }
    private static void ShowStats()
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

    private static void ShowInventory()
    {
        PlayerOptions.style.display = DisplayStyle.Flex;
        string InventoryString = "";
        if (invIndex == 0)
        {
            InventoryString = AlchemyInventory.GetCraftingPotentialString();
            InfoElement.text = $"Alchemy Inventory:\n {InventoryString}\n";
        }
        else if (invIndex == 1)
        {
            InventoryString = AlchemyInventory.GetElementalKnowlegdeString();
            InfoElement.text = $"Your Elemental Knowledge:\n {InventoryString}\n";
        }
        else if (invIndex == 2)
        {
            InventoryString = AlchemyInventory.GetKnownToolsString();
            InfoElement.text = $"Known Alchemy Tools:\n {InventoryString}\n";
        }
        else if (invIndex == 3)
        {
            InventoryString = QuestHandler.GetQuestCompletionStats();
            InfoElement.text = $"Quest progress:\n {InventoryString}\n";
        }
        invIndex++;
        if (invIndex > 3)
        {
            invIndex = 0;
        }
    }
    #endregion
    #region Settings Menu
    private static void ShowSettingsOptions()
    {
        PlayerOptions.style.display = DisplayStyle.Flex;
        ClearInfoElement();
        InfoElement.text = "";
        Button audioSettingsButton = AddButtons("Audio Settings", InfoElement, 50, 25);
        Button saveLoadButton = AddButtons("Save/Load", InfoElement, 50, 25);

        audioSettingsButton.RegisterCallback<ClickEvent>(e => ShowAudioSettings());
        saveLoadButton.RegisterCallback<ClickEvent>(e => ShowSaveLoad());
    }
    private static void ShowAudioSettings()
    {
        PlayerOptions.style.display = DisplayStyle.Flex;
        ClearInfoElement();
        InfoElement.text = "";
        EaseInOpacity(PlayerOptions, 350);
        Button volumeUpButton = AddButtons("Volume Up", InfoElement, 50, 25);
        Button volumeDownButton = AddButtons("Volume Down", InfoElement, 50, 25);

        volumeDownButton.RegisterCallback<ClickEvent>(e => AudioHandler.Instance.AdjustGeneralGameVolume(false));
        volumeUpButton.RegisterCallback<ClickEvent>(e => AudioHandler.Instance.AdjustGeneralGameVolume(true));

        volumeDownButton.RegisterCallback<ClickEvent>(e => DisplayVolume());
        volumeUpButton.RegisterCallback<ClickEvent>(e => DisplayVolume());

        DisplayVolume();


    }
    private static void ShowSaveLoad()
    {
        ClearInfoElement();

        Button saveButton = AddButtons("Save Game", InfoElement, 4, 20);
        saveButton.RegisterCallback<ClickEvent>(e => ShowSaveableSlots());
        Button loadButton = AddButtons("Load Game", InfoElement, 4, 20);
        loadButton.RegisterCallback<ClickEvent>(e => ShowLoadbleSlots());
    }
    private static void ShowSaveableSlots()
    {
        ClearInfoElement();

        foreach (var saveDataKvp in SaveHandler.saves)
        {
            Button saveButton = AddButtons(SaveHandler.GetSaveFileInfo(saveDataKvp.Key), InfoElement, 4, 20);
            saveButton.RegisterCallback<ClickEvent>(e => SaveHandler.SaveGameData(saveDataKvp.Key));
            saveButton.RegisterCallback<ClickEvent>(e => saveButton.text = SaveHandler.GetSaveFileInfo(saveDataKvp.Key));

        }
    }
    private static void ShowLoadbleSlots()
    {
        ClearInfoElement();

        foreach (var saveDataKvp in SaveHandler.saves)
        {
            Button loadButton = AddButtons(SaveHandler.GetSaveFileInfo(saveDataKvp.Key), InfoElement, 4, 20);
            loadButton.RegisterCallback<ClickEvent>(e => SaveHandler.LoadGameData(saveDataKvp.Key));
        }
    }
    private static void DisplayVolume()
    {
        InfoElement.text = $"\n \n \n \n {AudioHandler.Instance.GetGeneralGameVolume()}\n";
    }

    private static Button AddButtons(string buttonText, VisualElement panel, float width, float height)
    {
        TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
        Button button = prefabContainer.Q<Button>("ButtonPrefab");
        button.text = buttonText;

        panel.Add(button);
        return button;

    }
    #endregion

}

//Easing.OutCubic     // default-feeling UI motion
//Easing.OutQuad      // snappy
//Easing.InOutCubic   // smooth panels
//Easing.OutBack      // overshoot (Hades-style)
//Easing.OutElastic   // attention grabber
