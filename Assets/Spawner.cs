using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Points Parent")]
    public Transform spawnPointsParent;

    [Header("Settings")]
    public GameObject cubePrefab;        // normal cube (1 HP)
    public GameObject toughCubePrefab;   // tough cube (5 HP)
    public float spawnInterval = 1f;

    [Range(0f, 1f)]
    public float toughCubeChance = 0.05f; // 5% tough cubes

    [HideInInspector]
    public Transform[] spawnPoints;

    public GameObject superJumpCubePrefab;
[Range(0f, 1f)]
public float superJumpCubeChance = 0.05f; // 5% chance


    void OnValidate()
    {
        if (spawnPointsParent == null) return;

        Transform[] all = spawnPointsParent.GetComponentsInChildren<Transform>();
        if (all.Length <= 1) return;

        spawnPoints = new Transform[all.Length - 1];
        int index = 0;

        foreach (Transform t in all)
        {
            if (t != spawnPointsParent)
                spawnPoints[index++] = t;
        }
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    System.Collections.IEnumerator SpawnLoop()
{
    while (true)
    {
        yield return new WaitForSeconds(spawnInterval);

        if (spawnPoints == null || spawnPoints.Length == 0)
            continue;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = point.position;
        pos.y -= 0.1f;

        // Decide which cube to spawn
        GameObject prefabToSpawn;

        float roll = Random.value;

        // Super jump cube first
        if (superJumpCubePrefab != null && roll < superJumpCubeChance)
        {
            prefabToSpawn = superJumpCubePrefab;
        }
        // Tough cube second
        else if (toughCubePrefab != null && roll < superJumpCubeChance + toughCubeChance)
        {
            prefabToSpawn = toughCubePrefab;
        }
        // Normal cube fallback
        else
        {
            prefabToSpawn = cubePrefab;
        }

        Instantiate(prefabToSpawn, pos, Quaternion.identity);
    }
}

}
