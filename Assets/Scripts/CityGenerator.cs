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
        // Pass 1 – mark which tiles are roads
        roadGrid = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                roadGrid[x, z] = x % 3 == 0 || z % 3 == 0;

        // Pass 2 – spawn tiles with contextual prefab + rotation
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, z * tileSize);

                if (roadGrid[x, z])
                {
                    SpawnRoad(x, z, pos);
                }
                else
                {
                    SpawnRandomBuilding(pos);

                    if (Random.value < treeChance)
                        SpawnRandom(treePrefabs, pos + new Vector3(3, 0, 3), Quaternion.identity);

                    if (Random.value < propChance)
                        SpawnRandom(propPrefabs, pos + new Vector3(-3, 0, -3), Quaternion.identity);
                }
            }
        }
    }

    void SpawnRoad(int x, int z, Vector3 pos)
    {
        bool n = IsRoad(x,     z + 1);
        bool s = IsRoad(x,     z - 1);
        bool e = IsRoad(x + 1, z    );
        bool w = IsRoad(x - 1, z    );

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
                // Rotate so the closed side matches the missing direction.
                // Default prefab has West closed, so closed rotates: W→N→E→S per 90° CW step.
                prefabs = tIntersectionPrefabs;
                yRot = !w ? 0f : !n ? 90f : !e ? 180f : 270f;
                break;

            case 2:
                if ((n && s) || (e && w))
                {
                    // Straight road – default runs N↔S, rotate 90° for E↔W
                    prefabs = straightPrefabs;
                    yRot = (e && w) ? 90f : 0f;
                }
                else
                {
                    // Corner – default opens N+E; each 90° CW step cycles to E+S, S+W, W+N
                    prefabs = cornerPrefabs;
                    yRot = (n && e) ? 0f : (s && e) ? 90f : (s && w) ? 180f : 270f;
                }
                break;

            case 1:
                // Dead end – default opening faces North; rotate to match actual open direction
                prefabs = deadEndPrefabs;
                yRot = n ? 0f : e ? 90f : s ? 180f : 270f;
                break;

            default:
                // Isolated tile – fall back to a straight piece
                prefabs = straightPrefabs;
                yRot = 0f;
                break;
        }

        SpawnRandom(prefabs, pos, Quaternion.Euler(0, yRot, 0));
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
