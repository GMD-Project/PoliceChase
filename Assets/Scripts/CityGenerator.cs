using UnityEngine;

// Expected prefab orientations (all measured at Y-rotation 0):
//   straightPrefabs     – road runs North↔South
//   cornerPrefabs       – road opens to North and East (inner corner at SW)
//   tIntersectionPrefabs– road opens to North, South, East  (West side is closed)
//   crossroadPrefabs    – 4-way intersection, rotation-symmetric
//   deadEndPrefabs      – open end faces North (road enters from North, cap at South)
public class CityGenerator : MonoBehaviour
{
    [Header("City Size")]
    public int width  = 10;
    public int height = 10;
    public float tileSize = 10f;

    [Header("Road Prefabs by Type")]
    public GameObject[] straightPrefabs;
    public GameObject[] cornerPrefabs;
    public GameObject[] tIntersectionPrefabs;
    public GameObject[] crossroadPrefabs;
    public GameObject[] deadEndPrefabs;

    [Header("Building & Decoration")]
    public GameObject[] buildingPrefabs;
    public GameObject[] treePrefabs;
    public GameObject[] propPrefabs;

    [Header("Chances")]
    [Range(0f, 1f)] public float treeChance = 0.2f;
    [Range(0f, 1f)] public float propChance  = 0.15f;

    bool[,] roadGrid;

    void Start() => GenerateCity();

    void GenerateCity()
{
    bool[,] isRoad = new bool[width, height];

    // Pass 1: mark road cells
    for (int x = 0; x < width; x++)
        for (int z = 0; z < height; z++)
            isRoad[x, z] = x % 3 == 0 || z % 3 == 0;

    // Pass 2: spawn tiles
    for (int x = 0; x < width; x++)
    {
        for (int z = 0; z < height; z++)
        {
            Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);

            if (isRoad[x, z])
            {
                SpawnRoad(isRoad, x, z, position);
            }
            else
            {
                SpawnRandomBuilding(position);

                if (Random.value < treeChance)
                    SpawnRandom(treePrefabs, position + new Vector3(3, 0, 3), Quaternion.identity);

                if (Random.value < propChance)
                    SpawnRandom(propPrefabs, position + new Vector3(-3, 0, -3), Quaternion.identity);
            }
        }
    }
}

    void SpawnRoad(bool[,] isRoad, int x, int z, Vector3 position)
{
    bool n = z + 1 < height && isRoad[x, z + 1];
    bool s = z - 1 >= 0     && isRoad[x, z - 1];
    bool e = x + 1 < width  && isRoad[x + 1, z];
    bool w = x - 1 >= 0     && isRoad[x - 1, z];

    int connections = (n ? 1 : 0) + (s ? 1 : 0) + (e ? 1 : 0) + (w ? 1 : 0);

    GameObject[] prefabs;
    float yRot;

    switch (connections)
    {
        case 4:
            prefabs = crossroadPrefabs;
            yRot = 0f;
            break;

        case 3:
            prefabs = tIntersectionPrefabs;
            // rotate so the closed side faces the missing direction
            yRot = !s ? 0f : !w ? 90f : !n ? 180f : 270f;
            break;

        case 2 when (n && s) || (e && w):
            prefabs = straightPrefabs;
            // prefab is N-S by default; rotate 90 for E-W
            yRot = (n && s) ? 90f : 0f;
            break;

        case 2:
            prefabs = cornerPrefabs;
            // prefab opens N+E by default
            yRot = (n && e) ? 0f : (s && e) ? 90f : (s && w) ? 180f : 270f;
            break;

        default:
            // dead end or isolated — fall back to a straight
            prefabs = straightPrefabs;
            yRot = (e || w) ? 90f : 0f;
            break;
    }

    SpawnRandom(prefabs, position, Quaternion.Euler(0, yRot, 0));
}
    bool IsRoad(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height) return false;
        return roadGrid[x, z];
    }

    void SpawnRandomBuilding(Vector3 pos)
    {
        if (buildingPrefabs.Length == 0) return;
        Quaternion rot = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);
        Instantiate(buildingPrefabs[Random.Range(0, buildingPrefabs.Length)], pos, rot, transform);
    }

    void SpawnRandom(GameObject[] prefabs, Vector3 pos, Quaternion rot)
    {
        if (prefabs.Length == 0) return;
        Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos, rot, transform);
    }
}
