using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Points Parent")]
    public Transform spawnPointsParent;

    [Header("Cube Prefabs")]
    public GameObject cubePrefab;
    public GameObject toughCubePrefab;
    public GameObject superJumpCubePrefab;

    [Header("Spawn Chances")]
    [Range(0f, 1f)]
    public float toughCubeChance = 0.05f;
    [Range(0f, 1f)]
    public float superJumpCubeChance = 0.05f;

    [Header("Spawn Speed (Smooth Ramping)")]
    public float startSpawnInterval = 2f;
    public float minSpawnInterval = 0.3f;       // this will shrink over time
    public float rampRate = 1.5f;               // how fast interval lerps toward min

    [Header("Minimum Interval Shrink (Infinite Difficulty)")]
    public float minSpawnDecreaseRate = 0.01f;  // how fast minSpawnInterval shrinks
    public float absoluteMinLimit = 0.02f;      // never go below this

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
        // ⭐ Shrink the minimum interval over time (infinite difficulty)
        minSpawnInterval -= minSpawnDecreaseRate * Time.deltaTime;
        minSpawnInterval = Mathf.Max(minSpawnInterval, absoluteMinLimit);

        // ⭐ Smooth exponential ramping toward the *new* minimum
        currentSpawnInterval = Mathf.Lerp(
            currentSpawnInterval,
            minSpawnInterval,
            rampRate * Time.deltaTime
        );

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f;
            SpawnCube();
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

        if (superJumpCubePrefab != null && roll < superJumpCubeChance)
        {
            prefabToSpawn = superJumpCubePrefab;
        }
        else if (toughCubePrefab != null && roll < superJumpCubeChance + toughCubeChance)
        {
            prefabToSpawn = toughCubePrefab;
        }
        else
        {
            prefabToSpawn = cubePrefab;
        }

        Instantiate(prefabToSpawn, pos, Quaternion.identity);
    }
}
