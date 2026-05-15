using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Points Parent")]
    public Transform spawnPointsParent;

    [Header("Cube Prefabs")]
    public GameObject cubePrefab;        // normal cube (1 HP)
    public GameObject toughCubePrefab;   // tough cube (5 HP)
    public GameObject superJumpCubePrefab;

    [Header("Spawn Chances")]
    [Range(0f, 1f)]
    public float toughCubeChance = 0.05f; // 5%
    [Range(0f, 1f)]
    public float superJumpCubeChance = 0.05f; // 5%

    [Header("Spawn Speed (Ramping)")]
    public float startSpawnInterval = 2f;     // slow at start
    public float minSpawnInterval = 0.3f;     // fastest allowed
    public float rampRate = 0.05f;            // how fast it speeds up

    private float currentSpawnInterval;
    private float spawnTimer = 0f;

    [HideInInspector]
    public Transform[] spawnPoints;

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
        currentSpawnInterval = startSpawnInterval;
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f;
            SpawnCube();

            // ⭐ RAMP UP SPAWN SPEED
            currentSpawnInterval -= rampRate;
            currentSpawnInterval = Mathf.Clamp(currentSpawnInterval, minSpawnInterval, startSpawnInterval);
        }
    }

    void SpawnCube()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = point.position;
        pos.y -= 0.1f;

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
