using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Keeps the camera on the local player, offline and online, including when the player appears after the
/// level starts (online players are spawned by the host once the level has loaded).
/// </summary>
public class CameraFollowPlayer : MonoBehaviour
{
    private CinemachineCamera cam;

    void Start()
    {
        TryGetComponent(out cam);
    }

    void LateUpdate()
    {
        if (cam == null) return;
        PlayerStats player = PlayerStats.Instance;
        if (player != null && cam.Follow != player.transform) cam.Follow = player.transform;
    }
}
