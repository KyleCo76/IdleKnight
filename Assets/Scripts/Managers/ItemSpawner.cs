using Game;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("Base chance (0 to 1) to spawn an item on enemy death"), Range(0f, 1f)]
    private float baseItemSpawnChance = 0.05f;

    private PowerUpDatabase powerUpDatabase;

    private void Awake()
    {
        powerUpDatabase = Resources.Load<PowerUpDatabase>("ScriptableObjects/PowerUpDatabase");
        if (powerUpDatabase == null) {
            Debug.LogError("PowerUpDatabase not found in Resources/ScriptableObjects.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        Enemies.Controller.OnEnemyDeath += EnemyDeath;
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

    private void EnemyDeath(AttackType _attackType, float _points, float _itemChance, Vector2 _position)
    {
        float chanceToSpawnItem = baseItemSpawnChance + _itemChance;
        chanceToSpawnItem = Mathf.Clamp01(chanceToSpawnItem);
        float roll = Random.Range(0f, 1f);

        if (roll <= chanceToSpawnItem) {
            Debug.Log($"Spawning item at {_position} (Roll: {roll}, Chance: {chanceToSpawnItem})");
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

    private float RandomAmount()
    {
        return Random.Range(5f, 50f);
    }

    private float RandomDuration()
    {
        return Random.Range(5f, 20f);
    }

    private float RandomMultiplier()
    {
        float biasedRoll = Random.Range(0f, 1f);
        biasedRoll = Mathf.Pow(biasedRoll, 2); // Bias towards lower values
        return 1f + biasedRoll * (2f -1f); // Scale to range [1, 2]
    }

    private void SetPowerUpStats(Collectables _powerUp, PowerUpType _type, bool _isTemporary)
    {
        switch (_type) {
            case PowerUpType.Invincibility:
                _powerUp.Initialize(new PowerUpData(_type, duration: RandomDuration()));
                break;
            case PowerUpType.DoublePoints:
                _powerUp.Initialize(new PowerUpData(_type, duration: RandomDuration()));
                break;
            case PowerUpType.CoinMagnet:
                _powerUp.Initialize(new PowerUpData(_type, duration: RandomDuration()));
                break;
            case PowerUpType.AttackSpeedBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.RangedDamageBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.MeleeDamageBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.HealthRegenTickRate:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.HealthRegenAmount:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.MaxHealthBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, amount: RandomAmount()));
                break;
            case PowerUpType.HealAmount:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, amount: RandomAmount()));
                break;
            case PowerUpType.ManaRegenTickRate:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.ManaRegenAmount:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.ManaBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, amount: RandomAmount()));
                break;
            case PowerUpType.MaxManaBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, amount: RandomAmount()));
                break;
            case PowerUpType.SpeedBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.AuraTickSpeedBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.AuraRangeBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            case PowerUpType.AuraDamageBoost:
                _powerUp.Initialize(new PowerUpData(_type, duration: _isTemporary ? RandomDuration() : 0f, multiplier: RandomMultiplier()));
                break;
            default:
                Debug.LogWarning($"Unhandled PowerUpType {_type} in SetPowerUpStats.");
                break;
        }
    }

    public void SpawnItem(Vector2 _position)
    {
        PowerUpType chosenType = ChoosePowerUpType();
        bool isTemporary = ChoosePowerUpTemporary();
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
