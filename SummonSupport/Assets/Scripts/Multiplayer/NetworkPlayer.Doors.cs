using Unity.Netcode;

/// <summary>
/// Keeping doors the same for everyone (see DoorHandler).
/// A player's machine asks the host to toggle a door; the host applies the cooldown and tells every machine the new state.
/// A player who joins later is sent the doors that are already open.
/// </summary>
public partial class NetworkPlayer
{
    /// <summary>Owner side: opens or closes a door for everyone, through the host.</summary>
    public void RequestDoorToggle(DoorHandler door)
    {
        if (!IsSpawned || !IsOwner || door == null) return;
        if (IsServer) ToggleDoorForEveryone(door.DoorId);
        else RequestDoorToggleServerRpc(door.DoorId);
    }

    [ServerRpc]
    private void RequestDoorToggleServerRpc(int doorId)
    {
        ToggleDoorForEveryone(doorId);
    }

    private void ToggleDoorForEveryone(int doorId)
    {
        if (!DoorHandler.TryGetDoor(doorId, out DoorHandler door) || !door.TryStartHostToggle()) return;
        DoorClientRpc(doorId, !door.Open, OwnerClientId);
    }

    [ClientRpc]
    private void DoorClientRpc(int doorId, bool open, ulong openedBy, ClientRpcParams rpcParams = default)
    {
        if (DoorHandler.TryGetDoor(doorId, out DoorHandler door))
            door.ApplyOpen(open, openedBy == NetworkManager.LocalClientId);
    }

    /// <summary>Host side: brings a joining player's doors up to date.</summary>
    private void SendOpenDoorsToOwner()
    {
        foreach (DoorHandler door in DoorHandler.AllDoors)
            if (door != null && door.Open) DoorClientRpc(door.DoorId, true, NetworkManager.ServerClientId, ToOwner());
    }
}
