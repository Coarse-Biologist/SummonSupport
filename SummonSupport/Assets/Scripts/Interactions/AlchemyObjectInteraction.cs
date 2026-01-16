using UnityEngine;

public class AlchemyObjectInteraction : MonoBehaviour, I_Interactable
{
    [field: SerializeField] Transform playerHand_L;
    [field: SerializeField] Transform playerHand_R;
    [field: SerializeField] GameObject potion1;

    [field: SerializeField] GameObject potion2;

    private GameObject leftPotion;
    private GameObject rightPotion;

    public void HideInteractionOption()
    {
        //Debug.Log("destroying potions or maybe not?");
        if (leftPotion != null && rightPotion != null)
        {
            //Debug.Log("destroying potions!?");

            Destroy(leftPotion);
            Destroy(rightPotion);
            leftPotion = null;
            rightPotion = null;
        }
        else Debug.Log("No potions to delete");
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log("interqct called on screeglers neegler");
        if (leftPotion == null && rightPotion == null)
        {
            leftPotion = Instantiate(potion1, playerHand_L.transform.position, Quaternion.identity, playerHand_L);
            rightPotion = Instantiate(potion2, playerHand_R.transform.position, Quaternion.identity, playerHand_R);
        }

    }

    public void ShowInteractionOption()
    {
        throw new System.NotImplementedException();
    }
}
