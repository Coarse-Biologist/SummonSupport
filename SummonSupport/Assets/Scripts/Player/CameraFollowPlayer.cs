using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    void Start()
    {
        if (gameObject.TryGetComponent(out CinemachineCamera cam))
        {
            PlayerStats player = FindFirstObjectByType<PlayerStats>();
            if (player != null)
                cam.Follow = FindFirstObjectByType<PlayerStats>().transform;
        }
    }


}
