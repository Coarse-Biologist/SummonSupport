using UnityEngine;

public class MinionStats : MonoBehaviour
{

    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
