
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TurretHandler : MonoBehaviour
{
    [SerializeField] Ability ability;
    [SerializeField] float cooldown         = 5f;
    [SerializeField] float rotationSpeed    = 5f;
    [SerializeField] GameObject canon;
    [SerializeField] GameObject projectileSpawn;
    public List<GameObject> listTargets     = new List<GameObject>();
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
                Logging.Verbose("Starts shooting" + other.gameObject.name);
                StartCoroutine(ShootLoop());
            }
            Logging.Info("Minion added: " + other.gameObject.name);

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
                
            Logging.Info("Minion removed: " + other.gameObject.name);
        }
    }
    
    private IEnumerator ShootLoop()
    {
        while (true)
        {
            GameObject target = GetTarget();
            yield return StartCoroutine(AimAtTargetOverTime(target));
            ShootTarget(target);
            if (shootTargetIndexNext >= listTargets.Count)
                shootTargetIndexNext = 0;
            yield return new WaitForSeconds(cooldown);
        }
    }
    GameObject GetTarget()
    {
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
        Quaternion startingRotation = canon.transform.rotation;
        
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * rotationSpeed;
            canon.transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, timeElapsed);
            yield return null;
        }

        canon.transform.rotation = targetRotation;
    }
    void ShootTarget(GameObject target)
    {
        if (ability == null)
        {
            Logging.Warning("Turret has no ability");
            return;
        }

        if (listTargets.Count <= 0)
        {
            Logging.Warning("Turret has no target");
            return;
        }
        
        if (ability is ProjectileAbility projectileAbility)
        {
            Logging.Info($"Turret fires ability {ability.Name} at {target.name}");
            projectileAbility.Activate(gameObject, projectileSpawn, canon.transform);
        }
    }

    
}
