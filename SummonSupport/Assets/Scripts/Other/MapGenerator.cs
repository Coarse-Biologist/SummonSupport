using System;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [field: SerializeField] public GameObject[] FloorObjects { private set; get; }
    [field: SerializeField] public GameObject[] LargeForestObjects { private set; get; }
    [field: SerializeField] public GameObject[] MediumForestObjects { private set; get; }
    [field: SerializeField] public GameObject[] SmallForestObjects { private set; get; }
    [field: SerializeField] public GameObject[] ForestBuildingObjects { private set; get; }

    #region floor variables
    private GameObject floor;
    private Vector3 floorScale;
    private Renderer floorRenderer;
    private Collider floorCollider;
    private int objectGap = 5;

    #endregion

    private void Start()
    {
        //SpawnRandomMap();
    }
    void SpawnRandomMap()
    {
        if (FloorObjects.Count() != 0)
        {
            SpawnFloor();
            SpawnLevelObjects();
        }
        else Debug.Log("There are no floor objects.");
    }
    private void SpawnFloor()
    {
        floor = Instantiate(FloorObjects[UnityEngine.Random.Range(0, FloorObjects.Count())]);
        floorRenderer = floor.GetComponent<Renderer>();
        floorCollider = floor.GetComponent<Collider>();

        floorScale = floor.transform.localScale;
    }
    private void SpawnLevelObjects()
    {
        Vector3 floorCenter = floorRenderer.bounds.center;
        Vector3 floorExtents = floorRenderer.bounds.extents;
        Bounds floorBounds = floorRenderer.bounds;

        Vector3 bottomLeft = floorCenter + new Vector3(-floorExtents.x, floorExtents.y, -floorExtents.z);
        Vector3 size = floorBounds.size;
        Vector3 min = floorBounds.min;
        Vector3 max = floorBounds.max;
        GameObject objectToSpawn;
        for (int i = 0; i < size.x; i += objectGap)
        {
            for (int j = 0; j < size.z; j += objectGap)
            {
                if (FindHighestPointAtLoc(new Vector3(min.x + i * objectGap, max.y, min.z + j * objectGap), out Vector3 hitPoint))
                {
                    objectToSpawn = GetRandomObject();
                    Instantiate(objectToSpawn, hitPoint, objectToSpawn.transform.rotation);
                }
                else Debug.Log($"No point found at loc {bottomLeft.x - i * objectGap}, 9, {bottomLeft.z - j * objectGap}");
            }
        }
    }

    private GameObject GetRandomObject()
    {
        int randomRoll = UnityEngine.Random.Range(0, 100);
        if (randomRoll > 80)
            return LargeForestObjects[UnityEngine.Random.Range(0, LargeForestObjects.Count())];
        else if (randomRoll > 30)
            return MediumForestObjects[UnityEngine.Random.Range(0, MediumForestObjects.Count())];
        else
            return SmallForestObjects[UnityEngine.Random.Range(0, SmallForestObjects.Count())];
    }
    private bool FindHighestPointAtLoc(Vector3 loc, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        Vector3 rayStart = new Vector3(loc.x, floorRenderer.bounds.max.y + 3, loc.z);
        Ray ray = new Ray(rayStart, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            hitPoint = hit.point;
            return true;
        }
        return false;
    }
}

public enum MapType
{
    Forest,
    Castle,
}
