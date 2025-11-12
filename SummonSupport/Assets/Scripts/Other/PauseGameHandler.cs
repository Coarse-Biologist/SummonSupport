using System.Collections.Generic;
using SummonSupportEvents;
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
    private Button ResumeButton;
    private Button RestartButton;
    private Button OptionsButton;
    private Label InfoElement;

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
        ResumeButton = PauseMenu.Q<Button>("Resume");
        OptionsButton = PauseMenu.Q<Button>("Options");
        RestartButton = PauseMenu.Q<Button>("Restart");
        QuitButton = PauseMenu.Q<Button>("Quit");
        InfoElement = PauseMenu.Q<Label>("Info");

        ResumeButton.RegisterCallback<ClickEvent>(e => Resume());
        RestartButton.RegisterCallback<ClickEvent>(e => Restart());
        //QuitButton.RegisterCallback<ClickEvent>(e => Quit());

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

        InfoElement.style.display = DisplayStyle.None;
        OptionsButton.style.display = DisplayStyle.Flex;
        QuitButton.style.display = DisplayStyle.Flex;
        RestartButton.style.display = DisplayStyle.Flex;


        Time.timeScale = 0f;
    }

    private void Resume()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        UnityEngine.Cursor.visible = false;

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
        OptionsButton.style.display = DisplayStyle.None;
        InfoElement.text = "You have fallen asleep in death";


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
        OptionsButton.style.display = DisplayStyle.None;
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
}
