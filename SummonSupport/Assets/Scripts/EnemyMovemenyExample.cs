using UnityEngine;
using UnityEngine.AI;

public class EnemyMovemenyExample : MonoBehaviour
{
    [SerializeField] Transform target;
    private NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        agent.SetDestination(target.position);
    }
}
