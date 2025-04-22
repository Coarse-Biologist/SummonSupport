using UnityEngine;

public class DoorHandler : MonoBehaviour, I_Interactable
{
    [SerializeField] public bool Open {get; private set;} = false; 

    public void Interact()
    {
        Open = true;
    }

    public void ShowInteractionOption()
    {
    }

    public void HideInteractionOption()
    {
    }
}
