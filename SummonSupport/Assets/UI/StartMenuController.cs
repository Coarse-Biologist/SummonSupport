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
        Debug.Log("Start Game");
        // logic for loading first game scene
        SceneManager.LoadSceneAsync("FirstLevel3D");
    }


}
