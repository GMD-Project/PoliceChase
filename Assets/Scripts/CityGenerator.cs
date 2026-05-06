using UnityEngine;
using System.Collections.Generic;
public class CityGenerator : MonoBehaviour
{
    [Header("City Size")]
    public int width  = 200;
    public int height = 500;
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
    [Header("Road Props")]
    public GameObject[] trafficLightPrefabs;
    public GameObject[] signPrefabs;
    [Range(0f, 1f)] public float signChance = 0.3f;

    [Header("Chances")]
    [Range(0f, 1f)] public float treeChance = 0.2f;
    [Range(0f, 1f)] public float propChance  = 0.15f;

    bool[,] roadGrid;

    void Start() => GenerateCity();

void GenerateCity()
{
    bool[,] isRoad = new bool[width, height];

    List<int> colRoads = GenerateRoadLines(width);
    List<int> rowRoads = GenerateRoadLines(height);

    // Mark all intersection points
foreach (int col in colRoads)
    foreach (int row in rowRoads)
        isRoad[col, row] = true;

// Draw horizontal segments between intersections, randomly skipping some
foreach (int row in rowRoads)
{
    for (int i = 0; i < colRoads.Count - 1; i++)
    {
        if (Random.value < 0.2f) continue; // 20% chance to leave a gap
        for (int x = colRoads[i]; x <= colRoads[i + 1]; x++)
            isRoad[x, row] = true;
    }
}

// Draw vertical segments between intersections, randomly skipping some
foreach (int col in colRoads)
{
    for (int j = 0; j < rowRoads.Count - 1; j++)
    {
        if (Random.value < 0.2f) continue;
        for (int z = rowRoads[j]; z <= rowRoads[j + 1]; z++)
            isRoad[col, z] = true;
    }
}

    AddOrganicConnectors(isRoad);
    FixIsolatedAreas(isRoad);
    EnsureRoadAccess(isRoad); 

    for (int x = 0; x < width; x++)
        for (int z = 0; z < height; z++)
        {
            Vector3 pos = new Vector3(x * tileSize, 0, z * tileSize);
            if (isRoad[x, z]) SpawnRoad(isRoad, x, z, pos);
            else SpawnRandomBuilding(pos);
        }
        Debug.Log($"Generating city: {width} x {height}");
}

   List<int> GenerateRoadLines(int size)
{
    List<int> roads = new List<int>();
    int pos = 0;
    while (pos < size)
    {
        roads.Add(pos);
        pos += Random.Range(5, 10);
    }

    // Only add the far edge if it won't sit directly next to the last road
    if (size - 1 - roads[roads.Count - 1] > 1)
        roads.Add(size - 1);

    return roads;
}

void EnsureRoadAccess(bool[,] isRoad)
{
    for (int x = 0; x < width; x++)
    {
        for (int z = 0; z < height; z++)
        {
            if (isRoad[x, z]) continue;
            if (HasRoadNeighbour(isRoad, x, z)) continue;

            // Find the nearest road tile
            int nearestX = -1, nearestZ = -1, bestDist = int.MaxValue;
            for (int rx = 0; rx < width; rx++)
                for (int rz = 0; rz < height; rz++)
                    if (isRoad[rx, rz])
                    {
                        int d = Mathf.Abs(rx - x) + Mathf.Abs(rz - z);
                        if (d < bestDist) { bestDist = d; nearestX = rx; nearestZ = rz; }
                    }

            if (nearestX < 0) continue;

            // Walk from this tile toward the nearest road, marking each step
            int cx = x, cz = z;
            while (cx != nearestX || cz != nearestZ)
            {
                isRoad[cx, cz] = true;
                if (cx != nearestX) cx += nearestX > cx ? 1 : -1;
                else                cz += nearestZ > cz ? 1 : -1;
            }
        }
    }
}

bool HasRoadNeighbour(bool[,] isRoad, int x, int z)
{
    return (x > 0        && isRoad[x - 1, z]) ||
           (x < width-1  && isRoad[x + 1, z]) ||
           (z > 0        && isRoad[x, z - 1]) ||
           (z < height-1 && isRoad[x, z + 1]);
}

