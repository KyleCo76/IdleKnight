using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnemySpawnChances", menuName = "Scriptable Objects/EnemySpawnChances")]
    public class EnemySpawnChances : ScriptableObject
    {
        [System.Serializable]
        public struct EnemySpawnChance
        {
            public GameObject EnemyPrefab;
            public int LevelRequirement;
            [Range(0, 1)]
            public float SpawnChance;
            [Range(0, 1)]
            public float DifficultyMultiplier;
            [Tooltip("If true, the enemy will only spawn up to MaxSpawnCount times in the scene.")]
            public bool MaxSpawn;
            [ShowIf("MaxSpawn")]
            public int MaxSpawnCount;
        }

        [FormerlySerializedAs("enemySpawnChances")] public EnemySpawnChance[] SpawnChances;


        public int GetMaxSpawnCount(GameObject _enemyPrefab)
        {
            foreach (var enemy in SpawnChances)
            {
                if (enemy.EnemyPrefab == _enemyPrefab && enemy.MaxSpawn)
                {
                    return enemy.MaxSpawnCount;
                }
            }
            return -1; // Return -1 if the enemy prefab is not found or has no max spawn limit
        }

        public GameObject GetRandomEnemy(int _difficulty, int _currentLevel)
        {
            float totalChance = 0f;
            foreach (var enemy in SpawnChances)
            {
                if (enemy.LevelRequirement <= _currentLevel)
                {
                    totalChance += enemy.SpawnChance + (enemy.DifficultyMultiplier * _difficulty);
                }
            }
            float randomValue = Random.Range(0, totalChance);
            float cumulativeChance = 0f;
            foreach (var enemy in SpawnChances)
            {
                if (enemy.LevelRequirement > _currentLevel)
                    continue;
                cumulativeChance += enemy.SpawnChance + (enemy.DifficultyMultiplier * _difficulty);
                if (randomValue <= cumulativeChance)
                {
                    return enemy.EnemyPrefab;
                }
            }
            return null;
        }

        public Dictionary<GameObject, int> GetAllEnemyPrefabsWithCount()
        {
            Dictionary<GameObject, int> enemies = new();
            foreach (var enemy in SpawnChances) {
                enemies.Add(enemy.EnemyPrefab, enemy.MaxSpawnCount);
            }
            return enemies;
        }
    }
}
