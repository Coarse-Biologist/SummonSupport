using UnityEngine;

public class DoorHandler : MonoBehaviour, I_Interactable
{
    [SerializeField] public bool Open { get; private set; } = false;

    public void Interact()
    {
        Logging.Info("Door is now open");
        Open = true;
    }

    public void ShowInteractionOption()
    {
    }

    public void HideInteractionOption()
    {
    }
}
