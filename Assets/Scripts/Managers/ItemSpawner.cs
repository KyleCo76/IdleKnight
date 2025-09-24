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
        }

        private void OnDisable()
        {
            Enemies.Controller.OnEnemyDeath -= EnemyDeath;
        }

        private bool ChoosePowerUpTemporary()
        {
            float roll = Random.Range(0f, 1f);
            return roll <= 0.5f; // 50% chance
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

        private void SetPowerUpStats(Collectables _powerUp, PowerUpType _type, bool _isTemporary)
        {
            switch (_type) {
                case PowerUpType.Invincibility:
                    _powerUp.Initialize(new PowerUpData(_type, _duration: RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration)));
                    break;
                case PowerUpType.DoublePoints:
                    _powerUp.Initialize(new PowerUpData(_type, _duration: RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration)));
                    break;
                case PowerUpType.CoinMagnet:
                    _powerUp.Initialize(new PowerUpData(_type, _duration: RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration)));
                    break;
                case PowerUpType.MaxHealthBoost:
                case PowerUpType.HealAmount:
                case PowerUpType.ManaBoost:
                case PowerUpType.MaxManaBoost:
                    _powerUp.Initialize(new PowerUpData(_type, _duration: _isTemporary ? RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration) : 0f, _amount: RandomBiasedNumber(minAmount, maxAmount)));
                    break;
                case PowerUpType.SuperCooldownReduction:
                    _powerUp.Initialize(new PowerUpData(_type, _duration: 0f, _amount: Random.Range(0f, 1f)));
                    break;
                case PowerUpType.SuperDamageBoost:
                    _powerUp.Initialize(new PowerUpData(_type, _duration: 0f, _multiplier: RandomBiasedNumber(minMultiplier, maxMultiplier)));
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
                    _powerUp.Initialize(new PowerUpData(_type, _duration: _isTemporary ? RandomBiasedNumber(minTemporaryDuration, maxTemporaryDuration) : 0f, _multiplier: RandomBiasedNumber(minMultiplier, maxMultiplier)));
                    break;
                default:
                    Debug.LogWarning($"Unhandled PowerUpType {_type} in SetPowerUpStats.");
                    break;
            }
        }

        private void SpawnItem(Vector2 _position)
        {
            PowerUpType chosenType = ChoosePowerUpType();
            while (chosenType == PowerUpType.None) {
                chosenType = ChoosePowerUpType();
            }
            bool isTemporary = RandomBiasedNumber(0f, 1f) < temporaryPowerUpChance;
            Sprite powerUpSprite = powerUpDatabase.GetSpriteForPowerUpType(chosenType, isTemporary);
            GameObject powerUpPrefab = powerUpDatabase.GetPrefabForPowerUpType(chosenType, isTemporary);

            GameObject powerUpInstance = Instantiate(powerUpPrefab, _position, Quaternion.identity);

            if (powerUpInstance == null || !powerUpInstance.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) {
                Debug.LogError("PowerUp prefab is missing a SpriteRenderer component.");
                return;
            }

            spriteRenderer.sprite = powerUpSprite;
            if (powerUpInstance.TryGetComponent<Collectables>(out var powerUp)) {
                SetPowerUpStats(powerUp, chosenType, isTemporary);
            } else {
                Debug.LogError("PowerUp prefab is missing a Collectables component.");
            }
        }
    }
}
