using UnityEngine;
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
    [Header("Road Props")]
    public GameObject[] trafficLightPrefabs;
    public GameObject[] signPrefabs;
[   Range(0f, 1f)] public float signChance = 0.3f;

    [Header("Chances")]
    [Range(0f, 1f)] public float treeChance = 0.2f;
    [Range(0f, 1f)] public float propChance  = 0.15f;

    bool[,] roadGrid;

    void Start() => GenerateCity();

    void GenerateCity()
{
    bool[,] isRoad = new bool[width, height];

  
    for (int x = 0; x < width; x++)
        for (int z = 0; z < height; z++)
            isRoad[x, z] = x % 3 == 0 || z % 3 == 0;

   
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
            yRot = (e || w) ? 90f : 0f;
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
