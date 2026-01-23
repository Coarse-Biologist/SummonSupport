using System.Collections;
using System.Collections.Generic;
using SummonSupportEvents;
//using Unity.AppUI.UI;
using UnityEngine;

public class PullScript : MonoBehaviour
{
    [field: SerializeField] public float Duration = 4f;
    [field: SerializeField] public float PullStrength = 6f;

    private List<Rigidbody> targets = new();
    private WaitForSeconds waitTime = new WaitForSeconds(.1f);
    float pullTime = 5f;
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Enemy"));
        foreach (Collider col in rangeChecks)
        {
            if (col.gameObject.TryGetComponent(out Rigidbody rb))
            {
                if (!targets.Contains(rb))
                {
                    targets.Add(rb);
                    EventDeclarer.PlantAttack?.Invoke(rb);
                }
            }
        }
        StartCoroutine(PullTargets());
        Destroy(gameObject, 4f);

    }

    private IEnumerator PullTargets()
    {
        while (pullTime > 0)
        {
            pullTime -= .1f;

            foreach (Rigidbody rb in targets)
            {
                if (rb == null) continue;
                Vector3 direction = (transform.position - rb.position).normalized;
                rb.AddForce(direction * PullStrength, ForceMode.Impulse);
            }

            yield return waitTime;
        }
    }


}
