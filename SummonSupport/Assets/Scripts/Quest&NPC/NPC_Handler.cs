using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class NPC_Handler : MonoBehaviour, I_Interactable
{
    [SerializeField] NPC_SO npcData;
    [SerializeField] GameObject interactCanvas;
    private GameObject canvasInstance;
    private TextMeshProUGUI dialogue;

    void Awake()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = npcData.NPC_Sprite;
    }

    public void ShowInteractionOption()
    {
        if (canvasInstance == null)
        {
            canvasInstance = Instantiate(interactCanvas, transform.position, Quaternion.identity);
            dialogue = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            canvasInstance.SetActive(true);
            dialogue.text = "Tab to interact";
        }
    }

    public void HideInteractionOption()
    {

        dialogue.text = npcData.Goodbye;
        Invoke("HideandReset", 2f);
    }
    public void Interact()
    {
        if (dialogue != null) dialogue.text = npcData.Greeting;
    }

    private void HideandReset()
    {
        canvasInstance.SetActive(false);
        dialogue.text = npcData.Greeting;
    }
}
