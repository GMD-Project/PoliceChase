using UnityEngine;
using System.Collections.Generic;
public class CityGenerator : MonoBehaviour
{
    [Header("City Size")]
    public int width = 200;
    public int height = 500;
    public float tileSize = 10f;
    [Header("Player")]
    public GameObject playerCarPrefab;
    public GameObject player2Prefab;

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
    [Range(0f, 1f)] public float propChance = 0.15f;


    bool[,] roadGrid;
    Vector2Int exitTile;
    private Vector2Int _player1Tile;
    private Vector2Int _player1ForwardDir;


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
        SpawnPlayer();
    }

    void SpawnRoad(bool[,] isRoad, int x, int z, Vector3 position)
    {
        bool n = z + 1 < height && isRoad[x, z + 1];
        bool s = z - 1 >= 0 && isRoad[x, z - 1];
        bool e = x + 1 < width && isRoad[x + 1, z];
        bool w = x - 1 >= 0 && isRoad[x - 1, z];

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
            SpawnProp(trafficLightPrefabs, position + new Vector3(edge, 0, edge), Quaternion.Euler(0, 0, 0));
            SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, edge), Quaternion.Euler(0, 0, 0));
            SpawnProp(trafficLightPrefabs, position + new Vector3(edge, 0, -edge), Quaternion.Euler(0, 0, 0));
            SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, -edge), Quaternion.Euler(0, 0, 0));
        }
        else if (connections == 3)
        {
            if (!w) { SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, edge), Quaternion.Euler(0, 0, 0)); SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, -edge), Quaternion.Euler(0, 0, 0)); }
            else if (!e) { SpawnProp(trafficLightPrefabs, position + new Vector3(edge, 0, edge), Quaternion.Euler(0, 0, 0)); SpawnProp(trafficLightPrefabs, position + new Vector3(edge, 0, -edge), Quaternion.Euler(0, 0, 0)); }
            else if (!n) { SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, edge), Quaternion.Euler(0, 0, 0)); SpawnProp(trafficLightPrefabs, position + new Vector3(edge, 0, edge), Quaternion.Euler(0, 0, 0)); }
            else { SpawnProp(trafficLightPrefabs, position + new Vector3(-edge, 0, -edge), Quaternion.Euler(0, 0, 0)); SpawnProp(trafficLightPrefabs, position + new Vector3(edge, 0, -edge), Quaternion.Euler(0, 0, 0)); }
        }
        else if (connections == 2 && (n && s || e && w) && Random.value < signChance)
        {
            if (n && s)
            {
                bool eastSide = Random.value < 0.5f;
                Vector3 sideOffset = new Vector3(eastSide ? edge : -edge, 0, 0);
                float yRotation = eastSide ? 180f : 0f;
                SpawnProp(signPrefabs, position + sideOffset, Quaternion.Euler(0, yRotation, 0));
            }
            else
            {
                bool northSide = Random.value < 0.5f;
                Vector3 sideOffset = new Vector3(0, 0, northSide ? edge : -edge);
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

        // Close all borders
        for (int x = 0; x < width; x++)
        {
            roadGrid[x, 0] = false;
            roadGrid[x, height - 1] = false;
        }
        for (int z = 0; z < height; z++)
        {
            roadGrid[0, z] = false;
            roadGrid[width - 1, z] = false;
        }

        // Collect valid exit candidates (border tiles touching an interior road)
        List<Vector2Int> exits = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
        {
            if (roadGrid[x, 1]) exits.Add(new Vector2Int(x, 0));
            if (roadGrid[x, height - 2]) exits.Add(new Vector2Int(x, height - 1));
        }
        for (int z = 1; z < height - 1; z++)
        {
            if (roadGrid[1, z]) exits.Add(new Vector2Int(0, z));
            if (roadGrid[width - 2, z]) exits.Add(new Vector2Int(width - 1, z));
        }

        // Open one random exit
        if (exits.Count > 0)
        {
            Vector2Int exit = exits[Random.Range(0, exits.Count)];
            roadGrid[exit.x, exit.y] = true;
            exitTile = exit;
        }

    }
    void SpawnPlayer()
    {
        if (playerCarPrefab == null) return;

        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int z = 1; z < height - 1; z++)
            {
                if (!roadGrid[x, z]) continue;

                int neighbours = 0;
                if (roadGrid[x + 1, z]) neighbours++;
                if (roadGrid[x - 1, z]) neighbours++;
                if (roadGrid[x, z + 1]) neighbours++;
                if (roadGrid[x, z - 1]) neighbours++;

                if (neighbours >= 2)
                    candidates.Add(new Vector2Int(x, z));
            }
        }

        if (candidates.Count == 0) return;

        Vector2Int tile = candidates[Random.Range(0, candidates.Count)];

        Vector3 spawnPos = transform.position + new Vector3(
            tile.x * tileSize,
            0.2f,
            tile.y * tileSize
        );

        Quaternion spawnRot = Quaternion.identity;

        _player1Tile = tile;
        _player1Tile = tile;
        if (roadGrid[tile.x, tile.y + 1])      { spawnRot = Quaternion.Euler(0f,   0f, 0f); _player1ForwardDir = new Vector2Int( 0,  1); }
        else if (roadGrid[tile.x + 1, tile.y]) { spawnRot = Quaternion.Euler(0f,  90f, 0f); _player1ForwardDir = new Vector2Int( 1,  0); }
        else if (roadGrid[tile.x, tile.y - 1]) { spawnRot = Quaternion.Euler(0f, 180f, 0f); _player1ForwardDir = new Vector2Int( 0, -1); }
        else if (roadGrid[tile.x - 1, tile.y]) { spawnRot = Quaternion.Euler(0f, 270f, 0f); _player1ForwardDir = new Vector2Int(-1,  0); }



        GameObject player = Instantiate(playerCarPrefab, spawnPos, spawnRot);
        player.name = "PlayerCar";

        TopDownCamera cam = Camera.main.GetComponent<TopDownCamera>();
        if (cam != null) cam.target = player.transform;

        SpawnPlayer2();


    }

        void SpawnPlayer2()
    {
        if (player2Prefab == null) return;

        Vector2Int tile = _player1Tile;
        Vector2Int dir = -_player1ForwardDir;
        Vector2Int cameFrom = _player1Tile + _player1ForwardDir;

        for (int i = 0; i < 9; i++)
        {
            Vector2Int next = tile + dir;
            if (IsRoad(next.x, next.y))
            {
                cameFrom = tile;
                tile = next;
            }
            else
            {
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                bool moved = false;
                foreach (var d in dirs)
                {
                    Vector2Int candidate = tile + d;
                    if (candidate != cameFrom && IsRoad(candidate.x, candidate.y))
                    {
                        dir = d;
                        cameFrom = tile;
                        tile = candidate;
                        moved = true;
                        break;
                    }
                }
                if (!moved) break;
            }
        }

        Vector3 spawnPos = transform.position + new Vector3(tile.x * tileSize, 0.2f, tile.y * tileSize);

        Vector2Int facing = -dir;
        Quaternion spawnRot;
        if      (facing == new Vector2Int(0,  1)) spawnRot = Quaternion.Euler(0f,   0f, 0f);
        else if (facing == new Vector2Int(1,  0)) spawnRot = Quaternion.Euler(0f,  90f, 0f);
        else if (facing == new Vector2Int(0, -1)) spawnRot = Quaternion.Euler(0f, 180f, 0f);
        else                                      spawnRot = Quaternion.Euler(0f, 270f, 0f);

        GameObject player2 = Instantiate(player2Prefab, spawnPos, spawnRot);
        player2.name = "Player2Car";
    }
}
