using System;
using System.Collections.Generic;
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

        private Player.PlayerController player;
        private float timer;
        private int currentLevel = 1;

        private const float SpawnRangeMin = 10.0f; // Minimum distance from player
        private const float SpawnRangeMax = 20.0f; // Maximum distance from player

        private Transform enemyParent;
        private EnemySpawnChances enemySpawnChances;

        private readonly List<TrackedSpawn> trackedSpawns = new();
        
        private int spawnCount;

        private void Awake()
        {
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

            var parentObject = GameObject.Find("Enemies");
            if (parentObject == null || !parentObject.TryGetComponent(out enemyParent)) {
                enemyParent = new GameObject("Enemies").transform;
            }

        }

        private void OnEnable()
        {
            if (RunScoreManager.Instance == null) {
                Debug.LogError("RunScoreManager instance is null. Ensure it is initialized before enabling EnemySpawner.");
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

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval) {
                timer = 0f;
                SpawnRandomEnemy();
            }
        }

        private void HandleEnemyDeath(AttackType _attackType, int _points, float _itemChance, Vector2 _position, GameObject _enemy)
        {
            TryRemoveTrackedEnemy(_enemy);
            spawnCount--;
            if (_attackType == AttackType.PlayerAttack) {
                SpawnRandomEnemy();
            }
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
            bool trackedEnemy = false;

            while (!prefab && loopCount < 5) {
                loopCount++;
                prefab = enemySpawnChances.GetRandomEnemy(GameManager.Instance.DifficultyLevel, currentLevel);
                int maxSpawns = enemySpawnChances.GetMaxSpawnCount(prefab);
                if (maxSpawns > 0) {
                    trackedEnemy = true;
                    // Check if we have reached the max spawn count for this enemy type
                    foreach (var tracked in trackedSpawns) {
                        if (tracked.Enemy == prefab && tracked.Count >= maxSpawns) {
                            prefab = null; // Reset prefab to null to pick another
                            break;
                        }
                    }
                }
            }


            // Spawn at a random position outside the player's area
            bool useNegative = Random.value < 0.5f; // Randomly decide if we want to use negative or positive range
            Vector2 spawningRange = new(Random.Range(SpawnRangeMin, SpawnRangeMax), Random.Range(SpawnRangeMin, SpawnRangeMax));

            if (useNegative) {
                spawningRange.x *= -1;
                spawningRange.y *= -1;
            }

            Vector3 spawnPosition = player.transform.position + (Vector3)spawningRange;

            spawnCount++;
            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity, enemyParent);
        
            if (trackedEnemy) {
                // Track the spawned enemy
                bool found = false;
                foreach (var tracked in trackedSpawns) {
                    if (tracked.Enemy == prefab) {
                        tracked.Increment(enemy.name);
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    trackedSpawns.Add(new TrackedSpawn(prefab, enemy.name));
                }
            }

            //Get the EnemyController component and set the player reference
            if (enemy.TryGetComponent<Enemies.Controller>(out var newEnemy)) {
                newEnemy.SetPlayerTransform(player.transform);
            }
        }

        private void TryRemoveTrackedEnemy(GameObject _enemy)
        {
            foreach (var tracked in trackedSpawns) {
                if (tracked.Enemy == _enemy) {
                    tracked.Decrement(_enemy.name);
                    if (tracked.Count <= 0) {
                        trackedSpawns.Remove(tracked);
                    }
                    break;
                }
            }
        }
    }

    public struct TrackedSpawn : IEquatable<TrackedSpawn>
    {
        public GameObject Enemy { get; private set; }
        public int Count { get; private set; }
        private readonly List<string> names;

        public TrackedSpawn(GameObject _enemy, string _name)
        {
            Enemy = _enemy;
            Count = 1;
            names = new List<string> { _name };
        }

        public void Increment(string _name)
        {
            Count++;
            names.Add(_name);
        }

        public void Decrement(string _name)
        {
            Count--;
            names.Remove(_name);
        }

        public bool Equals(TrackedSpawn _other)
        {
            return Equals(Enemy, _other.Enemy) && Count == _other.Count && Equals(names, _other.names);
        }

        public override bool Equals(object _obj)
        {
            return _obj is TrackedSpawn other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Enemy, Count, names);
        }
    }
}