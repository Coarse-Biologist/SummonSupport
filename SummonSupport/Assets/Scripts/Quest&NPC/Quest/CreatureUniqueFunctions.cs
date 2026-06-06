using UnityEngine;

public class CreatureUniqueFunctions : MonoBehaviour
{
    void Start()
    {
        Invoke("BeginDead", .1f);
    }
    void BeginDead()
    {
        if (TryGetComponent(out MinionStats minionStats))
        {
            minionStats.Die();
        }
    }


}
