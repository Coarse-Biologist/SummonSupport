using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    public GameObject detectedTargetObject;
    public abstract AIState RunCurrentState();
}
