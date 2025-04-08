using UnityEngine;

public class NPC_Handler : MonoBehaviour, I_Interactable
{
    [SerializeField] NPC_SO npcData;
    [SerializeField] GameObject interactCanvas;
    private GameObject canvasInstance;

    void Awake()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = npcData.NPC_Sprite;
    }

    public void ShowInteractionOption()
    {
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, transform.position, Quaternion.identity);
        else canvasInstance.SetActive(true);
    }

    public void HideInteractionOption()
    {
        canvasInstance.SetActive(false);
    }
    public void Interact()
    {

    }
}
