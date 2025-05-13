using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawnHandler : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab1 = null;
    [SerializeField] int avrgEnemiesPerWaves = 5;
    [SerializeField] int avrgTimeBetweenWaves = 10;
    [SerializeField] int minDistanceToPlayer = 5;
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask avoidMask;

    void OnEnable()
    {
        EventDeclarer.SpawnEnemies.AddListener(SpawnEnemies);
    }
    void OnDisable()
    {
        EventDeclarer.SpawnEnemies.RemoveListener(SpawnEnemies);
    }

    private void SpawnEnemies(Vector2 targetArea)
    {
        List<Vector2> spawnLocs = GetSpawnLocations(targetArea);
        foreach (Vector2 loc in spawnLocs)
        {
            Instantiate(enemyPrefab1, loc, Quaternion.identity);
        }
    }

    private List<Vector2> GetSpawnLocations(Vector2 targetCenter)
    {
        LayerMask finalMask = targetMask & ~avoidMask;
        Vector2 origin = Camera.main.transform.position;
        List<Vector2> spawnLocs = new List<Vector2>();
        int spawnLocFound = 0;
        int goalSpawnLoc = GetVariedSpawnNumber();

        int maxAttempts = 1000;
        int attempts = 0;

        while (spawnLocFound < goalSpawnLoc && attempts < maxAttempts)
        {
            int xOffset = GetRandomOffset();
            int yOffset = GetRandomOffset();

            Logging.Info($" x offset =  {xOffset}, y offset = {yOffset}");

            Vector2 direction = new Vector2(targetCenter.x + xOffset, targetCenter.y + yOffset);
            Logging.Info($" direction = {direction}");
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, finalMask);
            if (hit)
            {
                spawnLocs.Add(direction);
                spawnLocFound++;
                Logging.Info($" Spawn loc found: {direction}");
            }
        }
        return spawnLocs;
    }

    private int GetVariedRadiusSize()
    {
        return Random.Range(minDistanceToPlayer, minDistanceToPlayer + 5);
    }
    private int GetVariedSpawnNumber()
    {
        return Random.Range(avrgEnemiesPerWaves - 3, avrgEnemiesPerWaves + 3);
    }
    private int GetRandomOffset()
    {
        return Random.Range(-5, 5);
    }
}
