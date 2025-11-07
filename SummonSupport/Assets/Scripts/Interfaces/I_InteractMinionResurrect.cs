using UnityEngine;

public class I_InteractMinionResurrect : MonoBehaviour, I_Interactable
{
    public void HideInteractionOption()
    {
        InteractCanvasHandler.Instance.HideInteractionOption();
    }

    public void Interact(GameObject interactor)
    {
        //if (interactor == null) return;
        if (interactor.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            InteractCanvasHandler.Instance.DisplayIncrementalText(transform.position, "Resurrecting...", playerStats.ResurrectTime);
            playerStats.ResurrectMinion(gameObject);
        }
    }

    public void ShowInteractionOption()
    {
        InteractCanvasHandler.Instance.ShowInteractionOption(transform.position, "Tab to resurrect");
    }
}
