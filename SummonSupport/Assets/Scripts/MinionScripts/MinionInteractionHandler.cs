using UnityEngine;

public class MinionInteractionHandler : MonoBehaviour
{
    [SerializeField] public bool CommandedToInteract { get; private set; } = false;

    public void SetCommandToInteract(bool isCommanded)
    {
        CommandedToInteract = isCommanded;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Logging.Info($"Entered trigger of {collision.gameObject.name}");

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
        interactInterfaceInstance.Interact();
    }

    private void ShowInteractionAnimation()
    {
        Logging.Info("Showing Interaction animation");
    }
}
