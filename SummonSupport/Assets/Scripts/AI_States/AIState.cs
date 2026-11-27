using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    public abstract AIState RunCurrentState();
}
