using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;

public class PullScript : MonoBehaviour
{
    private List<Rigidbody> targets = new();
    private WaitForSeconds waitTime = new WaitForSeconds(.1f);
    float pullTime = 5f;
    void Start()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Enemy"));
        foreach (Collider col in rangeChecks)
        {
            if (col.gameObject.TryGetComponent(out Rigidbody rb))
            {
                if (!targets.Contains(rb))
                    targets.Add(rb);
            }
        }
        StartCoroutine(PullTargets());

    }

    private IEnumerator PullTargets()
    {
        while (pullTime > 0)
        {
            pullTime -= .1f;

            foreach (Rigidbody rb in targets)
            {
                Debug.Log($"Applying force to {rb.gameObject.name}");
                Vector3 direction = (transform.position - rb.position).normalized;
                rb.AddForce(direction * 5, ForceMode.Impulse);
            }

            yield return waitTime;
        }
    }


}
