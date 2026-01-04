using UnityEngine;

public class MinionInteractionHandler : MonoBehaviour
{
    [SerializeField] public bool CommandedToInteract { get; private set; } = false;

    public void SetCommandToInteract(bool isCommanded)
    {
        CommandedToInteract = isCommanded;
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (CommandedToInteract)
        {
            I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
            if (interactInterfaceInstance != null)
            {
                InteractIfPossible(interactInterfaceInstance);
            }
        }
    }
    private void InteractIfPossible(I_Interactable interactInterfaceInstance)
    {
        ShowInteractionAnimation();
        interactInterfaceInstance.Interact(this.gameObject);
    }

    private void ShowInteractionAnimation()
    {
        Logging.Info("Showing Interaction animation");
    }
}
