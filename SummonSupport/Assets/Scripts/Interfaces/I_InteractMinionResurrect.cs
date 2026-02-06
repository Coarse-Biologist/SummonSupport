using UnityEngine;

public class I_InteractMinionResurrect : MonoBehaviour, I_Interactable
{
    public bool resurrecting { private set; get; } = false;

    public void HideInteractionOption()
    {
        FloatingInfoHandler.Instance.HideInteractionOption();
    }

    public void Interact(GameObject interactor)
    {
        if (!resurrecting)
            if (interactor.TryGetComponent(out PlayerStats playerStats))
            {
                SetResurrecting(true);
                FloatingInfoHandler.Instance.DisplayIncrementalText(transform, "Resurrecting...", playerStats.ResurrectTime);
                playerStats.ResurrectMinion(gameObject, this);
            }
    }

    public void ShowInteractionOption()
    {
        if (!resurrecting)
            FloatingInfoHandler.Instance.ShowInteractionOption(transform.position, "Z to resurrect");
    }

    public void SetResurrecting(bool isResurrecting)
    {
        resurrecting = isResurrecting;
    }

}
