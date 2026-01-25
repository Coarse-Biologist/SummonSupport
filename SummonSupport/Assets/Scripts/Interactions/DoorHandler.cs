using Quest;
using SummonSupportEvents;
using UnityEngine;

public class DoorHandler : MonoBehaviour, I_Interactable
{
    [SerializeField] public bool Open { get; private set; } = false;


    private int readyCooldownTime = 1;
    private bool ready = true;
    [SerializeField] public bool Locked = false;

    [field: SerializeField] public Quest_SO QuestToUnlock { private set; get; } = null;

    [SerializeField] public Element elementalRequisite;
    [SerializeField] public int difficulty = 1;
    private Collider doorCollider;
    [field: SerializeField] public Transform DestinationDoor { get; protected set; }
    [field: SerializeField] public Transform canvasSpawnLoc { get; private set; }


    public void Awake()
    {
        doorCollider = GetComponent<Collider>();
        if (canvasSpawnLoc == null) canvasSpawnLoc = transform;
    }
    void OnEnable()
    {
        EventDeclarer.QuestCompleted?.AddListener(CheckQuestRelatedDoor);
    }

    void OnDisable()
    {
        EventDeclarer.QuestCompleted?.RemoveListener(CheckQuestRelatedDoor);

    }

    public void Interact(GameObject interactor)
    {
        if (TryGetComponent(out LivingBeing livingBeing))
        {
            if (!Locked || HasElementalRequisite(livingBeing))
                ToggleOpenDoor();
        }
    }

    public void ShowInteractionOption()
    {
        if (!Open) FloatingInfoHandler.Instance.ShowInteractionOption(canvasSpawnLoc.position, "Tab to Open");
        else FloatingInfoHandler.Instance.ShowInteractionOption(canvasSpawnLoc.position, "Tab to Close");
    }

    public void HideInteractionOption()
    {
        FloatingInfoHandler.Instance.HideInteractionOption();

    }
    private void ToggleOpenDoor()
    {
        if (ready)
        {
            if (!Open)
            {
                HandleOpen();
            }
            else
            {
                HandleClose();
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
        Invoke("ReadyInteract", readyCooldownTime);

    }

    private void SetLocked(bool isLocked)
    {
        Locked = isLocked;
    }
    private void CheckQuestRelatedDoor(Quest_SO quest)
    {
        if (quest == QuestToUnlock)
        {
            SetLocked(false);
        }
    }

    private bool HasElementalRequisite(LivingBeing livingBeing)
    {
        if (elementalRequisite == Element.None) return true;
        if (livingBeing.Affinities[elementalRequisite].Get() > difficulty * 10) return true;
        else
        {
            //Logging.Info($"{livingBeing.name} did not have the required elemental affinity to open the door");
            FloatingInfoHandler.Instance.ShowInteractionOption(transform.position, "Failed to open");
            return false;
        }
    }
    void HandleOpen()
    {
        if (DestinationDoor != null)
            PlayerStats.Instance.gameObject.transform.position = DestinationDoor.position;

        if (doorCollider != null)
            doorCollider.enabled = false;

        transform.rotation = new Quaternion(0, 90, 0, 0);
        NotReadyToInteract();
        Open = true;
    }

    void HandleClose()
    {
        if (doorCollider != null)
            doorCollider.enabled = true;
        transform.rotation = Quaternion.identity;

        NotReadyToInteract();
        Open = false;

    }

    private void RequestCanvasText(string temporaryText)
    {
        FloatingInfoHandler.Instance.SetTemporaryCanvasText(transform, temporaryText);
    }
}
