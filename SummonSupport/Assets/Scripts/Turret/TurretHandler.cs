using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TurretHandler : MonoBehaviour
{
    [SerializeField] Ability ability;
    [SerializeField] float cooldown = 5f;
    public List<GameObject> listTargets = new List<GameObject>();
    public int shootTargetIndexNext;
    public bool hasTarget;
    void Start()
    {
        shootTargetIndexNext = 0;
    }
    private IEnumerator ShootLoop()
    {
        while (true)
        {
            ShootTarget();
            yield return new WaitForSeconds(cooldown);
        }
    }
    void ShootTarget()
    {
        if (listTargets.Count <= 0 || ability == null)
        {
            Logging.Warning("Turret has no ability");
            return;
        }
        GameObject target = listTargets[shootTargetIndexNext];
        Logging.Info($"Turret fires ability {ability.Name} at {target.name}");
        ability.Activate(gameObject);
        shootTargetIndexNext += 1;
        if (shootTargetIndexNext >= listTargets.Count)
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
                    StopCoroutine(ShootLoop());
                }
                
            Logging.Info("Minion removed: " + other.gameObject.name);
        }
    }
}
