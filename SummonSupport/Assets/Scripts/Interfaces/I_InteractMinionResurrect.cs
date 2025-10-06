using UnityEngine;

public class I_InteractMinionResurrect : MonoBehaviour, I_Interactable
{
    public void HideInteractionOption()
    {
        InteractCanvasHandler.Instance.HideInteractionOption();
    }

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            InteractCanvasHandler.Instance.DisplayIncrementalText(transform.position, "Resurrecting...");
            playerStats.ResurrectMinion(this.gameObject);
        }
    }

    public void ShowInteractionOption()
    {
        InteractCanvasHandler.Instance.ShowInteractionOption(transform.position, "Tab to resurrect");
    }
}
