using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// The player's character name online. The owner decides it (a new character, see CharacterCreationPipeline,
/// or a loaded save) and shares it, so every machine shows it and the host saves guests under it.
/// </summary>
public partial class NetworkPlayer
{
    private readonly NetworkVariable<FixedString64Bytes> characterName = new(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private string lastSharedName;

    private void StartCharacterName()
    {
        characterName.OnValueChanged += OnCharacterNameChanged;
        if (!IsOwner) ApplyCharacterName(characterName.Value);
    }

    private void StopCharacterName()
    {
        characterName.OnValueChanged -= OnCharacterNameChanged;
    }

    // Owner side, every frame: share the name whenever it changes.
    private void UpdateCharacterName()
    {
        string name = GetComponent<PlayerStats>().Name ?? "";
        if (name == lastSharedName) return;
        lastSharedName = name;

        FixedString64Bytes shared = default;
        shared.CopyFromTruncated(name);
        characterName.Value = shared;
    }

    private void OnCharacterNameChanged(FixedString64Bytes previous, FixedString64Bytes next)
    {
        if (!IsOwner) ApplyCharacterName(next);
    }

    private void ApplyCharacterName(FixedString64Bytes name)
    {
        if (name.Length > 0) GetComponent<PlayerStats>().SetName(name.ToString());
    }
}
