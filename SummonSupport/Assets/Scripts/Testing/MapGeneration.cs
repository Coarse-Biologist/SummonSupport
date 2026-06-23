using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    [Header("Dungeon Size")]
    [SerializeField] private int dungeonWidth = 120;
    [SerializeField] private int dungeonHeight = 120;

    [Header("Room Settings")]
    [SerializeField] private int roomCount = 20;

    [SerializeField] private Vector2Int smallRoomSizeMin = new Vector2Int(4, 4);
    [SerializeField] private Vector2Int smallRoomSizeMax = new Vector2Int(8, 8);

    [SerializeField] private Vector2Int largeRoomSizeMin = new Vector2Int(10, 10);
    [SerializeField] private Vector2Int largeRoomSizeMax = new Vector2Int(20, 20);

    [SerializeField] private float largeRoomChance = 0.3f;

    [Header("Hallway Settings")]
    [SerializeField] private int hallwayWidth = 2;

    [Header("World Settings")]
    [SerializeField] private float tileSize = 4f;
    [SerializeField] private float wallHeight = 4f;

    [Header("Prefabs")]
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material wallMaterial;

    [Header("Decoration")]
    [SerializeField] private List<GameObject> spawnableObjects = new List<GameObject>();
    [SerializeField] private int objectsPerRoomMin = 1;
    [SerializeField] private int objectsPerRoomMax = 6;

    [Header("Random Seed")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 0;

    private bool[,] floorMap;

    private List<Room> rooms = new List<Room>();

    private Transform dungeonParent;
    private Transform floorParent;
    private Transform wallParent;
    private Transform objectParent;

    private void Start()
    {
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();

        if (useRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }

        Random.InitState(seed);

        floorMap = new bool[dungeonWidth, dungeonHeight];

        CreateParents();

        GenerateRooms();

        ConnectRooms();

        GenerateFloorMeshes();

        GenerateWalls();

        SpawnObjects();
    }

    private void CreateParents()
    {
        dungeonParent = new GameObject("Dungeon").transform;

        floorParent = new GameObject("Floors").transform;
        floorParent.parent = dungeonParent;

        wallParent = new GameObject("Walls").transform;
        wallParent.parent = dungeonParent;

        objectParent = new GameObject("Objects").transform;
        objectParent.parent = dungeonParent;
    }

    private void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            bool largeRoom = Random.value < largeRoomChance;

            Vector2Int minSize = largeRoom ? largeRoomSizeMin : smallRoomSizeMin;
            Vector2Int maxSize = largeRoom ? largeRoomSizeMax : smallRoomSizeMax;

            int width = Random.Range(minSize.x, maxSize.x + 1);
            int height = Random.Range(minSize.y, maxSize.y + 1);

            int x = Random.Range(1, dungeonWidth - width - 1);
            int y = Random.Range(1, dungeonHeight - height - 1);

            Room room = new Room(x, y, width, height);

            bool overlaps = false;

            foreach (Room other in rooms)
            {
                if (room.Intersects(other, 2))
                {
                    overlaps = true;
                    break;
                }
            }

            if (overlaps)
                continue;

            rooms.Add(room);

            CarveRoom(room);
        }
    }

    private void CarveRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                floorMap[x, y] = true;
            }
        }
    }

    private void ConnectRooms()
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int a = rooms[i - 1].Center;
            Vector2Int b = rooms[i].Center;

            if (Random.value > 0.5f)
            {
                CreateHorizontalHallway(a.x, b.x, a.y);
                CreateVerticalHallway(a.y, b.y, b.x);
            }
            else
            {
                CreateVerticalHallway(a.y, b.y, a.x);
                CreateHorizontalHallway(a.x, b.x, b.y);
            }
        }
    }

    private void CreateHorizontalHallway(int x1, int x2, int y)
    {
        int start = Mathf.Min(x1, x2);
        int end = Mathf.Max(x1, x2);

        for (int x = start; x <= end; x++)
        {
            for (int w = 0; w < hallwayWidth; w++)
            {
                int py = y + w;

                if (InsideBounds(x, py))
                {
                    floorMap[x, py] = true;
                }
            }
        }
    }

    private void CreateVerticalHallway(int y1, int y2, int x)
    {
        int start = Mathf.Min(y1, y2);
        int end = Mathf.Max(y1, y2);

        for (int y = start; y <= end; y++)
        {
            for (int w = 0; w < hallwayWidth; w++)
            {
                int px = x + w;

                if (InsideBounds(px, y))
                {
                    floorMap[px, y] = true;
                }
            }
        }
    }

    private bool InsideBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < dungeonWidth && y < dungeonHeight;
    }

    private void GenerateFloorMeshes()
    {
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (!floorMap[x, y])
                    continue;

                Vector3 pos = GridToWorld(x, y);

                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);

                floor.name = $"Floor_{x}_{y}";

                floor.transform.position = pos;
                floor.transform.localScale = Vector3.one * (tileSize / 10f);

                floor.transform.parent = floorParent;

                if (floorMaterial != null)
                {
                    floor.GetComponent<MeshRenderer>().material = floorMaterial;
                }
            }
        }
    }

    private void GenerateWalls()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (!floorMap[x, y])
                    continue;

                foreach (Vector2Int dir in directions)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    if (!InsideBounds(nx, ny) || !floorMap[nx, ny])
                    {
                        CreateWall(x, y, dir);
                    }
                }
            }
        }
    }

    private void CreateWall(int x, int y, Vector2Int dir)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Plane);

        wall.name = $"Wall_{x}_{y}";

        wall.transform.parent = wallParent;

        Vector3 basePos = GridToWorld(x, y);

        Vector3 offset = new Vector3(dir.x, 0, dir.y) * tileSize * 0.5f;

        wall.transform.position = basePos + offset + Vector3.up * (wallHeight * 0.5f);

        wall.transform.localScale = new Vector3(tileSize / 10f, wallHeight / 10f, 1);

        if (dir == Vector2Int.left)
        {
            wall.transform.rotation = Quaternion.Euler(90, 90, 0);
        }
        else if (dir == Vector2Int.right)
        {
            wall.transform.rotation = Quaternion.Euler(90, -90, 0);
        }
        else if (dir == Vector2Int.up)
        {
            wall.transform.rotation = Quaternion.Euler(90, 180, 0);
        }
        else
        {
            wall.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        if (wallMaterial != null)
        {
            wall.GetComponent<MeshRenderer>().material = wallMaterial;
        }
    }

    private void SpawnObjects()
    {
        if (spawnableObjects.Count == 0)
            return;

        foreach (Room room in rooms)
        {
            int count = Random.Range(objectsPerRoomMin, objectsPerRoomMax + 1);

            for (int i = 0; i < count; i++)
            {
                int x = Random.Range(room.x + 1, room.x + room.width - 1);
                int y = Random.Range(room.y + 1, room.y + room.height - 1);

                Vector3 pos = GridToWorld(x, y);

                GameObject prefab = spawnableObjects[Random.Range(0, spawnableObjects.Count)];

                if (prefab == null)
                    continue;

                GameObject obj = Instantiate(prefab);

                obj.transform.position = pos + Vector3.up * 0.5f;

                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                obj.transform.parent = objectParent;
            }
        }
    }

    private Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x * tileSize, 0, y * tileSize);
    }

    private void ClearDungeon()
    {
        GameObject old = GameObject.Find("Dungeon");

        if (old != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(old);
#else
            Destroy(old);
#endif
        }

        rooms.Clear();
    }

    [System.Serializable]
    private class Room
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public Room(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Vector2Int Center
        {
            get
            {
                return new Vector2Int(
                    x + width / 2,
                    y + height / 2
                );
            }
        }

        public bool Intersects(Room other, int padding = 0)
        {
            return x - padding < other.x + other.width &&
                   x + width + padding > other.x &&
                   y - padding < other.y + other.height &&
                   y + height + padding > other.y;
        }
    }
}