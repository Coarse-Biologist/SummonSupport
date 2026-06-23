using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public static StartMenuController Instance { private set; get; }
    void Start()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    public void StartGame()
    {

        SceneManager.LoadSceneAsync("Map1");
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
