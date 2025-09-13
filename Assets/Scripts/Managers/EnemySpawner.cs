using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField, Tooltip("Time interval between enemy spawns")]
    private float spawnInterval = 5f;

    private Player.PlayerController player;
    private GameObject[] enemyPrefabs;
    private float timer = 0f;

    private readonly float spawnRangeMin = 8.0f; // Minimum distance from player
    private readonly float spawnRangeMax = 15.0f; // Maximum distance from player

    private void Start()
    {
        // Load all prefabs from Resources/Enemies at start
        enemyPrefabs = Resources.LoadAll<GameObject>("Enemies");
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            if (!playerObj.TryGetComponent<Player.PlayerController>(out player)) {
                Debug.LogError("Player GameObject does not have a PlayerController component.");
                enabled = false;
            }
        } else {
            Debug.LogError("No GameObject tagged 'Player' found. Please assign the player tag.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        Enemies.Controller.OnEnemyDeath += (attackType, points) => { SpawnRandomEnemy(); };
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval) {
            timer = 0f;
            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Length == 0) {
            Debug.LogWarning("No enemy prefabs found in Resources/Enemies!");
            return;
        }

        // Pick a random prefab
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[index];

        // Spawn at a random position outsisde the player's area
        Vector2 spawningRange;
        bool useNegative = Random.value < 0.5f; // Randomly decide if we want to use negative or positive range
        spawningRange = new Vector2(Random.Range(spawnRangeMin, spawnRangeMax), Random.Range(spawnRangeMin, spawnRangeMax)); // Set default range
                                                                                                                                // Adjust the spawning range based on the random choice
        if (useNegative) {
            spawningRange.x *= -1;
            spawningRange.y *= -1;
        }

        Vector3 spawnPosition = player.transform.position + (Vector3)spawningRange;

        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

        //Get the EnemyController component and set the player reference
        if (enemy.TryGetComponent<Enemies.Controller>(out var newEnemy)) {
            newEnemy.SetPlayerTransform(player.transform);
        }

    }
}
