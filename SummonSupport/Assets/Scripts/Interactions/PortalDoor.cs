using UnityEngine;

public class PortalDoor : MonoBehaviour
{

    [field: SerializeField] public Transform DestinationDoor { get; protected set; }


    void OnTriggerEnter(Collider collision)
    {
        //Debug.Log($"Someone or something collided with {collision.gameObject.name}");
        if (collision.gameObject.GetComponent<PlayerStats>() != null && DestinationDoor != null)
        {
            collision.gameObject.transform.position = DestinationDoor.position;
        }
        //else Debug.Log($"Something collided with a  portaldoor script contatining object, but it wasnt an object containing a Player stats script");
    }
}
