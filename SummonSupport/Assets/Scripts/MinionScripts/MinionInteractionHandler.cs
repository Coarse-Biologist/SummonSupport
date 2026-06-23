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
            if (collision.gameObject.TryGetComponent(out I_Interactable interactInterfaceInstance))
            {
                if (!collision.gameObject.TryGetComponent(out AlchemyHandler undesiredComponent)) // make sure this isnt the alchemy bench

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
