using UnityEngine;

public class I_InteractMinionResurrect : MonoBehaviour, I_Interactable
{
    public void HideInteractionOption()
    {
        FloatingInfoHandler.Instance.HideInteractionOption();
    }

    public void Interact(GameObject interactor)
    {
        //if (interactor == null) return;
        if (interactor.TryGetComponent(out PlayerStats playerStats))
        {
            FloatingInfoHandler.Instance.DisplayIncrementalText(transform, "Resurrecting...", playerStats.ResurrectTime);
            playerStats.ResurrectMinion(gameObject);
        }
    }

    public void ShowInteractionOption()
    {
        FloatingInfoHandler.Instance.ShowInteractionOption(transform.position, "Z to resurrect");
    }
}
