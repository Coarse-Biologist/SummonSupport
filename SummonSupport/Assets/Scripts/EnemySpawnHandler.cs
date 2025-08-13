using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

//using System.Numerics;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawnHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject enemyPrefab { get; private set; } = null;
    [field: SerializeField] public GameObject SpawnCenter { get; private set; } = null;



    void OnEnable()
    {
        EventDeclarer.SpawnEnemies.AddListener(SpawnEnemies);
    }
    void OnDisable()
    {
        EventDeclarer.SpawnEnemies.RemoveListener(SpawnEnemies);
    }

    private void SpawnEnemies(GameObject testNothing = null)
    {
        List<GameObject> spawnedCreatures = new();
        if (SpawnCenter != null && SpawnCenter.TryGetComponent<SpawnLocationInfo>(out SpawnLocationInfo spawnInfo) && spawnInfo.Creatures.Count > 0)
        {

            List<Vector2> spawnLocs = GetSpawnLocations(SpawnCenter.transform.position, spawnInfo.radius, spawnInfo.desiredSpawns);
            foreach (Vector2 loc in spawnLocs)
            {

                spawnedCreatures.Add(Instantiate(spawnInfo.Creatures[Random.Range(0, spawnInfo.Creatures.Count)], loc, Quaternion.identity));
            }
            if (spawnInfo.MoveTowardLocation && spawnInfo.TargetLocation != null)
            {

                foreach (GameObject creature in spawnedCreatures)
                {

                    MoveTowardTarget(creature, spawnInfo.TargetLocation.position);
                }
            }
        }

    }
    private void MoveTowardTarget(GameObject creature, Vector2 loc)
    {
        if (creature.TryGetComponent<AIStateHandler>(out AIStateHandler stateHandler))
        {
            stateHandler.lastSeenLoc = PlayerStats.Instance.transform.position;
            stateHandler.SetTarget(PlayerStats.Instance);
            stateHandler.SetCurrentState(creature.GetComponent<AIChaseState>());
        }
        if (creature.TryGetComponent<Collider2D>(out Collider2D collider))
        {

            collider.isTrigger = true;
        }
    }

    private List<Vector2> GetSpawnLocations(Vector2 origin, float radius, int desiredSpawns)
    {
        List<Vector2> spawnLocs = new();
        for (int i = 0; i < desiredSpawns; i++)
        {
            spawnLocs.Add(new Vector2(origin.x + GetRandomOffset(radius), origin.y + GetRandomOffset(radius)));
        }
        return spawnLocs;
    }

    private float GetRandomOffset(float radius)
    {
        return Random.Range(-radius, radius);
    }
}


