using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DoorHandler : MonoBehaviour, I_Interactable
{
    [SerializeField] public bool Open { get; private set; } = false;

    public Sprite ClosedSprite;// { private set; get; }
    public Sprite OpenSprite;// { private set; get; }
    private int readyCooldownTime = 1;
    private bool ready = true;
    [SerializeField] public bool Locked = false;

    [SerializeField] public Element elementalRequisite;
    [SerializeField] public int difficulty = 1;



    private SpriteRenderer minionsSpriteRenderer;
    public void Awake()
    {
        minionsSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Interact(GameObject interactor)
    {
        LivingBeing livingBeing = interactor.GetComponent<LivingBeing>();
        if (livingBeing != null && HasElementalRequisite(livingBeing)) ToggleOpenDoor();
    }

    public void ShowInteractionOption()
    {
        if (!Open) InteractCanvasHandler.Instance.ShowInteractionOption(transform.position, "Tab to Open");
        else InteractCanvasHandler.Instance.ShowInteractionOption(transform.position, "Tab to Close");
    }

    public void HideInteractionOption()
    {
        InteractCanvasHandler.Instance.HideInteractionOption();

    }
    private void ToggleOpenDoor()
    {
        if (ready)
        {
            if (!Open)
            {
                NotReadyToInteract();
                Open = true;
                minionsSpriteRenderer.sprite = OpenSprite;
                Invoke("ReadyInteract", readyCooldownTime);
            }
            else
            {
                NotReadyToInteract();
                Open = false;
                minionsSpriteRenderer.sprite = ClosedSprite;
                Invoke("ReadyInteract", readyCooldownTime);
            }
        }
    }

    private void ReadyInteract()
    {
        ready = true;
    }
    private void NotReadyToInteract()
    {
        ready = false;
    }

    private void SetLocked(bool isLocked)
    {
        Locked = isLocked;
    }

    private bool HasElementalRequisite(LivingBeing livingBeing)
    {
        if (elementalRequisite == Element.None) return true;
        if (livingBeing.Affinities[elementalRequisite].Get() > difficulty * 10) return true;
        else
        {
            Logging.Info($"{livingBeing.name} did not have the required elemental affinity to open the door");
            InteractCanvasHandler.Instance.ShowInteractionOption(transform.position, "Failed to open");
            return false;
        }
    }

    private void RequestCanvasText(string temporaryText)
    {
        InteractCanvasHandler.Instance.SetTemporaryCanvasText(transform, temporaryText);
    }
}
