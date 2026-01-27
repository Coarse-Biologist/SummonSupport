using UnityEngine;

public class InteractHandler : MonoBehaviour
{
    private I_Interactable mostRecentInteractable;
    private bool checkingForTab = false;


    void OnTriggerEnter(Collider collision)
    {

        if (collision.gameObject.TryGetComponent(out I_Interactable interactable))
        {
            checkingForTab = true;
            mostRecentInteractable = interactable;
            InvokeRepeating("AttemptInteraction", 0f, .05f);
            interactable.ShowInteractionOption();
        }

    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out I_Interactable interactable))
        {
            checkingForTab = false;
            CancelInvoke("AttemptInteraction");
            mostRecentInteractable = null;
            interactable.HideInteractionOption();
        }
    }

    private void AttemptInteraction()
    {
        if (checkingForTab)
        {
            if (Input.GetKeyDown(KeyCode.Z) && !mostRecentInteractable.Equals(null))
            {
                mostRecentInteractable.Interact(this.gameObject); // Todo null reference after minon is ressed
            }
        }
    }

}