    void AddOrganicConnectors(bool[,] isRoad)
{
    int count = (width + height) / 12; // was /6
    for (int i = 0; i < count; i++)
    {
        int x1 = Random.Range(1, width - 1);
        int z1 = Random.Range(1, height - 1);
        int x2 = Mathf.Clamp(x1 + Random.Range(-4, 5), 1, width - 2);
        int z2 = Mathf.Clamp(z1 + Random.Range(-4, 5), 1, height - 2);

        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            isRoad[x, z1] = true;
        for (int z = Mathf.Min(z1, z2); z <= Mathf.Max(z1, z2); z++)
            isRoad[x2, z] = true;
    }
}

void FixIsolatedAreas(bool[,] isRoad)
{
    bool[,] visited = new bool[width, height];

    for (int startX = 0; startX < width; startX++)
    {
        for (int startZ = 0; startZ < height; startZ++)
        {
            if (isRoad[startX, startZ] || visited[startX, startZ]) continue;

            List<Vector2Int> island = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startZ));
            visited[startX, startZ] = true;

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                island.Add(cell);

                foreach (var dir in new[] {
                    Vector2Int.up, Vector2Int.down,
                    Vector2Int.left, Vector2Int.right })
                {
                    Vector2Int next = cell + dir;
                    if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height) continue;
                    if (visited[next.x, next.y] || isRoad[next.x, next.y]) continue;
                    visited[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }

            if (island.Count > 10)
            {
                Vector2Int centre = island[island.Count / 2];
                for (int x = Mathf.Max(0, centre.x - 2); x <= Mathf.Min(width - 1, centre.x + 2); x++)
                    isRoad[x, centre.y] = true;
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
            yRot = !s ? 0f : !w ? 90f : !n ? 180f : 270f;
            break;

        case 2 when (n && s) || (e && w):
            prefabs = straightPrefabs;
            yRot = (n && s) ? 90f : 0f;
            break;

        case 2:
            prefabs = cornerPrefabs;
            yRot = (n && e) ? 0f : (s && e) ? 90f : (s && w) ? 180f : 270f;
            break;

        default:
            prefabs = straightPrefabs;
            yRot = (n || s) ? 90f : 0f; 
            break;
    }

    SpawnRandom(prefabs, position, Quaternion.Euler(0, yRot, 0));
    float edge = tileSize * 0.4f;

    if (connections == 4)
    {
        SpawnProp(trafficLightPrefabs, position + new Vector3( edge, 0,  edge), Quaternion.Euler(0,0,0));
        SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0,  edge), Quaternion.Euler(0,0,0));
        SpawnProp(trafficLightPrefabs, position + new Vector3( edge, 0, -edge), Quaternion.Euler(0,0,0));
        SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, -edge), Quaternion.Euler(0,0,0));
    }
    else if (connections == 3)
    {
        // traffic lights only on the two corners of the closed side
        if      (!w) { SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0,  edge), Quaternion.Euler(0,0,0)); SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, -edge), Quaternion.Euler(0,0,0)); }
        else if (!e) { SpawnProp(trafficLightPrefabs, position + new Vector3( edge, 0,  edge), Quaternion.Euler(0,0,0)); SpawnProp(trafficLightPrefabs, position + new Vector3( edge, 0, -edge), Quaternion.Euler(0,0,0)); }
        else if (!n) { SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0,  edge), Quaternion.Euler(0,0,0)); SpawnProp(trafficLightPrefabs, position + new Vector3( edge, 0,  edge), Quaternion.Euler(0,0,0)); }
        else         { SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, -edge), Quaternion.Euler(0,0,0)); SpawnProp(trafficLightPrefabs, position + new Vector3( edge, 0, -edge), Quaternion.Euler(0,0,0)); }
    }
    else if (connections == 2 && (n && s || e && w) && Random.value < signChance)
    {
        if(n&&s)
        {
         bool eastSide = Random.value < 0.5f;
         Vector3 sideOffset = new Vector3(eastSide?edge : -edge, 0, 0);
          float yRotation = eastSide ? 180f : 0f;
          SpawnProp(signPrefabs, position + sideOffset, Quaternion.Euler(0, yRotation, 0));
        }
       else
       {
        bool northSide = Random.value < 0.5f;
         Vector3 sideOffset = new Vector3(0, 0, northSide?edge : -edge);
          float yRotation = northSide ? 270f : 90f;
          SpawnProp(signPrefabs, position + sideOffset, Quaternion.Euler(0, yRotation, 0));
       }
        
    }
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
    void SpawnProp(GameObject[] prefabs, Vector3 position, Quaternion rotation)
    {
        if (prefabs.Length == 0) return;
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        Instantiate(prefab, position, rotation, transform);
    }

    

}
