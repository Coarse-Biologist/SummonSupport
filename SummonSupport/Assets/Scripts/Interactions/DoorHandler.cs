using UnityEngine;

public class DoorHandler : MonoBehaviour, I_Interactable
{
    [SerializeField] public bool Open { get; private set; } = false;

    public Sprite ClosedSprite;// { private set; get; }
    public Sprite OpenSprite;// { private set; get; }
    private int readyCooldownTime = 1;
    private bool ready = true;

    private SpriteRenderer minionsSpriteRenderer;
    public void Awake()
    {
        minionsSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Interact()
    {
        ToggleOpenDoor();
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
                Logging.Info("Door is now open");
                Open = true;
                minionsSpriteRenderer.sprite = OpenSprite;
                Invoke("ReadyInteract", readyCooldownTime);
            }
            else
            {
                NotReadyToInteract();
                Logging.Info("Door is now closed");
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
}
