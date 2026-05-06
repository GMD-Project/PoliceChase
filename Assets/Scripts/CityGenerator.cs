using UnityEngine;
using System.Collections.Generic;
public class CityGenerator : MonoBehaviour
{
    [Header("City Size")]
    public int width  = 80;
    public int height = 70;
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

    List<Vector2Int> junctions = GenerateJunctions();

    foreach (var j in junctions)
        isRoad[j.x, j.y] = true;

    ConnectJunctions(isRoad, junctions);
    FixIsolatedAreas(isRoad);

    for (int x = 0; x < width; x++)
    {
        for (int z = 0; z < height; z++)
        {
            Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);
            if (isRoad[x, z])
                SpawnRoad(isRoad, x, z, position);
            else
                SpawnRandomBuilding(position);
        }
    }
}

List<Vector2Int> GenerateJunctions()
{
    List<Vector2Int> junctions = new List<Vector2Int>();
    int spacing = 4; // was 7

    for (int x = spacing / 2; x < width; x += spacing)
    {
        for (int z = spacing / 2; z < height; z += spacing)
        {
            int jx = Mathf.Clamp(x + Random.Range(-1, 2), 1, width - 2);
            int jz = Mathf.Clamp(z + Random.Range(-1, 2), 1, height - 2);
            junctions.Add(new Vector2Int(jx, jz));
        }
    }
    return junctions;
}

void ConnectJunctions(bool[,] isRoad, List<Vector2Int> junctions)
{
    for (int i = 0; i < junctions.Count; i++)
    {
        for (int j = i + 1; j < junctions.Count; j++)
        {
            // Connect all junction pairs that are close enough
            if (ManhattanDist(junctions[i], junctions[j]) <= 12)
                DrawLShapedRoad(isRoad, junctions[i], junctions[j]);
        }
    }
}

void DrawLShapedRoad(bool[,] isRoad, Vector2Int a, Vector2Int b)
{
    // Randomly pick which corner the L turns at, so routes vary
    if (Random.value < 0.5f)
    {
        DrawHorizontal(isRoad, a.x, b.x, a.y);
        DrawVertical(isRoad, a.y, b.y, b.x);
    }
    else
    {
        DrawVertical(isRoad, a.y, b.y, a.x);
        DrawHorizontal(isRoad, a.x, b.x, b.y);
    }
}

void DrawHorizontal(bool[,] isRoad, int x1, int x2, int z)
{
    for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        if (x >= 0 && x < width && z >= 0 && z < height)
            isRoad[x, z] = true;
}

void DrawVertical(bool[,] isRoad, int z1, int z2, int x)
{
    for (int z = Mathf.Min(z1, z2); z <= Mathf.Max(z1, z2); z++)
        if (x >= 0 && x < width && z >= 0 && z < height)
            isRoad[x, z] = true;
}

int ManhattanDist(Vector2Int a, Vector2Int b)
{
    return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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

            // If the island is too large, cut a road through its centre
            if (island.Count > 4)
            {
                Vector2Int centre = island[island.Count / 2];
                // Draw a road horizontally through the centre of the island
                DrawHorizontal(isRoad, centre.x - 2, centre.x + 2, centre.y);
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
