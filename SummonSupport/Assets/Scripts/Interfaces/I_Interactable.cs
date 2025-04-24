using UnityEngine;

public interface I_Interactable
{
    void ShowInteractionOption();
    void HideInteractionOption();

    void Interact(GameObject interactor);
}
