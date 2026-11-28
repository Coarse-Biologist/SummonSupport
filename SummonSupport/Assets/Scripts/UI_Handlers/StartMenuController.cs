using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    public static StartMenuController Instance { private set; get; }

    [SerializeField] private string gameplaySceneName = "Map1";
    // The setup panel is built from copies of these buttons; found by name if not set.
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    // The character name field is a copy of this one; found by name if not set.
    [SerializeField] private TMP_InputField nameInputTemplate;
    [SerializeField] private CharacterCreationProfile_SO characterCreationProfile;

    void Start()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
        if (startButton == null) startButton = FindButton("StartButton");
        if (quitButton == null) quitButton = FindButton("QuitButton");
        if (nameInputTemplate == null) nameInputTemplate = FindComponent<TMP_InputField>("JoinCodeInput");
        // Gameplay locks the cursor, and a client sent back here when the host leaves arrives with it still locked.
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        // A controller starts on Start (see MenuNavigation).
        if (startButton != null && EventSystem.current != null) EventSystem.current.firstSelectedGameObject = startButton.gameObject;
        LinkMenuButtons();
    }

    // The main buttons and the host/join column are on two canvases with different render modes, which Unity's
    // automatic navigation can't move between; link them by where they are on screen instead.
    private void LinkMenuButtons()
    {
        Canvas.ForceUpdateCanvases(); // let the layout groups place the rows first
        List<Selectable> items = new() { startButton, quitButton };
        MultiplayerMenuController multiplayerMenu = FindFirstObjectByType<MultiplayerMenuController>();
        if (multiplayerMenu != null) items.AddRange(multiplayerMenu.MenuItems);
        MenuNavigation.LinkByScreenPosition(items);
    }

    public void StartGame()
    {
        OpenGameSetup("Single Player (offline)", (slot, difficulty, save) =>
        {
            GameSession.Begin(slot, difficulty, save);
            SceneManager.LoadSceneAsync(gameplaySceneName);
        }, null);
    }

    /// <summary>
    /// Shows the save slot and difficulty choice in place of the start screen buttons. onBegin gets the chosen
    /// slot, difficulty and save (null for a new game); onBack runs if the player backs out.
    /// A new game asks for the new character first; a continued game brings its saved character.
    /// </summary>
    public void OpenGameSetup(string heading, Action<int, Difficulty, SessionSaveData> onBegin, Action onBack)
    {
        if (startButton == null) return;
        SetMenuButtonsVisible(false);
        GameSetupMenu.Open(startButton, heading,
            (slot, difficulty, save) =>
            {
                if (save != null)
                {
                    SetMenuButtonsVisible(true);
                    onBegin?.Invoke(slot, difficulty, save);
                    return;
                }
                // Back returns to the slot choice.
                CharacterCreationMenu.Open(startButton, nameInputTemplate, characterCreationProfile, heading,
                    _ => { SetMenuButtonsVisible(true); onBegin?.Invoke(slot, difficulty, null); },
                    () => OpenGameSetup(heading, onBegin, onBack));
            },
            () => { SetMenuButtonsVisible(true); onBack?.Invoke(); });
    }

    /// <summary>
    /// Shows the new character choice (name and element) in place of the start screen buttons, e.g. before joining.
    /// onConfirm runs once the player confirms it; onBack runs if they back out.
    /// </summary>
    public void OpenCharacterCreation(string heading, Action<CharacterCreationData> onConfirm, Action onBack)
    {
        if (startButton == null)
        {
            onConfirm?.Invoke(CharacterSelection.Current);
            return;
        }
        SetMenuButtonsVisible(false);
        CharacterCreationMenu.Open(startButton, nameInputTemplate, characterCreationProfile, heading,
            data => { SetMenuButtonsVisible(true); onConfirm?.Invoke(data); },
            () => { SetMenuButtonsVisible(true); onBack?.Invoke(); });
    }

    // The slot panel takes the place of every other start screen button, including the host/join column.
    private void SetMenuButtonsVisible(bool visible)
    {
        if (startButton != null) startButton.gameObject.SetActive(visible);
        if (quitButton != null) quitButton.gameObject.SetActive(visible);
        MultiplayerMenuController multiplayerMenu = FindFirstObjectByType<MultiplayerMenuController>();
        if (multiplayerMenu != null) multiplayerMenu.SetPanelVisible(visible);
        if (visible) LinkMenuButtons();
    }

    private static Button FindButton(string objectName) => FindComponent<Button>(objectName);

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<T>() : null;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }



}
