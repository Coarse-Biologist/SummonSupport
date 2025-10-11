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

    private Button QuitButton;
    void OnEnable()
    {
        EventDeclarer.TogglePauseGame?.AddListener(ToggleGamePause);
        EventDeclarer.PlayerDead?.AddListener(DeathPause);

    }
    void OnDisable()
    {
        EventDeclarer.TogglePauseGame?.RemoveListener(ToggleGamePause);
        EventDeclarer.PlayerDead?.RemoveListener(DeathPause);

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
        PauseMenu.SetEnabled(true);
        PauseMenu.style.display = DisplayStyle.Flex;

        Time.timeScale = 0f;
    }

    private void Resume()
    {
        PauseMenu.SetEnabled(false);
        PauseMenu.style.display = DisplayStyle.None;

        Time.timeScale = 1f;
    }
    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    private void DeathPause(bool dead)
    {
        PauseMenu.SetEnabled(true);
        PauseMenu.style.display = DisplayStyle.Flex;
        ResumeButton.style.display = DisplayStyle.None;
        OptionsButton.style.display = DisplayStyle.None;


        Time.timeScale = 0f;
    }
}
