// using System;
// using System.Collections.Generic;

using System.Collections;
using Game;
using ScriptableObjects;
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
        private int currentLevel = 1;
        private bool shouldSpawn;
        private float currentSpawnTime;

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
            GameSceneManager.Instance.OnSceneLoaded += StartupComponents;
        }

        private void OnDisable()
        {
            if (RunScoreManager.Instance == null) {
                return;
            }

            RunScoreManager.Instance.OnPlayerLeveledUp -= HandlePlayerLevelUp;
            Enemies.Controller.OnEnemyDeath -= HandleEnemyDeath;
            GameSceneManager.Instance.OnSceneLoaded -= StartupComponents;
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
            currentSpawnTime = Mathf.Max(0.1f, spawnInterval - 0.1f * (_newLevel - 1));
            currentLevel = _newLevel;
            StopAllCoroutines();
            StartCoroutine(MinionSpawnerTimer(currentSpawnTime));
        }
        
        private Vector2 RandomPointInAnnulus(Vector2 _center, float _innerRadius, float _outerRadius)
        {
            if (_outerRadius < _innerRadius) (_innerRadius, _outerRadius) = (_outerRadius, _innerRadius);
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float u = Random.value;
            float r = Mathf.Sqrt(Mathf.Lerp(_innerRadius * _innerRadius, _outerRadius * _outerRadius, u));
            return _center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
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
            
            Vector3 spawnPosition = RandomPointInAnnulus(player.transform.position, SpawnRangeMin, SpawnRangeMax);

            spawnCount++;
            var enemy = pooledMinions.GetFromPool(prefab, spawnPosition, Quaternion.identity);

            //Get the EnemyController component and set the player reference
            if (enemy && enemy.TryGetComponent<Enemies.Controller>(out var newEnemy)) {
                newEnemy.SetPlayerTransform(player.transform);
                newEnemy.OnTakenFromPool(prefab);
            }
        }

        private void StartupComponents(int _sceneIndex)
        {
            StopAllCoroutines();
            if (_sceneIndex is SceneNames.MainMenu or SceneNames.PlayerHome) {
                shouldSpawn = false;
                return;
            } else {
                shouldSpawn = true;
            }

            spawnCount = 0;
            
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
            
            shouldSpawn = true;
            
            StartCoroutine(MinionSpawnerTimer(spawnInterval));
        }

        private IEnumerator MinionSpawnerTimer(float _interval)
        {
            var wait = new WaitForSeconds(_interval);
            while (enabled) {
                if (GameManager.Instance && !GameManager.Instance.IsPaused && shouldSpawn) {
                    if (maxEnemies == 0 || spawnCount < maxEnemies) {
                        SpawnRandomEnemy();
                    }
                }
                yield return wait;
            }
        }
    }
}