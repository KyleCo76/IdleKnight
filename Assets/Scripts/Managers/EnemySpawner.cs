using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField, Tooltip("Time interval between enemy spawns")]
    private float spawnInterval = 5f;

    private Player.PlayerController player;
    private List<GameObject> enemyPrefabs;
    private float timer = 0f;

    private readonly float spawnRangeMin = 10.0f; // Minimum distance from player
    private readonly float spawnRangeMax = 20.0f; // Maximum distance from player

    private Transform enemyParent;

    private void Awake()
    {
        // Load all prefabs from Resources/Enemies at start
        enemyPrefabs = new List<GameObject>(Resources.LoadAll<GameObject>("Enemies/Level1"));
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

        var parentObject = GameObject.Find("Enemies");
        if (parentObject == null || !parentObject.TryGetComponent<Transform>(out enemyParent)) {
            enemyParent = new GameObject("Enemies").transform;
        }
    }

    private void OnEnable()
    {
        Enemies.Controller.OnEnemyDeath += (attackType, points, itemChance, position) => { if (attackType == Game.AttackType.PlayerAttack) SpawnRandomEnemy(); };
        RunScoreManager.Instance.OnPlayerLeveledUp += HandlePlayerLevelUp;
    }

    private void OnDisable()
    {
        RunScoreManager.Instance.OnPlayerLeveledUp -= HandlePlayerLevelUp;
        Enemies.Controller.OnEnemyDeath -= (attackType, points, itemChance, position) => { if (attackType == Game.AttackType.PlayerAttack) SpawnRandomEnemy(); };
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval) {
            timer = 0f;
            SpawnRandomEnemy();
        }
    }

    private void HandlePlayerLevelUp(int _newLevel)
    {
        spawnInterval = Mathf.Max(0.25f, spawnInterval - 0.5f * (_newLevel - 1));
        string path = $"Enemies/Level{_newLevel}";
        var newPrefabs = Resources.LoadAll<GameObject>(path);
        if (newPrefabs.Length == 0) {
            Debug.LogWarning($"No enemy prefabs found in Resources/{path}! Continuing to use previous level's enemies.");
            return;
        }
        enemyPrefabs.AddRange(newPrefabs);
    }

    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Count == 0) {
            Debug.LogWarning("No enemy prefabs found in Resources/Enemies!");
            return;
        }

        // Pick a random prefab
        int index = Random.Range(0, enemyPrefabs.Count);
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

        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity, enemyParent);

        //Get the EnemyController component and set the player reference
        if (enemy.TryGetComponent<Enemies.Controller>(out var newEnemy)) {
            newEnemy.SetPlayerTransform(player.transform);
        }

    }
}
