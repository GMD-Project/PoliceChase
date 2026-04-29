using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    [Header("City Size")]
    public int width = 10;
    public int height = 10;
    public float tileSize = 10f;

    [Header("Prefabs")]
    public GameObject[] buildingPrefabs;
    public GameObject[] roadPrefabs;
    public GameObject[] treePrefabs;
    public GameObject[] propPrefabs;

    [Header("Chances")]
    [Range(0f, 1f)] public float treeChance = 0.2f;
    [Range(0f, 1f)] public float propChance = 0.15f;
void Start()
    {
        GenerateCity();
    }

    void GenerateCity()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);

                bool isRoad = x % 3 == 0 || z % 3 == 0;

                if (isRoad)
                {
                    SpawnRandom(roadPrefabs, position, Quaternion.identity);
                }
                else
                {
                    SpawnRandomBuilding(position);

                    if (Random.value < treeChance)
                    {
                        Vector3 treePos = position + new Vector3(3, 0, 3);
                        SpawnRandom(treePrefabs, treePos, Quaternion.identity);
                    }

                    if (Random.value < propChance)
                    {
                        Vector3 propPos = position + new Vector3(-3, 0, -3);
                        SpawnRandom(propPrefabs, propPos, Quaternion.identity);
                    }
                }
            }
        }
    }

    void SpawnRandomBuilding(Vector3 position)
    {
        if (buildingPrefabs.Length == 0) return;

        GameObject building = buildingPrefabs[
            Random.Range(0, buildingPrefabs.Length)
        ];

        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);

        Instantiate(building, position, rotation, transform);
    }

    void SpawnRandom(GameObject[] prefabs, Vector3 position, Quaternion rotation)
    {
        if (prefabs.Length == 0) return;

        GameObject prefab = prefabs[
            Random.Range(0, prefabs.Length)
        ];

        Instantiate(prefab, position, rotation, transform);
    }
}
