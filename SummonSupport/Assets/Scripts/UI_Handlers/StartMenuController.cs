using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    public static StartMenuController Instance { private set; get; }

    [SerializeField] private string gameplaySceneName = "Map1";
    // The setup panel is built from copies of these buttons; found by name if not set.
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
        if (startButton == null) startButton = FindButton("StartButton");
        if (quitButton == null) quitButton = FindButton("QuitButton");
    }

    public void StartGame()
    {
        OpenGameSetup("Single Player", (slot, difficulty, save) =>
        {
            GameSession.Begin(slot, difficulty, save);
            SceneManager.LoadSceneAsync(gameplaySceneName);
        }, null);
    }

    /// <summary>
    /// Shows the save slot and difficulty choice in place of the start screen buttons. onBegin gets the chosen
    /// slot, difficulty and save (null for a new game); onBack runs if the player backs out.
    /// </summary>
    public void OpenGameSetup(string heading, Action<int, Difficulty, SessionSaveData> onBegin, Action onBack)
    {
        if (startButton == null) return;
        SetMenuButtonsVisible(false);
        GameSetupMenu.Open(startButton, heading,
            (slot, difficulty, save) => { SetMenuButtonsVisible(true); onBegin?.Invoke(slot, difficulty, save); },
            () => { SetMenuButtonsVisible(true); onBack?.Invoke(); });
    }

    private void SetMenuButtonsVisible(bool visible)
    {
        if (startButton != null) startButton.gameObject.SetActive(visible);
        if (quitButton != null) quitButton.gameObject.SetActive(visible);
    }

    private static Button FindButton(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<Button>() : null;
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
