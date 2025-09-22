// using System;
// using System.Collections.Generic;

using System.Collections;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("Time interval between enemy spawns")]
        private float spawnInterval = 5f;

        [SerializeField, Tooltip("Maximum number of enemies allowed in the scene at once")]
        private int maxEnemies = 100;

        [SerializeField, Tooltip("Default initial pool size for minions")]
        private int initialDefaultPoolSize = 10;

        [SerializeField, Tooltip("Default maximum pool size for minions")]
        private int maxDefaultPoolSize = 40;

        [HideInInspector]
        public EnemySpawner Instance;

        private Player.PlayerController player;
        //private float timer;
        private int currentLevel = 1;

        private const float SpawnRangeMin = 10.0f; // Minimum distance from player
        private const float SpawnRangeMax = 20.0f; // Maximum distance from player

        private EnemySpawnChances enemySpawnChances;
        private PooledMinionManager pooledMinions;


        private int spawnCount;

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                if (!playerObj.TryGetComponent(out player)) {
                    Debug.LogError("Player GameObject does not have a PlayerController component.");
                    enabled = false;
                    return;
                }
            } else {
                Debug.LogError("No GameObject tagged 'Player' found. Please assign the player tag.");
                enabled = false;
                return;
            }

            enemySpawnChances = Resources.Load<EnemySpawnChances>("ScriptableObjects/EnemySpawnChances");
            if (enemySpawnChances == null) {
                Debug.LogError("EnemySpawnChances ScriptableObject not found in Resources/ScriptableObjects!");
                enabled = false;
                return;
            }

            // Initialize the pooled minion manager
            pooledMinions = new PooledMinionManager(initialDefaultPoolSize);
            var allEnemyPrefabs = enemySpawnChances.GetAllEnemyPrefabsWithCount();
            foreach (var enemy in allEnemyPrefabs) {
                var initialPoolSize = initialDefaultPoolSize;
                var prefabMaxCount = enemy.Value == 0 ? maxDefaultPoolSize : enemy.Value;
                if (initialPoolSize > prefabMaxCount) {
                    initialPoolSize = prefabMaxCount / 2;
                }

                pooledMinions.PreWarm(enemy.Key, initialPoolSize, prefabMaxCount);
            }

            StartCoroutine(MinionSpawnerTimer(spawnInterval));
        }

        private void OnEnable()
        {
            if (RunScoreManager.Instance == null) {
                Debug.LogError(
                    "RunScoreManager instance is null. Ensure it is initialized before enabling EnemySpawner.");
                enabled = false;
                return;
            }

            Enemies.Controller.OnEnemyDeath += HandleEnemyDeath;
            RunScoreManager.Instance.OnPlayerLeveledUp += HandlePlayerLevelUp;
        }

        private void OnDisable()
        {
            if (RunScoreManager.Instance == null) {
                return;
            }

            RunScoreManager.Instance.OnPlayerLeveledUp -= HandlePlayerLevelUp;
            Enemies.Controller.OnEnemyDeath -= HandleEnemyDeath;
        }
        
        
        private void HandleEnemyDeath(AttackType _attackType, int _points, float _itemChance, Vector2 _position,
            GameObject _enemy)
        {
            spawnCount--;
            if (_attackType == AttackType.PlayerAttack) {
                SpawnRandomEnemy();
            }

            pooledMinions.Release(_enemy);
        }

        private void HandlePlayerLevelUp(int _newLevel)
        {
            spawnInterval = Mathf.Max(0.25f, spawnInterval - 0.5f * (_newLevel - 1));
            currentLevel = _newLevel;
        }

        private void SpawnRandomEnemy()
        {
            if (spawnCount >= maxEnemies)
                return;
            // Pick a random prefab
            GameObject prefab = null;
            int loopCount = 0;

            while (!prefab && loopCount < 5) {
                loopCount++;
                prefab = enemySpawnChances.GetRandomEnemy(GameManager.Instance.DifficultyLevel, currentLevel);
            }


            // Spawn at a random position outside the player's area
            bool useNegative = Random.value < 0.5f; // Randomly decide if we want to use negative or positive range
            Vector2 spawningRange = new(Random.Range(SpawnRangeMin, SpawnRangeMax),
                Random.Range(SpawnRangeMin, SpawnRangeMax));

            if (useNegative) {
                spawningRange.x *= -1;
                spawningRange.y *= -1;
            }

            Vector3 spawnPosition = player.transform.position + (Vector3)spawningRange;

            spawnCount++;
            var enemy = pooledMinions.GetFromPool(prefab, spawnPosition, Quaternion.identity);

            //Get the EnemyController component and set the player reference
            if (enemy && enemy.TryGetComponent<Enemies.Controller>(out var newEnemy)) {
                newEnemy.SetPlayerTransform(player.transform);
                newEnemy.OnTakenFromPool(prefab);
            }
        }

        private IEnumerator MinionSpawnerTimer(float _interval)
        {
            var wait = new WaitForSeconds(_interval);
            while (enabled) {
                if (GameManager.Instance && !GameManager.Instance.IsPaused) {
                    if (maxEnemies == 0 || spawnCount < maxEnemies) {
                        SpawnRandomEnemy();
                    }
                }
                yield return wait;
            }
        }
    }
}