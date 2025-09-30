using System.Collections.Generic;
using Game;
using ScriptableObjects;
using UnityEngine;

namespace Managers
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("Base chance (0 to 1) to spawn an item on enemy death"), Range(0f, 1f)]
        private float baseItemSpawnChance = 0.05f;
        [SerializeField, Tooltip("Chance (0 to 1) that a spawned power-up is temporary"), Range(0f, 1f)]
        private float temporaryPowerUpChance = 0.5f;
        [SerializeField, Tooltip("Minimum duration for temporary power-ups")]
        private float minTemporaryDuration = 5f;
        [SerializeField, Tooltip("Maximum duration for temporary power-ups")]
        private float maxTemporaryDuration = 30f;
        [SerializeField, Tooltip("Minimum amount for power-ups that have an amount")]
        private float minAmount = 5f;
        [SerializeField, Tooltip("Maximum amount for power-ups that have an amount")]
        private float maxAmount = 30f;
        [SerializeField, Tooltip("Minimum multiplier for power-ups that have a multiplier")]
        private float minMultiplier = 1.05f;
        [SerializeField, Tooltip("Maximum multiplier for power-ups that have a multiplier")]
        private float maxMultiplier = 1.3f;

        private PowerUpDatabase powerUpDatabase;

        private PooledPowerUpManager poolManager;
        
        private const int InitialPoolSize = 3;
        private const int MaxCountPerPrefab = 10;

        private void Awake()
        {
            powerUpDatabase = Resources.Load<PowerUpDatabase>("ScriptableObjects/PowerUpDatabase");
            if (powerUpDatabase == null) {
                Debug.LogError("PowerUpDatabase not found in Resources/ScriptableObjects.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            Enemies.Controller.OnEnemyDeath += EnemyDeath;
            GameSceneManager.Instance.OnSceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            Enemies.Controller.OnEnemyDeath -= EnemyDeath;
            GameSceneManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
        }

        private PowerUpType ChoosePowerUpType()
        {
            var roll = Random.Range(0f, 1f);

            var normalizedWeights = NormalizedWeights();

            float cumulativeWeight = 0f;
            for (int i = 0; i < normalizedWeights.Length; i++) {
                cumulativeWeight += normalizedWeights[i];
                if (roll <= cumulativeWeight) {
                    return powerUpDatabase.SpawnWeights[i].PowerUpType;
                }
            }
            return PowerUpType.None;
        }

        private void EnemyDeath(AttackType _attackType, int _points, float _itemChance, Vector2 _position, GameObject _enemy)
        {
            float chanceToSpawnItem = baseItemSpawnChance + _itemChance;
            chanceToSpawnItem = Mathf.Clamp01(chanceToSpawnItem);
            float roll = Random.Range(0f, 1f);

            if (roll <= chanceToSpawnItem) {
                SpawnItem(_position);
            }
        }

        private void HandleSceneLoaded(int _sceneIndex)
        {
            if (_sceneIndex is SceneNames.MainMenu or SceneNames.PlayerHome)
                return;
            
            poolManager = new PooledPowerUpManager(InitialPoolSize, MaxCountPerPrefab);
            
            if (!powerUpDatabase) {
                powerUpDatabase = Resources.Load<PowerUpDatabase>("ScriptableObjects/PowerUpDatabase");
                if (powerUpDatabase == null) {
                    Debug.LogError("PowerUpDatabase not found in Resources/ScriptableObjects.");
                }
            }

            foreach (var prefab in powerUpDatabase.GetAllPrefabs()) {
                poolManager.PreWarm(prefab);
            }
        }

        private float[] NormalizedWeights()
        {
            float totalWeight = 0f;
            foreach (var entry in powerUpDatabase.SpawnWeights) {
                totalWeight += entry.Weight;
            }
            float[] normalizedWeights = new float[powerUpDatabase.SpawnWeights.Length];
            for (int i = 0; i < powerUpDatabase.SpawnWeights.Length; i++) {
                normalizedWeights[i] = powerUpDatabase.SpawnWeights[i].Weight / totalWeight;
            }
            return normalizedWeights;
        }

        private float RandomBiasedNumber(float _min = 5f, float _max = 30f)
        {
            float uniform = Random.Range(0f, 1f);
            float biased = Mathf.Pow(uniform, 2); // Bias towards lower values
            return _min + biased * (_max - _min); // Scale to range [_min, _max]
        }

        public void ReleasePowerUp(GameObject _powerUp)
        {
            poolManager.Release(_powerUp);
        }

        private void SetPowerUpStats(Collectables _powerUp, PowerUpType _type, bool _isTemporary, GameObject _prefabReference)
        {
            switch (_type) {
                case PowerUpType.Invincibility:
                    _powerUp.Initialize(new PowerUpData(this, _prefabReference, _type, _duration: RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration)));
                    break;
                case PowerUpType.DoublePoints:
                    _powerUp.Initialize(new PowerUpData(this,_prefabReference,_type, _duration: RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration)));
                    break;
                case PowerUpType.CoinMagnet:
                    _powerUp.Initialize(new PowerUpData(this, _prefabReference,_type, _duration: RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration)));
                    break;
                case PowerUpType.MaxHealthBoost:
                case PowerUpType.HealAmount:
                case PowerUpType.ManaBoost:
                case PowerUpType.MaxManaBoost:
                    _powerUp.Initialize(new PowerUpData(this, _prefabReference,_type, _duration: _isTemporary ? RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration) : 0f, _amount: RandomBiasedNumber(minAmount, maxAmount)));
                    break;
                case PowerUpType.SuperCooldownReduction:
                    _powerUp.Initialize(new PowerUpData(this, _prefabReference,_type, _duration: 0f, _amount: Random.Range(0f, 1f)));
                    break;
                case PowerUpType.SuperDamageBoost:
                    _powerUp.Initialize(new PowerUpData(this, _prefabReference,_type, _duration: 0f, _multiplier: RandomBiasedNumber(minMultiplier, maxMultiplier)));
                    break;
                case PowerUpType.ManaRegenTickRate:
                case PowerUpType.AttackSpeedBoost:
                case PowerUpType.RangedDamageBoost:
                case PowerUpType.MeleeDamageBoost:
                case PowerUpType.HealthRegenTickRate:
                case PowerUpType.HealthRegenAmount:
                case PowerUpType.ManaRegenAmount:
                case PowerUpType.SpeedBoost:
                case PowerUpType.AuraTickSpeedBoost:
                case PowerUpType.AuraRangeBoost:
                case PowerUpType.AuraDamageBoost:
                    _powerUp.Initialize(new PowerUpData(this, _prefabReference,_type, _duration: _isTemporary ? RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration) : 0f, _multiplier: RandomBiasedNumber(minMultiplier, maxMultiplier)));
                    break;
                default:
                    Debug.LogWarning($"Unhandled PowerUpType {_type} in SetPowerUpStats.");
                    break;
            }
        }

        private void SpawnItem(Vector2 _position)
        {
            var chosenType = ChoosePowerUpType();
            while (chosenType == PowerUpType.None) {
                chosenType = ChoosePowerUpType();
            }
            var isTemporary = RandomBiasedNumber(0f, 1f) < temporaryPowerUpChance;
            var powerUpSprite = powerUpDatabase.GetSpriteForPowerUpType(chosenType, isTemporary);
            var powerUpPrefab = powerUpDatabase.GetPrefabForPowerUpType(chosenType, isTemporary);

            var powerUpInstance = poolManager.GetFromPool(powerUpPrefab, _position, Quaternion.identity);

            if (!powerUpInstance || !powerUpInstance.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) {
                Debug.LogError("PowerUp prefab is missing a SpriteRenderer component.");
                return;
            }

            spriteRenderer.sprite = powerUpSprite;
            if (powerUpInstance.TryGetComponent<Collectables>(out var powerUp)) {
                SetPowerUpStats(powerUp, chosenType, isTemporary, powerUpPrefab);
            } else {
                Debug.LogError("PowerUp prefab is missing a Collectables component.");
            }
        }
        
        private sealed class PooledPowerUpManager
        {
            private readonly Transform poolRoot;
            private readonly int initialSize;
            private readonly int globalMaxPrefabAmount;
            private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
            private readonly Dictionary<GameObject, int> liveCounts = new();
            
            public PooledPowerUpManager(int _initialPoolSize, int _maxSizePerPrefab)
            {
                this.initialSize = Mathf.Max(0, _initialPoolSize);
                this.globalMaxPrefabAmount = Mathf.Max(0, _maxSizePerPrefab);

                var rootGo = GameObject.Find("PowerUps");
                if (!rootGo) {
                    rootGo = new GameObject("PowerUps");
                }
                poolRoot = rootGo.transform;
            }

            private void EnsurePool(GameObject _prefab)
            {
                if (!pools.ContainsKey(_prefab)) {
                    pools[_prefab] = new Queue<GameObject>(initialSize);
                    liveCounts[_prefab] = 0;
                }
            }

            public GameObject GetFromPool(GameObject _prefab, Vector3 _position, Quaternion _rotation)
            {
                EnsurePool(_prefab);
                var queue = pools[_prefab];

                GameObject go;
                if (queue.Count > 0) {
                    go = queue.Dequeue();
                } else {
                    if (!HasSpawnsAvailable(_prefab) || (globalMaxPrefabAmount > 0 && GetLiveCount(_prefab) >= globalMaxPrefabAmount)) {
                        return null;
                    }
                    go = Object.Instantiate(_prefab, poolRoot);
                }

                go.transform.SetPositionAndRotation(_position, _rotation);
                go.SetActive(true);
                
                liveCounts[_prefab]++;
                return go;
            }

            private int GetLiveCount(GameObject _prefab)
            {
                return liveCounts.GetValueOrDefault(_prefab);
            }

            private bool HasSpawnsAvailable(GameObject _prefab)
            {
                return globalMaxPrefabAmount == 0 || GetLiveCount(_prefab) < globalMaxPrefabAmount;
            }
            
            public void PreWarm(GameObject _prefab)
            {
                if (_prefab == null)
                    return;

                EnsurePool(_prefab);
                var queue = pools[_prefab];
                while (queue.Count < initialSize) {
                    var go = Object.Instantiate(_prefab, poolRoot);
                    go.SetActive(false);
                    queue.Enqueue(go);
                }
            }

            public void Release(GameObject _instance)
            {
                if (!_instance)
                    return;
                
                var lootMarker = _instance.GetComponent<IPooledResettable>();
                if (lootMarker == null) {
                    Debug.LogError("Instance does not have a IPooledResettable component for release.");
                    return;
                }
                
                var prefab = lootMarker.GetSourcePrefab();
                EnsurePool(prefab);
                
                _instance.SetActive(false);
                //_instance.transform.SetParent(poolRoot, true);
                pools[prefab].Enqueue(_instance);
                
                liveCounts[prefab]--;
                liveCounts[prefab] = Mathf.Max(0, GetLiveCount(prefab) - 1);
            }
        }
    }
}
