using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class InteractCanvasHandler : MonoBehaviour
{
    public static InteractCanvasHandler Instance { get; private set; }
    private GameObject canvasInstance;
    [SerializeField] GameObject interactCanvas;

    public void Awake()
    {
        Instance = this;
    }

    public void ShowInteractionOption(Vector2 spawnLoc, string interactMessage)
    {
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, spawnLoc, Quaternion.identity);
        else canvasInstance.SetActive(true);
        canvasInstance.GetComponentInChildren<TextMeshProUGUI>().text = interactMessage;
    }

    public void HideInteractionOption()
    {
        Destroy(canvasInstance);
    }

    public void SetTemporaryCanvasText(Transform transform, string temporaryText)
    {
        Canvas canvas = transform.GetComponent<Canvas>();
        canvas.GetComponentInChildren<TextMeshProUGUI>().text = temporaryText;


    }

}
