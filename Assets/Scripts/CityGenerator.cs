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
    GenerateMaze();
    for (int x = 0; x < width; x++)
        for (int z = 0; z < height; z++)
        {
            Vector3 pos = new Vector3(x * tileSize, 0, z * tileSize);
            if (roadGrid[x, z]) SpawnRoad(roadGrid, x, z, pos);
            else SpawnRandomBuilding(pos);
        }
      
}

   List<int> GenerateRoadLines(int size)
{
    List<int> roads = new List<int>();
    int pos = 0;
    while (pos < size)
    {
        roads.Add(pos);
        pos += Random.Range(7, 13);
    }

    if (size - 1 - roads[roads.Count - 1] > 1)
        roads.Add(size - 1);

    return roads;
}

void EnsureRoadAccess(bool[,] isRoad)
{
    bool changed = true;
    while (changed)
    {
        changed = false;
        bool[,] visited = new bool[width, height];

        for (int startX = 0; startX < width; startX++)
        {
            for (int startZ = 0; startZ < height; startZ++)
            {
                if (isRoad[startX, startZ] || visited[startX, startZ]) continue;

                List<Vector2Int> cluster = new List<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                queue.Enqueue(new Vector2Int(startX, startZ));
                visited[startX, startZ] = true;

                while (queue.Count > 0)
                {
                    Vector2Int cell = queue.Dequeue();
                    cluster.Add(cell);
                    foreach (var dir in new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down })
                    {
                        Vector2Int next = cell + dir;
                        if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                         continue;
                        if (visited[next.x, next.y] || isRoad[next.x, next.y]) 
                        continue;
                        visited[next.x, next.y] = true;
                        queue.Enqueue(next);
                    }
                }

                bool needsFix = false;
                foreach (var t in cluster)
                    if (!HasRoadNeighbour(isRoad, t.x, t.y)) { needsFix = true; break; }

                if (!needsFix) continue;

                
                int minX = int.MaxValue, maxX = int.MinValue;
                int minZ = int.MaxValue, maxZ = int.MinValue;
                foreach (var t in cluster)
                {
                    if (t.x < minX) minX = t.x;
                    if (t.x > maxX) maxX = t.x;
                    if (t.y < minZ) minZ = t.y;
                    if (t.y > maxZ) maxZ = t.y;
                }

                int centerX = (minX + maxX) / 2;
                int centerZ = (minZ + maxZ) / 2;

               
                for (int x = minX; x <= maxX; x++)
                    if (!isRoad[x, centerZ]) { isRoad[x, centerZ] = true; changed = true; }

                if (maxZ - minZ > 2)
                    for (int z = minZ; z <= maxZ; z++)
                        if (!isRoad[centerX, z]) { isRoad[centerX, z] = true; changed = true; }
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
    int count = (width + height) / 16;
    for (int i = 0; i < count; i++)
    {
        int x1 = Random.Range(1, width - 1);
        int z1 = Random.Range(1, height - 1);

        if (isRoad[x1, z1]) continue;

        int x2 = Mathf.Clamp(x1 + Random.Range(-3, 4), 1, width - 2);
        int z2 = Mathf.Clamp(z1 + Random.Range(-3, 4), 1, height - 2);

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
        void GenerateMaze()
    {
        roadGrid = new bool[width, height];

        int cellCols = width / 2;
        int cellRows = height / 2;
        bool[,] visited = new bool[cellCols, cellRows];

        System.Collections.Generic.Stack<Vector2Int> stack = new System.Collections.Generic.Stack<Vector2Int>();
        Vector2Int start = new Vector2Int(0, 0);
        visited[start.x, start.y] = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();

            System.Collections.Generic.List<Vector2Int> neighbours = new System.Collections.Generic.List<Vector2Int>();
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                Vector2Int neighbour = current + dir;
                if (neighbour.x >= 0 && neighbour.x < cellCols &&
                    neighbour.y >= 0 && neighbour.y < cellRows &&
                    !visited[neighbour.x, neighbour.y])
                {
                    neighbours.Add(neighbour);
                }
            }

            if (neighbours.Count > 0)
            {
                Vector2Int next = neighbours[Random.Range(0, neighbours.Count)];
                visited[next.x, next.y] = true;

                int wallX = current.x * 2 + 1 + (next.x - current.x);
                int wallZ = current.y * 2 + 1 + (next.y - current.y);
                roadGrid[wallX, wallZ] = true;

                roadGrid[next.x * 2 + 1, next.y * 2 + 1] = true;

                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }

        roadGrid[1, 1] = true;

        roadGrid[1, 0] = true;
    }

    

}
