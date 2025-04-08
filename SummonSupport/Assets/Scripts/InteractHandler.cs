using UnityEngine;

public class InteractHandler : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        if (interactInterfaceInstance != null)
        {
            Logging.Info("Trying to call show interction function");
            interactInterfaceInstance.ShowInteractionOption();
        }
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        if (interactInterfaceInstance != null && Input.GetKey(KeyCode.Tab))
        {
            interactInterfaceInstance.Interact();
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        if (interactInterfaceInstance != null)
        {
            interactInterfaceInstance.HideInteractionOption();
        }
    }

}
