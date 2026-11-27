using UnityEngine;

/// <summary>
/// Added to a dead minion or a downed player so that a living player or a commanded minion can resurrect it
/// by staying close for a few seconds (see I_Resurrector).
/// </summary>
public class I_InteractResurrect : MonoBehaviour, I_Interactable
{
    public bool resurrecting { private set; get; } = false;

    public void HideInteractionOption()
    {
        FloatingInfoHandler.Instance.HideInteractionOption();
    }

    public void Interact(GameObject interactor)
    {
        if (resurrecting || !CanResurrect(interactor)) return;
        if (interactor.TryGetComponent(out I_Resurrector resurrector))
        {
            SetResurrecting(true);
            FloatingInfoHandler.Instance.DisplayIncrementalText(transform, "Resurrecting...", resurrector.ResurrectTime);
            resurrector.Resurrect(gameObject, this);
        }
    }

    // A living player resurrects from their own machine; a minion where its AI runs (the host, or offline).
    private bool CanResurrect(GameObject interactor)
    {
        if (interactor == gameObject) return false;
        if (!interactor.TryGetComponent(out LivingBeing being) || being.Dead) return false;
        if (being is PlayerStats) return being == PlayerStats.Instance;
        return NetworkSession.IsAuthority;
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
