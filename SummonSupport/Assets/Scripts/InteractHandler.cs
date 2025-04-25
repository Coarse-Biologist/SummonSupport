using Unity.VisualScripting;
using UnityEngine;

public class InteractHandler : MonoBehaviour
{
    private I_Interactable mostRecentInteractable;
    private bool checkingForTab = false;


    void OnTriggerEnter2D(Collider2D collision)
    {
        //InvokeRepeating("AttemptInteraction", 0f, .05f);
        checkingForTab = true;
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        if (interactInterfaceInstance != null)
        {
            mostRecentInteractable = interactInterfaceInstance;
            interactInterfaceInstance.ShowInteractionOption();
        }

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        checkingForTab = false;
        //CancelInvoke("AttemptInteraction");
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        mostRecentInteractable = null;
        if (interactInterfaceInstance != null)
        {
            interactInterfaceInstance.HideInteractionOption();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        Logging.Info("you are in the collider");

    }

    void FixedUpdate()
    {
        if (checkingForTab)
        {
            if (Input.GetKeyDown(KeyCode.Tab) && mostRecentInteractable != null)
            {
                Logging.Info("tab pressed");
                mostRecentInteractable.Interact(this.gameObject);
            }
        }
    }

}
