using Game;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    [FoldoutGroup("Collectable Settings")]
    [FoldoutGroup("Collectable Settings/General Settings"), SerializeField, Tooltip("Should this object be collected on pickup?")]
    private bool collectOnPickup = true;
    [FoldoutGroup("Collectable Settings/General Settings"), SerializeField, Tooltip("Should this object be destroyed on pickup?")]
    private bool destroyOnPickup = true;
    [FoldoutGroup("Collectable Settings/General Settings"), SerializeField, Tooltip("Is this a super power up?")]
    private bool isSuperPowerUp = false;
    [FoldoutGroup("Collectable Settings/Audio Settings"), SerializeField, Tooltip("Sound to play on pickup")]
    private AudioClip pickupSound;
    [FoldoutGroup("Collectable Settings/Audio Settings"), SerializeField, Tooltip("Volume of the pickup sound"), Range(0f, 1f)]
    private float pickupSoundVolume = 1f;
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("Should the sprite be randomized?"), HideIf("isSuperPowerUp")]
    private bool randomizeSprite = false;
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("List of sprites? If false, will default to a folder selection"), ShowIf("randomizeSprite")]
    private bool useCustomSpriteList = false;
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("List of sprites to choose from"), ShowIf("useCustomSpriteList")]
    private List<Sprite> customSpriteList = new();
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("Folder to load sprites from"), FolderPath, ShowIf("@this.randomizeSprite && !this.useCustomSpriteList")]
    private string spriteFolderPath = "";
    [FoldoutGroup("Collectable Settings/Collection Settings"), SerializeField, Tooltip("Should the stats be randomized?"), HideIf("isSuperPowerUp")]
    private bool randomizeStats = false;

    [FoldoutGroup("Collectable Stats")]
    [FoldoutGroup("Collectable Stats/Static Stats/Temp Stats"), SerializeField, Tooltip("Should the power up give invincibility?"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private bool giveInvincibility = false;
    [FoldoutGroup("Collectable Stats/Static Stats/Temp Stats"), SerializeField, Tooltip("Duration of invincibility"), ShowIf("giveInvincibility"), Min(0.1f)]
    private float invincibilityDuration = 5f;
    [FoldoutGroup("Collectable Stats/Static Stats/Temp Stats"), SerializeField, Tooltip("Should the power up give double points?"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private bool giveDoublePoints = false;
    [FoldoutGroup("Collectable Stats/Static Stats/Temp Stats"), SerializeField, Tooltip("Duration of double points"), ShowIf("giveDoublePoints"), Min(0.1f)]
    private float doublePointsDuration = 5f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Health to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float healthAmount = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the health boost, 0 is permanent"), ShowIf("@!this.randomizeStats && healthAmount != 0f")]
    private float healthDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Mana to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float manaAmount = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the mana boost, 0 is permanent"), ShowIf("@!this.randomizeStats && manaAmount != 0f")]
    private float manaDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Max mana boost to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float maxManaBoost = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the max mana boost, 0 is permanent"), ShowIf("@!this.randomizeStats && maxManaBoost != 0f")]
    private float maxManaDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Attack speed multiplier to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float attackSpeedMultiplier = 1f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the attack speed boost, 0 is permanent"), ShowIf("@!this.randomizeStats && attackSpeedMultiplier != 1f")]
    private float attackSpeedDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Melee damage boost multiplier to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float meleeDamageBoostMultiplier = 1f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the melee damage boost, 0 is permanent"), ShowIf("@!this.randomizeStats && meleeDamageBoostMultiplier != 1f")]
    private float meleeDamageDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Ranged damage boost multiplier to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float rangedDamageBoostMultiplier = 1f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the ranged damage boost, 0 is permanent"), ShowIf("@!this.randomizeStats && rangedDamageBoostMultiplier != 1f")]
    private float rangedDamageDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Max health boost to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float maxHealthBoost = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the max health boost, 0 is permanent"), ShowIf("@!this.randomizeStats && maxHealthBoost != 0f")]
    private float maxHealthDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Speed boost multiplier to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float speedBoostMultiplier = 1f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the speed boost, 0 is permanent"), ShowIf("@!this.randomizeStats && speedBoostMultiplier != 1f")]
    private float speedBoostDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Aura tick rate multiplier to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float auraTickRateMultiplier = 1f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the aura tick rate boost, 0 is permanent"), ShowIf("@!this.randomizeStats && auraTickRateMultiplier != 1f")]
    private float auraTickRateDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Aura range boost to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float auraRangeBoost = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the aura range boost, 0 is permanent"), ShowIf("@!this.randomizeStats && auraRangeBoost != 0f")]
    private float auraRangeDuration = 0f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Aura damage boost multiplier to give on pickup"), ShowIf("@!this.randomizeStats && !isSuperPowerUp")]
    private float auraDamageBoostMultiplier = 1f;
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Duration of the aura damage boost, 0 is permanent"), ShowIf("@!this.randomizeStats && auraDamageBoostMultiplier != 1f")]
    private float auraDamageDuration = 0f;

    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give Invincibility?"), ShowIf("randomizeStats")]
    private bool randomGiveInvincibility = false;
    [BoxGroup("Collectable Stats/Random Stats/Invincibility"), SerializeField, Tooltip("Min/Max duration of invincibility"), ShowIf("randomGiveInvincibility"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxInvincibilityDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give Double Points?"), ShowIf("randomizeStats")]
    private bool randomGiveDoublePoints = false;
    [BoxGroup("Collectable Stats/Random Stats/Double Points"), SerializeField, Tooltip("Min/Max duration of double points"), ShowIf("randomGiveDoublePoints"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxDoublePointsDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give a health boost?"), ShowIf("randomizeStats")]
    private bool randomGiveHealthBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Health Boost"), SerializeField, Tooltip("Min/Max health to give on pickup"), ShowIf("randomGiveHealthBoost"), MinMaxSlider(0.1f, 1000f, true)]
    private Vector2 maxHealthBoostRange = new(0.1f, 1000f);
    [BoxGroup("Collectable Stats/Random Stats/Health Boost"), SerializeField, Tooltip("Should the health boost be temporary?"), ShowIf("randomGiveHealthBoost")]
    private bool randomHealthBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Health Boost"), SerializeField, Tooltip("Min/Max duration of the health boost"), ShowIf("randomHealthBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxHealthBoostDurationRange = new(0.2f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give a max health boost?"), ShowIf("randomizeStats")]
    private bool randomGiveMaxHealthBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Max Health Boost"), SerializeField, Tooltip("Min/Max max health boost multiplier to give on pickup"), ShowIf("randomGiveMaxHealthBoost"), MinMaxSlider(1f, 5f, true)]
    private Vector2 maxMaxHealthBoostRange = new(1f, 5f);
    [BoxGroup("Collectable Stats/Random Stats/Max Health Boost"), SerializeField, Tooltip("Should the max health boost be temporary?"), ShowIf("randomGiveMaxHealthBoost")]
    private bool randomMaxHealthBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Max Health Boost"), SerializeField, Tooltip("Min/Max duration of the max health boost"), ShowIf("randomMaxHealthBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxMaxHealthBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give a mana boost?"), ShowIf("randomizeStats")]
    private bool randomGiveManaBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Mana Boost"), SerializeField, Tooltip("Min/Max mana to give on pickup"), ShowIf("randomGiveManaBoost"), MinMaxSlider(0.1f, 1000f, true)]
    private Vector2 maxManaBoostRange = new(0.1f, 1000f);
    [BoxGroup("Collectable Stats/Random Stats/Mana Boost"), SerializeField, Tooltip("Should the mana boost be temporary?"), ShowIf("randomGiveManaBoost")]
    private bool randomManaBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Mana Boost"), SerializeField, Tooltip("Min/Max duration of the mana boost"), ShowIf("randomManaBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxManaBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give a max mana boost?"), ShowIf("randomizeStats")]
    private bool randomGiveMaxManaBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Max Mana Boost"), SerializeField, Tooltip("Min/Max max mana boost to give on pickup"), ShowIf("randomGiveMaxManaBoost"), MinMaxSlider(0.1f, 1000f, true)]
    private Vector2 maxMaxManaBoostRange = new(0.1f, 1000f);
    [BoxGroup("Collectable Stats/Random Stats/Max Mana Boost"), SerializeField, Tooltip("Should the max mana boost be temporary?"), ShowIf("randomGiveMaxManaBoost")]
    private bool randomMaxManaBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Max Mana Boost"), SerializeField, Tooltip("Min/Max duration of the max mana boost"), ShowIf("randomMaxManaBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxMaxManaBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give an attack speed boost?"), ShowIf("randomizeStats")]
    private bool randomGiveAttackSpeedBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Attack Speed Boost"), SerializeField, Tooltip("Min/Max attack speed multiplier to give on pickup"), ShowIf("randomGiveAttackSpeedBoost"), MinMaxSlider(1f, 5f, true)]
    private Vector2 maxAttackSpeedBoostRange = new(1f, 5f);
    [BoxGroup("Collectable Stats/Random Stats/Attack Speed Boost"), SerializeField, Tooltip("Should the attack speed boost be temporary?"), ShowIf("randomGiveAttackSpeedBoost")]
    private bool randomAttackSpeedBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Attack Speed Boost"), SerializeField, Tooltip("Min/Max duration of the attack speed boost"), ShowIf("randomAttackSpeedBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxAttackSpeedBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give a melee damage boost?"), ShowIf("randomizeStats")]
    private bool randomGiveMeleeDamageBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Melee Damage Boost"), SerializeField, Tooltip("Min/Max melee damage boost multiplier to give on pickup"), ShowIf("randomGiveMeleeDamageBoost"), MinMaxSlider(1f, 5f, true)]
    private Vector2 maxMeleeDamageBoostRange = new(1f, 5f);
    [BoxGroup("Collectable Stats/Random Stats/Melee Damage Boost"), SerializeField, Tooltip("Should the melee damage boost be temporary?"), ShowIf("randomGiveMeleeDamageBoost")]
    private bool randomMeleeDamageBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Melee Damage Boost"), SerializeField, Tooltip("Min/Max duration of the melee damage boost"), ShowIf("randomMeleeDamageBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxMeleeDamageBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give a ranged damage boost?"), ShowIf("randomizeStats")]
    private bool randomGiveRangedDamageBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Ranged Damage Boost"), SerializeField, Tooltip("Min/Max ranged damage boost multiplier to give on pickup"), ShowIf("randomGiveRangedDamageBoost"), MinMaxSlider(1f, 5f, true)]
    private Vector2 maxRangedDamageBoostRange = new(1f, 5f);
    [BoxGroup("Collectable Stats/Random Stats/Ranged Damage Boost"), SerializeField, Tooltip("Should the ranged damage boost be temporary?"), ShowIf("randomGiveRangedDamageBoost")]
    private bool randomRangedDamageBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Ranged Damage Boost"), SerializeField, Tooltip("Min/Max duration of the ranged damage boost"), ShowIf("randomRangedDamageBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxRangedDamageBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give a speed boost?"), ShowIf("randomizeStats")]
    private bool randomGiveSpeedBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Speed Boost"), SerializeField, Tooltip("Min/Max speed boost multiplier to give on pickup"), ShowIf("randomGiveSpeedBoost"), MinMaxSlider(1f, 5f, true)]
    private Vector2 maxSpeedBoostRange = new(1f, 5f);
    [BoxGroup("Collectable Stats/Random Stats/Speed Boost"), SerializeField, Tooltip("Should the speed boost be temporary?"), ShowIf("randomGiveSpeedBoost")]
    private bool randomSpeedBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Speed Boost"), SerializeField, Tooltip("Min/Max duration of the speed boost"), ShowIf("randomSpeedBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxSpeedBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give an aura tick rate boost?"), ShowIf("randomizeStats")]
    private bool randomGiveAuraTickRateBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Aura Tick Rate Boost"), SerializeField, Tooltip("Min/Max aura tick rate boost multiplier to give on pickup"), ShowIf("randomGiveAuraTickRateBoost"), MinMaxSlider(1f, 5f, true)]
    private Vector2 maxAuraTickRateBoostRange = new(1f, 5f);
    [BoxGroup("Collectable Stats/Random Stats/Aura Tick Rate Boost"), SerializeField, Tooltip("Should the aura tick rate boost be temporary?"), ShowIf("randomGiveAuraTickRateBoost")]
    private bool randomAuraTickRateBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Aura Tick Rate Boost"), SerializeField, Tooltip("Min/Max duration of the aura tick rate boost"), ShowIf("randomAuraTickRateBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxAuraTickRateBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give an aura range boost?"), ShowIf("randomizeStats")]
    private bool randomGiveAuraRangeBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Aura Range Boost"), SerializeField, Tooltip("Min/Max aura range boost to give on pickup"), ShowIf("randomGiveAuraRangeBoost"), MinMaxSlider(1f, 100f, true)]
    private Vector2 maxAuraRangeBoostRange = new(1f, 100f);
    [BoxGroup("Collectable Stats/Random Stats/Aura Range Boost"), SerializeField, Tooltip("Should the aura range boost be temporary?"), ShowIf("randomGiveAuraRangeBoost")]
    private bool randomAuraRangeBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Aura Range Boost"), SerializeField, Tooltip("Min/Max duration of the aura range boost"), ShowIf("randomAuraRangeBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxAuraRangeBoostDurationRange = new(0.1f, 120f);
    [FoldoutGroup("Collectable Stats/Random Stats"), SerializeField, Tooltip("Can the power up give an aura damage boost?"), ShowIf("randomizeStats")]
    private bool randomGiveAuraDamageBoost = false;
    [BoxGroup("Collectable Stats/Random Stats/Aura Damage Boost"), SerializeField, Tooltip("Min/Max aura damage boost multiplier to give on pickup"), ShowIf("randomGiveAuraDamageBoost"), MinMaxSlider(1f, 5f, true)]
    private Vector2 maxAuraDamageBoostRange = new(1f, 5f);
    [BoxGroup("Collectable Stats/Random Stats/Aura Damage Boost"), SerializeField, Tooltip("Should the aura damage boost be temporary?"), ShowIf("randomGiveAuraDamageBoost")]
    private bool randomAuraDamageBoostTemporary = false;
    [BoxGroup("Collectable Stats/Random Stats/Aura Damage Boost"), SerializeField, Tooltip("Min/Max duration of the aura damage boost"), ShowIf("randomAuraDamageBoostTemporary"), MinMaxSlider(0.1f, 120f, true)]
    private Vector2 maxAuraDamageBoostDurationRange = new(0.1f, 120f);

    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Invincibility"), ShowIf("@randomizeStats && randomGiveInvincibility"), Min(0f)]
    private float weightInvincibility = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Double Points"), ShowIf("@randomizeStats && randomGiveDoublePoints"), Min(0f)]
    private float weightDoublePoints = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Health Boost"), ShowIf("@randomizeStats && randomGiveHealthBoost"), Min(0f)]
    private float weightHealthBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Max Health Boost"), ShowIf("@randomizeStats && randomGiveMaxHealthBoost"), Min(0f)]
    private float weightMaxHealthBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Mana Boost"), ShowIf("@randomizeStats && randomGiveManaBoost"), Min(0f)]
    private float weightManaBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Max Mana Boost"), ShowIf("@randomizeStats && randomGiveMaxManaBoost"), Min(0f)]
    private float weightMaxManaBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Attack Speed Boost"), ShowIf("@randomizeStats && randomGiveAttackSpeedBoost"), Min(0f)]
    private float weightAttackSpeedBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Melee Damage Boost"), ShowIf("@randomizeStats && randomGiveMeleeDamageBoost"), Min(0f)]
    private float weightMeleeDamageBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Ranged Damage Boost"), ShowIf("@randomizeStats && randomGiveRangedDamageBoost"), Min(0f)]
    private float weightRangedDamageBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Speed Boost"), ShowIf("@randomizeStats && randomGiveSpeedBoost"), Min(0f)]
    private float weightSpeedBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Aura Tick Rate Boost"), ShowIf("@randomizeStats && randomGiveAuraTickRateBoost"), Min(0f)]
    private float weightAuraTickRateBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Aura Range Boost"), ShowIf("@randomizeStats && randomGiveAuraRangeBoost"), Min(0f)]
    private float weightAuraRangeBoost = 1f;
    [FoldoutGroup("Collectable Stats/Weights"), SerializeField, Tooltip("Weight for Aura Damage Boost"), ShowIf("@randomizeStats && randomGiveAuraDamageBoost"), Min(0f)]
    private float weightAuraDamageBoost = 1f;

    [FoldoutGroup("Collectable Stats/Number of Stats"), SerializeField, Tooltip("Number of stats to give on pickup"), Min(1), ShowIf("randomizeStats")]
    private int numberOfStatsToGive = 1;

    [FoldoutGroup("Super Settings"), SerializeField, Tooltip("The Prefab to use for the super power up"), ShowIf("isSuperPowerUp")]
    private GameObject superPowerUpPrefab;

    [SerializeField, ReadOnly, Tooltip("Unique ID for this collectable")]
    private string collectableID;
    [SerializeField, ReadOnly, Tooltip("Has the ID been set?")]
    private bool iDSet = false;

    private const float pickupDelay = 0.5f;
    private float pickupDelayTimer = 0f;
    private readonly List<PowerUpData> powerUps = new();
    private bool isEnabled = false;

    // Cached Component
    private Collider2D collectableCollider;


    private void Awake()
    {
        if (randomizeSprite) {
            RandomizeSprite();
        }
        if (randomizeStats) {
            RandomizeStats();
        } else {
            LoadStaticStats();
        }
        if (!iDSet) {
            collectableID = System.Guid.NewGuid().ToString();
            iDSet = true;
        }
        if (!this.TryGetComponent<Collider2D>(out collectableCollider)) {
            Debug.LogError("No Collider2D component found on " + gameObject.name);
        } else {
            collectableCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (pickupDelayTimer > 0) {
            pickupDelayTimer -= Time.deltaTime;
        } else if (!isEnabled) {
            collectableCollider.enabled = true;
            isEnabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player")) {
            if (collectOnPickup) {
                Collect(_other);
            }
            if (pickupSound != null) {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
            }
            if (destroyOnPickup) {
                Destroy(gameObject);
            }
        }
    }

    public void Initialize(PowerUpData _data)
    {
        powerUps.Add(_data);
    }


    private void ChooseRandomStat(List<(string, float)> _possibleStats)
    {
        float totalWeight = 0f;
        foreach (var stat in _possibleStats) {
            totalWeight += stat.Item2;
        }
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        foreach (var stat in _possibleStats) {
            cumulativeWeight += stat.Item2;
            if (randomValue <= cumulativeWeight) {
                SetStat(stat);
                break;
            }
        }
    }

    private void Collect(Collider2D _other)
    {
        if (_other.TryGetComponent<Player.PlayerController>(out var player)) {
            if (isSuperPowerUp && superPowerUpPrefab != null) {
                player.SetSuper(superPowerUpPrefab);
                return;
            }
            foreach (var powerUp in powerUps) {
                player.ActivatePowerUp(powerUp);
            }
        } else {
            Debug.LogWarning("PlayerController component not found on " + _other.name);
        }
    }

    private void LoadStaticStats()
    {
        if (giveInvincibility) {
            powerUps.Add(new PowerUpData(PowerUpType.Invincibility, invincibilityDuration));
        }
        if (giveDoublePoints) {
            powerUps.Add(new PowerUpData(PowerUpType.DoublePoints, doublePointsDuration));
        }
        if (healthAmount != 0f) {
            powerUps.Add(new PowerUpData(PowerUpType.HealAmount, healthDuration, 0f, healthAmount));
        }
        if (maxHealthBoost != 0f) {
            powerUps.Add(new PowerUpData(PowerUpType.MaxHealthBoost, maxHealthDuration, 0f, maxHealthBoost));
        }
        if (manaAmount != 0f) {
            powerUps.Add(new PowerUpData(PowerUpType.ManaBoost, manaDuration, 0f, manaAmount));
        }
        if (maxManaBoost != 0f) {
            powerUps.Add(new PowerUpData(PowerUpType.MaxManaBoost, maxManaDuration, 0f, maxManaBoost));
        }
        if (attackSpeedMultiplier != 1f) {
            powerUps.Add(new PowerUpData(PowerUpType.AttackSpeedBoost, attackSpeedDuration, attackSpeedMultiplier));
        }
        if (meleeDamageBoostMultiplier != 1f) {
            powerUps.Add(new PowerUpData(PowerUpType.MeleeDamageBoost, meleeDamageDuration, meleeDamageBoostMultiplier));
        }
        if (rangedDamageBoostMultiplier != 1f) {
            powerUps.Add(new PowerUpData(PowerUpType.RangedDamageBoost, rangedDamageDuration, rangedDamageBoostMultiplier));
        }
        if (speedBoostMultiplier != 1f) {
            powerUps.Add(new PowerUpData(PowerUpType.SpeedBoost, speedBoostDuration, speedBoostMultiplier));
        }
        if (auraTickRateMultiplier != 1f) {
            powerUps.Add(new PowerUpData(PowerUpType.AuraTickSpeedBoost, auraTickRateDuration, auraTickRateMultiplier));
        }
        if (auraRangeBoost != 0f) {
            powerUps.Add(new PowerUpData(PowerUpType.AuraRangeBoost, auraRangeDuration, auraRangeBoost));
        }
        if (auraDamageBoostMultiplier != 1f) {
            powerUps.Add(new PowerUpData(PowerUpType.AuraDamageBoost, auraDamageDuration, auraDamageBoostMultiplier));
        }
    }

    private void RandomizeSprite()
    {
        List<Sprite> sprites = new();
        if (useCustomSpriteList) {
            sprites = customSpriteList;
        } else {
            sprites.AddRange(Resources.LoadAll<Sprite>(spriteFolderPath));
        }
        if (sprites.Count > 0) {
            if (this.TryGetComponent<SpriteRenderer>(out var sr)) {
                sr.sprite = sprites[Random.Range(0, sprites.Count)];
            }
        } else {
            Debug.LogWarning("No sprites found in folder: " + spriteFolderPath);
        }
    }

    private void RandomizeStats()
    {
        List<(string, float)> possibleStats = new();
        if (randomGiveInvincibility) {
            possibleStats.Add(("Invincibility", weightInvincibility));
        }
        if (randomGiveDoublePoints) {
            possibleStats.Add(("DoublePoints", weightDoublePoints));
        }
        if (randomGiveHealthBoost) {
            possibleStats.Add(("HealthBoost", weightHealthBoost));
        }
        if (randomGiveMaxHealthBoost) {
            possibleStats.Add(("MaxHealthBoost", weightMaxHealthBoost));
        }
        if (randomGiveManaBoost) {
            possibleStats.Add(("ManaBoost", weightManaBoost));
        }
        if (randomGiveMaxManaBoost) {
            possibleStats.Add(("MaxManaBoost", weightMaxManaBoost));
        }
        if (randomGiveAttackSpeedBoost) {
            possibleStats.Add(("AttackSpeedBoost", weightAttackSpeedBoost));
        }
        if (randomGiveMeleeDamageBoost) {
            possibleStats.Add(("MeleeDamageBoost", weightMeleeDamageBoost));
        }
        if (randomGiveRangedDamageBoost) {
            possibleStats.Add(("RangedDamageBoost", weightRangedDamageBoost));
        }
        if (randomGiveSpeedBoost) {
            possibleStats.Add(("SpeedBoost", weightSpeedBoost));
        }
        if (randomGiveAuraTickRateBoost) {
            possibleStats.Add(("AuraTickRateBoost", weightAuraTickRateBoost));
        }
        if (randomGiveAuraRangeBoost) {
            possibleStats.Add(("AuraRangeBoost", weightAuraRangeBoost));
        }
        if (randomGiveAuraDamageBoost) {
            possibleStats.Add(("AuraDamageBoost", weightAuraDamageBoost));
        }
        if (possibleStats.Count > 0) {
            if (possibleStats.Count < numberOfStatsToGive) {
                numberOfStatsToGive = possibleStats.Count;
            }
            for (int i = 0; i < numberOfStatsToGive; i++) {
                ChooseRandomStat(possibleStats);
            }
        }
    }

    private void SetStat((string, float) _stat)
    {
        switch (_stat.Item1) {
            case "Invincibility":
                giveInvincibility = true;
                invincibilityDuration = Random.Range(maxInvincibilityDurationRange.x, maxInvincibilityDurationRange.y);
                powerUps.Add(new PowerUpData(PowerUpType.Invincibility, invincibilityDuration));
                break;
            case "DoublePoints":
                giveDoublePoints = true;
                doublePointsDuration = Random.Range(maxDoublePointsDurationRange.x, maxDoublePointsDurationRange.y);
                powerUps.Add(new PowerUpData(PowerUpType.DoublePoints, doublePointsDuration));
                break;
            case "HealthBoost":
                healthAmount = Random.Range(maxHealthBoostRange.x, maxHealthBoostRange.y);
                if (randomHealthBoostTemporary) {
                    healthDuration = Random.Range(maxHealthBoostDurationRange.x, maxHealthBoostDurationRange.y);
                } else {
                    healthDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.HealAmount, healthDuration, 0f, healthAmount));
                break;
            case "MaxHealthBoost":
                maxHealthBoost = Random.Range(maxMaxHealthBoostRange.x, maxMaxHealthBoostRange.y);
                if (randomMaxHealthBoostTemporary) {
                    maxHealthDuration = Random.Range(maxMaxHealthBoostDurationRange.x, maxMaxHealthBoostDurationRange.y);
                } else {
                    maxHealthDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.MaxHealthBoost, maxHealthDuration, 0f, maxHealthBoost));
                break;
            case "ManaBoost":
                manaAmount = Random.Range(maxManaBoostRange.x, maxManaBoostRange.y);
                if (randomManaBoostTemporary) {
                    manaDuration = Random.Range(maxManaBoostDurationRange.x, maxManaBoostDurationRange.y);
                } else {
                    manaDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.ManaBoost, manaDuration, 0f, manaAmount));
                break;
            case "MaxManaBoost":
                maxManaBoost = Random.Range(maxMaxManaBoostRange.x, maxMaxManaBoostRange.y);
                if (randomMaxManaBoostTemporary) {
                    maxManaDuration = Random.Range(maxMaxManaBoostDurationRange.x, maxMaxManaBoostDurationRange.y);
                } else {
                    maxManaDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.MaxManaBoost, maxManaDuration, 0f, maxManaBoost));
                break;
            case "AttackSpeedBoost":
                attackSpeedMultiplier = Random.Range(maxAttackSpeedBoostRange.x, maxAttackSpeedBoostRange.y);
                if (randomAttackSpeedBoostTemporary) {
                    attackSpeedDuration = Random.Range(maxAttackSpeedBoostDurationRange.x, maxAttackSpeedBoostDurationRange.y);
                } else {
                    attackSpeedDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.AttackSpeedBoost, attackSpeedDuration, attackSpeedMultiplier));
                break;
            case "MeleeDamageBoost":
                meleeDamageBoostMultiplier = Random.Range(maxMeleeDamageBoostRange.x, maxMeleeDamageBoostRange.y);
                if (randomMeleeDamageBoostTemporary) {
                    meleeDamageDuration = Random.Range(maxMeleeDamageBoostDurationRange.x, maxMeleeDamageBoostDurationRange.y);
                } else {
                    meleeDamageDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.MeleeDamageBoost, meleeDamageDuration, meleeDamageBoostMultiplier));
                break;
            case "RangedDamageBoost":
                rangedDamageBoostMultiplier = Random.Range(maxRangedDamageBoostRange.x, maxRangedDamageBoostRange.y);
                if (randomRangedDamageBoostTemporary) {
                    rangedDamageDuration = Random.Range(maxRangedDamageBoostDurationRange.x, maxRangedDamageBoostDurationRange.y);
                } else {
                    rangedDamageDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.RangedDamageBoost, rangedDamageDuration, rangedDamageBoostMultiplier));
                break;
            case "SpeedBoost":
                speedBoostMultiplier = Random.Range(maxSpeedBoostRange.x, maxSpeedBoostRange.y);
                if (randomSpeedBoostTemporary) {
                    speedBoostDuration = Random.Range(maxSpeedBoostDurationRange.x, maxSpeedBoostDurationRange.y);
                } else {
                    speedBoostDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.SpeedBoost, speedBoostDuration, speedBoostMultiplier));
                break;
            case "AuraTickRateBoost":
                auraTickRateMultiplier = Random.Range(maxAuraTickRateBoostRange.x, maxAuraTickRateBoostRange.y);
                if (randomAuraTickRateBoostTemporary) {
                    auraTickRateDuration = Random.Range(maxAuraTickRateBoostDurationRange.x, maxAuraTickRateBoostDurationRange.y);
                } else {
                    auraTickRateDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.AuraTickSpeedBoost, auraTickRateDuration, auraTickRateMultiplier));
                break;
            case "AuraRangeBoost":
                auraRangeBoost = Random.Range(maxAuraRangeBoostRange.x, maxAuraRangeBoostRange.y);
                if (randomAuraRangeBoostTemporary) {
                    auraRangeDuration = Random.Range(maxAuraRangeBoostDurationRange.x, maxAuraRangeBoostDurationRange.y);
                } else {
                    auraRangeDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.AuraRangeBoost, auraRangeDuration, auraRangeBoost));
                break;
            case "AuraDamageBoost":
                auraDamageBoostMultiplier = Random.Range(maxAuraDamageBoostRange.x, maxAuraDamageBoostRange.y);
                if (randomAuraDamageBoostTemporary) {
                    auraDamageDuration = Random.Range(maxAuraDamageBoostDurationRange.x, maxAuraDamageBoostDurationRange.y);
                } else {
                    auraDamageDuration = 0f;
                }
                powerUps.Add(new PowerUpData(PowerUpType.AuraDamageBoost, auraDamageDuration, auraDamageBoostMultiplier));
                break;
            default:
                Debug.LogError("Unknown stat: " + _stat.Item1);
                break;
        }
    }

    private void OnValidate()
    {
        if (!randomizeSprite) {
            useCustomSpriteList = false;
        } else {
            giveInvincibility = false;
            giveDoublePoints = false;
        }
        if (!randomizeStats) {
            randomGiveInvincibility = false;
            randomGiveDoublePoints = false;
            randomGiveHealthBoost = false;
            randomGiveMaxHealthBoost = false;
            randomGiveManaBoost = false;
            randomGiveMaxManaBoost = false;
            randomGiveAttackSpeedBoost = false;
            randomGiveMeleeDamageBoost = false;
            randomGiveRangedDamageBoost = false;
            randomGiveSpeedBoost = false;
            randomGiveAuraTickRateBoost = false;
            randomGiveAuraRangeBoost = false;
            randomGiveAuraDamageBoost = false;
        }
        if (!randomGiveHealthBoost) {
            randomHealthBoostTemporary = false;
        }
        if (!randomGiveMaxHealthBoost) {
            randomMaxHealthBoostTemporary = false;
        }
        if (!randomGiveManaBoost) {
            randomManaBoostTemporary = false;
        }
        if (!randomGiveMaxManaBoost) {
            randomMaxManaBoostTemporary = false;
        }
        if (!randomGiveAttackSpeedBoost) {
            randomAttackSpeedBoostTemporary = false;
        }
        if (!randomGiveMeleeDamageBoost) {
            randomMeleeDamageBoostTemporary = false;
        }
        if (!randomGiveRangedDamageBoost) {
            randomRangedDamageBoostTemporary = false;
        }
        if (!randomGiveSpeedBoost) {
            randomSpeedBoostTemporary = false;
        }
        if (!randomGiveAuraTickRateBoost) {
            randomAuraTickRateBoostTemporary = false;
        }
        if (!randomGiveAuraRangeBoost) {
            randomAuraRangeBoostTemporary = false;
        }
        if (!randomGiveAuraDamageBoost) {
            randomAuraDamageBoostTemporary = false;
        }
        if (!randomGiveAttackSpeedBoost) {
            weightAttackSpeedBoost = 0f;
        }
        if (!randomGiveDoublePoints) {
            weightDoublePoints = 0f;
        }
        if (!randomGiveHealthBoost) {
            weightHealthBoost = 0f;
        }
        if (!randomGiveInvincibility) {
            weightInvincibility = 0f;
        }
        if (!randomGiveManaBoost) {
            weightManaBoost = 0f;
        }
        if (!randomGiveMaxHealthBoost) {
            weightMaxHealthBoost = 0f;
        }
        if (!randomGiveMaxManaBoost) {
            weightMaxManaBoost = 0f;
        }
        if (!randomGiveMeleeDamageBoost) {
            weightMeleeDamageBoost = 0f;
        }
        if (!randomGiveRangedDamageBoost) {
            weightRangedDamageBoost = 0f;
        }
        if (!randomGiveSpeedBoost) {
            weightSpeedBoost = 0f;
        }
        if (!randomGiveAuraTickRateBoost) {
            weightAuraTickRateBoost = 0f;
        }
        if (!randomGiveAuraRangeBoost) {
            weightAuraRangeBoost = 0f;
        }
        if (!randomGiveAuraDamageBoost) {
            weightAuraDamageBoost = 0f;
        }
        if (numberOfStatsToGive < 1) {
            numberOfStatsToGive = 1;
        }

        }

}
