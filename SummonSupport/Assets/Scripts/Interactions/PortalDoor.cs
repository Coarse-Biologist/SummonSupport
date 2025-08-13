using UnityEngine;

public class PortalDoor : MonoBehaviour
{

    [field: SerializeField] public Transform DestinationDoor { get; protected set; }


    void OnTriggerEnter2D(Collider2D collision)
    {
        Logging.Info(collision.gameObject.name.ToString());
        if (collision.gameObject.GetComponent<PlayerStats>() != null)
        {
            collision.gameObject.transform.position = DestinationDoor.position;
        }
        //else Logging.Info($"{collision.gameObject.name} was not a player");
    }
}
