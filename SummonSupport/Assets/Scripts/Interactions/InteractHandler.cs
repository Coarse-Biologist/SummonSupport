using System.Collections.Generic;
using UnityEngine;

public class InteractHandler : MonoBehaviour
{
    // Only the local player's copy interacts: NetworkPlayer disables this on other players' copies.
    // Unity still sends trigger messages and runs Invoke on a disabled component, so each entry point checks enabled.
    private readonly List<I_Interactable> interactablesInRange = new();

    // The one entered last is the one Z uses.
    private I_Interactable Current => interactablesInRange.Count > 0 ? interactablesInRange[interactablesInRange.Count - 1] : null;

    void OnDisable()
    {
        CancelInvoke(nameof(AttemptInteraction));
        if (IsUsable(Current)) Current.HideInteractionOption();
        interactablesInRange.Clear();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (!enabled || !collision.gameObject.TryGetComponent(out I_Interactable interactable) || interactablesInRange.Contains(interactable)) return;

        interactablesInRange.Add(interactable);
        interactable.ShowInteractionOption();
        if (interactablesInRange.Count == 1) InvokeRepeating(nameof(AttemptInteraction), 0f, .05f);
    }

    void OnTriggerExit(Collider collision)
    {
        if (!enabled || !collision.gameObject.TryGetComponent(out I_Interactable interactable) || !interactablesInRange.Remove(interactable)) return;

        interactable.HideInteractionOption();
        RemoveDestroyed();
        if (Current != null) Current.ShowInteractionOption();
        else CancelInvoke(nameof(AttemptInteraction));
    }

    private void AttemptInteraction()
    {
        RemoveDestroyed();
        if (!enabled || Current == null)
        {
            CancelInvoke(nameof(AttemptInteraction));
            return;
        }
        if (Input.GetKey(KeyCode.Z)) Current.Interact(gameObject);
    }

    // An interactable can be destroyed or removed while we stand in it (e.g. a resurrected minion's I_InteractResurrect).
    private void RemoveDestroyed()
    {
        interactablesInRange.RemoveAll(interactable => !IsUsable(interactable));
    }

    private static bool IsUsable(I_Interactable interactable)
    {
        return interactable != null && !interactable.Equals(null);
    }
}
