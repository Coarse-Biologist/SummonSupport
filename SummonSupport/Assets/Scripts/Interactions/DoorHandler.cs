using System.Collections.Generic;
using Quest;
using SummonSupportEvents;
using TMPro;
using UnityEngine;

/// <summary>
/// A door anyone can open or close. Online, the player's machine checks the lock (it has their real affinities),
/// then NetworkPlayer.RequestDoorToggle asks the host, which tells everyone the new state (see NetworkPlayer.Doors.cs).
/// Doors are scene objects, not network objects: each finds the same DoorId on every machine from its place in the scene.
/// </summary>
public class DoorHandler : MonoBehaviour, I_Interactable
{
    [SerializeField] public bool Open { get; private set; } = false;


    private int readyCooldownTime = 1;
    private float nextToggleTime; // this machine: stops a held Z from toggling (or asking the host) again straight away
    private float nextHostToggleTime; // host: the cooldown that counts online, so two players can't toggle at once
    private bool localPlayerInRange; // whether the local player is being shown this door's prompt

    private static readonly Dictionary<int, DoorHandler> doors = new();
    public int DoorId { get; private set; }
    [SerializeField] public bool Locked = false;

    [field: SerializeField] public Quest_SO QuestToUnlock { private set; get; } = null;

    [SerializeField] public Element elementalRequisite;
    [SerializeField] public int difficulty = 1;
    private Collider doorCollider;
    [field: SerializeField] public Transform DestinationDoor { get; protected set; }
    [field: SerializeField] public Transform canvasSpawnLoc { get; private set; }
    private Quaternion startRotation;
    private TextMeshProUGUI infoCanvasText;


    public void Awake()
    {
        startRotation = transform.rotation;
        doorCollider = GetComponent<Collider>();
        if (canvasSpawnLoc == null) canvasSpawnLoc = transform;
        Register();
    }

    void OnDestroy()
    {
        if (doors.TryGetValue(DoorId, out DoorHandler registered) && registered == this) doors.Remove(DoorId);
    }

    public static bool TryGetDoor(int doorId, out DoorHandler door)
    {
        return doors.TryGetValue(doorId, out door) && door != null;
    }

    public static IEnumerable<DoorHandler> AllDoors => doors.Values;

    // The id comes from the scene and the door's path in it, so every machine that loads the scene finds the same one.
    private void Register()
    {
        string path = gameObject.scene.name;
        for (Transform t = transform; t != null; t = t.parent)
            path += "/" + t.GetSiblingIndex() + ":" + t.name;

        uint hash = 2166136261; // FNV-1a: string.GetHashCode isn't guaranteed to match between machines
        foreach (char c in path)
        {
            hash ^= c;
            hash *= 16777619;
        }
        DoorId = (int)hash;

        // A copy left over from before the scene reloaded is replaced; two doors in the same scene would be a real clash.
        if (TryGetDoor(DoorId, out DoorHandler other) && other != this && other.gameObject.scene == gameObject.scene)
            Debug.LogWarning($"{name} and {other.name} have the same door id, so only one of them is kept in step online.");
        else doors[DoorId] = this;
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
        if (Time.time < nextToggleTime || !interactor.TryGetComponent(out LivingBeing livingBeing)) return;
        if (Locked && !HasElementalRequisite(livingBeing)) return;

        if (NetworkSession.IsOnline && interactor.TryGetComponent(out NetworkPlayer networkPlayer) && networkPlayer.IsSpawned)
        {
            nextToggleTime = Time.time + readyCooldownTime;
            networkPlayer.RequestDoorToggle(this);
        }
        else ApplyOpen(!Open, true);
    }

    /// <summary>Host side: true if the door may change now, and starts its cooldown.</summary>
    public bool TryStartHostToggle()
    {
        if (Time.time < nextHostToggleTime) return false;
        nextHostToggleTime = Time.time + readyCooldownTime;
        return true;
    }

    /// <summary>Opens or closes the door on this machine. Only the player who opened it goes through to DestinationDoor.</summary>
    public void ApplyOpen(bool open, bool teleportLocalPlayer)
    {
        nextToggleTime = Time.time + readyCooldownTime;
        if (open == Open) return;
        if (open) HandleOpen(teleportLocalPlayer);
        else HandleClose();
    }

    public void ShowInteractionOption()
    {
        localPlayerInRange = true;

        if (!Open) infoCanvasText = FloatingInfoHandler.Instance.ShowInteractionOption(canvasSpawnLoc.position, "Z to Open");
        else infoCanvasText = FloatingInfoHandler.Instance.ShowInteractionOption(canvasSpawnLoc.position, "Z to Close");
    }

    public void HideInteractionOption()
    {
        localPlayerInRange = false;
        FloatingInfoHandler.Instance.HideInteractionOption();

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
    void HandleOpen(bool teleportLocalPlayer)
    {
        SetPromptText("Z to Close");
        if (teleportLocalPlayer && DestinationDoor != null && PlayerStats.Instance != null)
            PlayerStats.Instance.gameObject.transform.position = DestinationDoor.position;

        transform.rotation = new Quaternion(0, 90, 0, 0);
        Open = true;
    }

    void HandleClose()
    {
        SetPromptText("Z to Open");

        //if (doorCollider != null)
        //    doorCollider.enabled = true;
        transform.rotation = startRotation;
        Open = false;
    }

    // Another player can move the door while the local player isn't near it, when there's no prompt of ours to update.
    private void SetPromptText(string text)
    {
        if (localPlayerInRange && infoCanvasText != null) infoCanvasText.text = text;
    }

    private void RequestCanvasText(string temporaryText)
    {
        FloatingInfoHandler.Instance.SetTemporaryCanvasText(transform, temporaryText);
    }
}
