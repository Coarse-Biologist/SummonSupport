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
    private static Button QuestsButton;

    private static Button SaveButton;

    private static Button LoadButton;

    private static Button RestartButton;
    private static Button InventoryButton;
    private static Button StatsButton;
    private static Button HostOptionsButton;

    private static Label InfoElement;
    private static VisualElement MainUI;
    private static VisualElement QuestInfoContainer;
    private static Label QuestInfoLabel;
    private static VisualElement PlayerOptions;
    private static int statIndex = 0;
    private static int invIndex = 0;
    private static int activeQuestIndex = 0;

    private static List<Button> SpawnedButtons = new();



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
        QuestsButton = PauseMenu.Q<Button>("Quests");

        InventoryButton = PauseMenu.Q<Button>("Inventory");
        RestartButton = PauseMenu.Q<Button>("Restart");
        QuitButton = PauseMenu.Q<Button>("Quit");
        StatsButton = PauseMenu.Q<Button>("ShowStats");
        SettingsButton = PauseMenu.Q<Button>("Settings");
        HostOptionsButton = PauseMenu.Q<Button>("HostOptions");
        MainUI = root.Q<VisualElement>("MainUI");
        QuestInfoContainer = MainUI.Q<VisualElement>("QuestInfoContainer");
        PlayerOptions = MainUI.Q<VisualElement>("PlayerOptions");
        InfoElement = PlayerOptions.Q<Label>("Info");
        QuestInfoLabel = QuestInfoContainer.Q<Label>("QuestInfoLabel");
        #endregion

        #region  register buton callbacks
        QuestsButton.RegisterCallback<ClickEvent>(e => ShowNextQuestInfo());
        ResumeButton.RegisterCallback<ClickEvent>(e => RequestPauseMenu(false));


        RestartButton.RegisterCallback<ClickEvent>(e => Restart());
        SettingsButton.RegisterCallback<ClickEvent>(e => ShowSettingsOptions());
        HostOptionsButton?.RegisterCallback<ClickEvent>(e => ShowHostOptions());


        StatsButton.RegisterCallback<ClickEvent>(e => ShowStats());
        StatsButton.RegisterCallback<ClickEvent>(e => ChangeStatIndex());


        InventoryButton.RegisterCallback<ClickEvent>(e => ShowInventory());

        // StatsButton.RegisterCallback<ClickEvent>(e => ClearInfoElement());
        InventoryButton.RegisterCallback<ClickEvent>(e => ClearInfoElement());
        QuitButton.RegisterCallback<ClickEvent>(e => QuitToMainMenu());


        #endregion

        InfoElement.text = "";

    }

    #region Pause and Resume Methods
    /// <summary>
    /// Opens or closes the pause menu. Online the pause menu is shared, so the request goes
    /// through the host and the menu opens or closes for every player.
    /// </summary>
    public static void RequestPauseMenu(bool open)
    {
        NetworkPlayer localPlayer = NetworkSession.IsOnline && PlayerStats.Instance != null ? PlayerStats.Instance.GetComponent<NetworkPlayer>() : null;
        if (localPlayer != null && localPlayer.IsSpawned)
        {
            localPlayer.RequestSharedPause(open);
            return;
        }
        ApplyPauseMenu(open, null);
    }

    /// <summary>Opens or closes the pause menu on this machine. pausedBy names who paused, for online sessions.</summary>
    public static void ApplyPauseMenu(bool open, string pausedBy)
    {
        // A player at an alchemy bench already has their own pause and cursor; leave them in it.
        if (PlayerStats.Instance != null && PlayerStats.Instance.TryGetComponent(out PlayerMovement movement) && movement.usingUI) return;
        if (open)
        {
            EventDeclarer.PauseGame?.Invoke();
            EventDeclarer.ShowPauseScreen?.Invoke();
            if (NetworkSession.IsOnline && !string.IsNullOrEmpty(pausedBy)) InfoElement.text = $"Game paused by {pausedBy}";
        }
        else
        {
            EventDeclarer.UnpauseGame?.Invoke();
            EventDeclarer.HidePauseScreen?.Invoke();
        }
    }

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
        ShowHostOnlyButtons();

        EaseInOpacity(PauseMenu, 350);
        EaseInOpacity(PlayerOptions, 350);
    }
    // Online, only the host restarts the game or changes host options.
    private static void ShowHostOnlyButtons()
    {
        RestartButton.style.display = NetworkSession.IsClientOnly ? DisplayStyle.None : DisplayStyle.Flex;
        if (HostOptionsButton != null) HostOptionsButton.style.display = NetworkSession.IsHost ? DisplayStyle.Flex : DisplayStyle.None;
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
        ClearSpawnedButtons(PlayerOptions);
        InfoElement.Clear();
    }
    private static void SetInfoElementText(string words)
    {
        InfoElement.text = words;
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
    #region 

    private static void ShowNextQuestInfo()
    {
        ClearInfoElement();

        activeQuestIndex++;
        if (activeQuestIndex >= QuestHandler.ActiveQuests.Count) activeQuestIndex = 0;
        SetInfoElementText(QuestHandler.GetQuestInfo(QuestHandler.ActiveQuests[activeQuestIndex]));
    }

    #endregion


    #region level up pause
    private static void LevelUpPause(List<string> ImprovedAttributesList)
    {

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
        ShowHostOnlyButtons();
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
        if (NetworkSession.IsClientOnly) return;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = false;
        Time.timeScale = 1f;
        // Restarting goes back to the slot's last save (or a fresh start if the game was never saved).
        GameSession.ReloadFromSlot();
        if (NetworkSession.IsHost)
        {
            // Bring everyone back, unpause them, then reload the level for the whole session.
            foreach (Unity.Netcode.NetworkClient client in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null || !client.PlayerObject.TryGetComponent(out PlayerStats stats)) continue;
                stats.Resurrect();
                stats.RestoreResources();
            }
            RequestPauseMenu(false);
            Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            return;
        }
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region quit to main menu
    private static void QuitToMainMenu()
    {
        // A client sends its character to the host first, so the host saves it as the client leaves.
        if (NetworkSession.IsClientOnly && NetworkPlayer.Local != null) NetworkPlayer.Local.PushCharacter();
        // Leaving ends this machine's part in an online session; when the host leaves, everyone returns to the start screen.
        if (NetworkSession.IsOnline && RelayConnectionManager.Instance != null) RelayConnectionManager.Instance.Disconnect();
        GameSession.End();
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
        statIndex++;

        Debug.Log($"stat index: {statIndex}");

    }
    private static void ShowStats()
    {
        InfoElement.style.display = DisplayStyle.Flex;
        InfoElement.text = "";
        ClearSpawnedButtons(PlayerOptions);


        int minionsNum = AlchemyHandler.Instance.activeMinions.Count;
        if (statIndex == 0)
        {
            ShowPlayerstats();
        }
        else if (statIndex == 1)
        {
            ShowKnownAbilities();
        }
        else
        {
            if (minionsNum >= statIndex - 1)
            {
                LivingBeing minion = AlchemyHandler.Instance.activeMinions[statIndex - 2];
                string minionStats = minion.GetLivingBeingStats();
                InfoElement.text = $"{minion.Name} stats:\n {minionStats}\n";
                return;
            }
            else
            {
                statIndex = 0;
                ShowPlayerstats();
                return;

            }
        }
    }
    private static void ShowKnownAbilities()
    {
        InfoElement.style.display = DisplayStyle.None;

        foreach (Ability ability in PlayerStats.Instance.abilityHandler.Abilities)
        {
            TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
            Button button = prefabContainer.Q<Button>("ButtonPrefab");

            PlayerOptions.Add(button);
            SpawnedButtons.Add(button);


            button.text = $"{ability.Name}";

            button.RegisterCallback<ClickEvent>(e => ShowAbilityInfo(ability.DisplayAbilityInfo()));

        }
    }
    private static void ShowAbilityInfo(string info)
    {
        InfoElement.style.display = DisplayStyle.Flex;

        InfoElement.text = info;
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
        audioSettingsButton.RegisterCallback<ClickEvent>(e => ShowAudioSettings());
        // Online, saving belongs to the host. Saves are loaded from the start screen, when a game starts.
        if (!NetworkSession.IsClientOnly)
        {
            Button saveButton = AddButtons("Save Game", InfoElement, 50, 25);
            saveButton.RegisterCallback<ClickEvent>(e => ShowSaveScreen());
        }
        Button joinCodeButton = AddButtons("Join Code", InfoElement, 50, 25);
        joinCodeButton.RegisterCallback<ClickEvent>(e => ShowJoinCode());
    }

    private static void ShowJoinCode()
    {
        ClearInfoElement();
        InfoElement.text = "";

        RelayConnectionManager relay = RelayConnectionManager.Instance;
        string joinCode = relay != null ? relay.ActiveJoinCode : null;
        if (string.IsNullOrEmpty(joinCode))
        {
            AddInfoLabel("Not in an online session.\nHost a game from the start screen to get a join code.", 30);
            return;
        }

        bool isHost = NetworkSession.IsHost;
        AddInfoLabel(isHost ? "Your join code" : "Session join code", 30);
        Label codeLabel = AddInfoLabel(joinCode, 96);
        codeLabel.style.letterSpacing = 12;
        codeLabel.selection.isSelectable = true;
        AddInfoLabel(isHost ? "Share it so friends can join your game." : "You joined with this code. Share it so others can join too.", 24);

        Button copyButton = AddButtons("Copy Code", InfoElement, 50, 25);
        copyButton.RegisterCallback<ClickEvent>(e =>
        {
            GUIUtility.systemCopyBuffer = joinCode;
            copyButton.text = "Copied!";
        });
    }
    private static Label AddInfoLabel(string text, float fontSize)
    {
        Label label = new Label(text);
        label.style.fontSize = fontSize;
        label.style.color = Color.white;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.marginBottom = 10;
        InfoElement.Add(label);
        return label;
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
    private static void ShowSaveScreen(string status = null)
    {
        if (NetworkSession.IsClientOnly) return;
        PlayerOptions.style.display = DisplayStyle.Flex;
        ClearInfoElement();
        InfoElement.text = "";

        int activeSlot = GameSession.ActiveSlot;
        AddInfoLabel(status ?? (activeSlot == GameSession.NoSlot ? "Choose a slot to save this game to." : $"This game saves to slot {activeSlot}."), 26);
        if (NetworkSession.IsHost) AddInfoLabel("Saves every player's character.", 20);

        for (int slot = 1; slot <= SaveHandler.SlotCount; slot++)
        {
            int slotNumber = slot;
            string description = SaveHandler.DescribeSlot(slotNumber) + (slotNumber == activeSlot ? " (current)" : "");
            bool needsConfirm = slotNumber != activeSlot && SaveHandler.ReadSlot(slotNumber) != null;
            Button slotButton = AddButtons(description, InfoElement, 4, 20);
            bool confirming = false;
            bool saving = false;
            slotButton.RegisterCallback<ClickEvent>(e =>
            {
                if (saving) return;
                if (needsConfirm && !confirming)
                {
                    confirming = true;
                    slotButton.text = $"Click again to overwrite slot {slotNumber}";
                    return;
                }
                saving = true;
                slotButton.text = "Saving...";
                NetworkPlayer.SaveSession(slotNumber, saved =>
                    ShowSaveScreen(saved ? $"Saved to slot {slotNumber}." : "Could not save; see the console."));
            });
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
    private static void ClearSpawnedButtons(VisualElement panel)
    {
        foreach (Button b in SpawnedButtons)
        {
            panel.Remove(b);

        }
        SpawnedButtons.Clear();
    }
    #endregion

    #region Host Options
    private static void ShowHostOptions()
    {
        if (!NetworkSession.IsHost) return;
        PlayerOptions.style.display = DisplayStyle.Flex;
        ClearInfoElement();
        InfoElement.text = "";

        AddInfoLabel($"Difficulty: {GameSession.Difficulty?.ToString() ?? "set by the level"}", 24);

        AddInfoLabel("Players", 30);
        Unity.Netcode.NetworkManager networkManager = Unity.Netcode.NetworkManager.Singleton;
        bool anyGuests = false;
        foreach (Unity.Netcode.NetworkClient client in networkManager.ConnectedClientsList)
        {
            if (client.ClientId == Unity.Netcode.NetworkManager.ServerClientId) continue;
            anyGuests = true;
            ulong clientId = client.ClientId;
            string playerName = GetPlayerName(client);
            Button removeButton = AddButtons($"Remove {playerName}", InfoElement, 50, 25);
            bool confirming = false;
            removeButton.RegisterCallback<ClickEvent>(e =>
            {
                if (!confirming)
                {
                    confirming = true;
                    removeButton.text = $"Click again to remove {playerName}";
                    return;
                }
                // Their newest character is saved as they go (see NetworkPlayer.RemovePlayerAfterSaving).
                removeButton.text = $"Saving and removing {playerName}...";
                removeButton.SetEnabled(false);
                NetworkPlayer.RemovePlayerAfterSaving(clientId);
            });
        }
        if (!anyGuests) AddInfoLabel("No one else has joined yet.", 20);
    }

    private static string GetPlayerName(Unity.Netcode.NetworkClient client)
    {
        PlayerStats stats = client.PlayerObject != null ? client.PlayerObject.GetComponent<PlayerStats>() : null;
        string name = stats != null && !string.IsNullOrEmpty(stats.Name) ? stats.Name : "Player";
        return $"{name} ({client.ClientId})";
    }
    #endregion

}

//Easing.OutCubic     // default-feeling UI motion
//Easing.OutQuad      // snappy
//Easing.InOutCubic   // smooth panels
//Easing.OutBack      // overshoot (Hades-style)
//Easing.OutElastic   // attention grabber
