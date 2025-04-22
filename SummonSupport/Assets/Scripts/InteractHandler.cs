using Unity.VisualScripting;
using UnityEngine;

public class InteractHandler : MonoBehaviour
{
    private I_Interactable mostRecentInteractable;


    void OnTriggerEnter2D(Collider2D collision)
    {
        InvokeRepeating("AttemptInteraction", 0f, .1f);
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        if (interactInterfaceInstance != null)
        {
            mostRecentInteractable = interactInterfaceInstance;
            interactInterfaceInstance.ShowInteractionOption();
        }

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        CancelInvoke("AttemptInteraction");
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        mostRecentInteractable = null;
        if (interactInterfaceInstance != null)
        {
            interactInterfaceInstance.HideInteractionOption();
        }
    }

    void AttemptInteraction()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && mostRecentInteractable != null) mostRecentInteractable.Interact();
    }

}
