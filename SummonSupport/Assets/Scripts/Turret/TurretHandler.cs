
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TurretHandler : AbilityHandler
{
    [SerializeField] float cooldown         = 5f;
    [SerializeField] float rotationSpeed    = 5f;
    

    public List<GameObject> listTargets = new List<GameObject>();
    public int shootTargetIndexNext;
    public bool hasTarget;

    void Start()
    {
        shootTargetIndexNext = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Minion") && (!listTargets.Contains(other.gameObject)))
        {
            listTargets.Add(other.gameObject);
            if (!hasTarget)
            {
                hasTarget = true;
                Logging.Verbose("Starts shooting " + other.gameObject.name);
                StartCoroutine(ShootLoop());
            }

        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Minion") && listTargets.Contains(other.gameObject))
        {
            listTargets.Remove(other.gameObject);
            if (hasTarget && listTargets.Count <= 0)
                {
                    hasTarget = false;
                    Logging.Verbose("Stops shooting" + other.gameObject.name);
                    StopCoroutine(ShootLoop());
                }
        }
    }
    
    private IEnumerator ShootLoop()
    {
        while (true)
        {
            GameObject target = GetTarget();
            if (target == null)
                yield break;
            yield return StartCoroutine(AimAtTargetOverTime(target));
            ShootTarget(target);
            if (shootTargetIndexNext >= listTargets.Count)
                shootTargetIndexNext = 0;
            yield return new WaitForSeconds(cooldown);
        }
    }

    GameObject GetTarget()
    {
        if (listTargets.Count == 0) 
            return null;

        shootTargetIndexNext %= listTargets.Count;
        GameObject target = listTargets[shootTargetIndexNext];
        shootTargetIndexNext += 1;
        return target;
    }

    private IEnumerator AimAtTargetOverTime(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        
        float timeElapsed = 0f;
        Quaternion startingRotation = abilityDirection.transform.rotation;
        
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * rotationSpeed;
            abilityDirection.transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, timeElapsed);
            yield return null;
        }

        abilityDirection.transform.rotation = targetRotation;
    }

    void ShootTarget(GameObject target)
    {
        if (abilities == null)
            return;

        if (listTargets.Count <= 0)
            return;
        
        CastAbility(0);
    }
}
