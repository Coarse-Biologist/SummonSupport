using UnityEngine;

public class MinionInteractionHandler : MonoBehaviour
{
    [SerializeField] public bool CommandedToInteract {get; private set;}= false;

public void SetCommandToInteract(bool isCommanded)
{

}

    void OnTriggerEnter2D(Collider2D collision)
    {
        I_Interactable interactInterfaceInstance = collision.gameObject.GetComponent<I_Interactable>();
        if (interactInterfaceInstance != null)
        {
            interactInterfaceInstance.ShowInteractionOption();
        }
    }
}
