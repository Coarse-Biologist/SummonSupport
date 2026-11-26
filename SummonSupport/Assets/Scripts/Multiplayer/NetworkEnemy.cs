using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkEnemy : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer) return;

        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is NetworkBehaviour) continue;
            if (behaviour.GetType().Name.StartsWith("AI")) behaviour.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null) body.isKinematic = true;
    }
}
