using UnityEngine;

public class InteractHandler : MonoBehaviour
{
    private I_Interactable mostRecentInteractable;
    private bool checkingForTab = false;


    void OnTriggerEnter(Collider collision)
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

    void OnTriggerExit(Collider collision)
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

    void Update()
    {
        if (checkingForTab)
        {
            if (Input.GetKeyDown(KeyCode.Tab) && !mostRecentInteractable.Equals(null))
            {
                mostRecentInteractable.Interact(this.gameObject); // Todo null reference after minon is ressed
            }
        }
    }

}
