using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class EnemyAuraEffects : MonoBehaviour
{
    [FoldoutGroup("Settings"), SerializeField, Tooltip("The radius of the aura effect.")]
    private float auraRadius;

    [SerializeField, Tooltip("The effects of the aura.")]
    private AuraEffect[] auraEffects;

    private bool damagePlayer = false;
    private float playerTickTimer = 0f;
    private float enemyTickTimer = 0f;


    // Cached Components
    private readonly List<Enemies.Controller> affectedEnemies = new();
    private Player.PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!this.TryGetComponent<CircleCollider2D>(out var collider)) {
            collider = this.gameObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = true;
        collider.radius = auraRadius;

        if (System.Array.Exists(auraEffects, effect => effect.AuraEffectType == AuraEffectTypes.DamageOverTime)) {
            damagePlayer = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (damagePlayer && playerController != null) {
            playerTickTimer += Time.deltaTime;
            foreach (var effect in auraEffects) {
                if (effect.AuraEffectType == AuraEffectTypes.DamageOverTime && playerTickTimer >= effect.AuraEffectTickRate) {
                    playerController.ChangeHealth(-effect.AuraEffectIntensityValue);
                    playerTickTimer = 0f;
                }
            }

            if (affectedEnemies.Count > 0) {
                enemyTickTimer += Time.deltaTime;
                foreach (var effect in auraEffects) {
                    if (effect.AuraEffectType == AuraEffectTypes.HealOverTime && enemyTickTimer >= effect.AuraEffectTickRate) {
                        foreach (var enemy in affectedEnemies) {
                            enemy.ChangeHealth(effect.AuraEffectIntensityValue);
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemies.Controller>(out var enemy)) {
            if (!affectedEnemies.Contains(enemy)) {
                affectedEnemies.Add(enemy);
                ApplyAuraEffects(enemy);
            }
        } else if (collision.TryGetComponent<Player.PlayerController>(out var player)) {
            playerController = player;
            ApplyAuraEffects(playerController);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemies.Controller>(out var enemy)) {
            if (affectedEnemies.Contains(enemy)) {
                RemoveAuraEffects(enemy);
                affectedEnemies.Remove(enemy);
            } else if (collision.TryGetComponent<Player.PlayerController>(out var player)) {
                if (playerController == player) {
                    playerController = null;
                    RemoveAuraEffects(player);
                }
            }
        }
    }


    private void ApplyAuraEffects(Enemies.Controller enemy)
    {
        foreach (var effect in auraEffects)
        {
            switch (effect.AuraEffectType)
            {
                case AuraEffectTypes.SpeedBoost:
                    enemy.ApplySpeedBoost(effect.AuraEffectIntensityMultiplier);
                    break;
                case AuraEffectTypes.Shield:
                    enemy.ApplyShield(effect.AuraEffectIntensityValue);
                    break;
            }
        }
    }

    private void ApplyAuraEffects(Player.PlayerController player)
    {
        foreach (var effect in auraEffects)
        {
            if (effect.AuraEffectType == AuraEffectTypes.Slow) {
                player.ApplySlow(effect.AuraEffectIntensityMultiplier);
            }
        }
    }

    private void RemoveAuraEffects(Enemies.Controller enemy)
    {
        foreach (var effect in auraEffects)
        {
            if (effect.AuraEffectType == AuraEffectTypes.SpeedBoost) {
                enemy.RemoveSpeedBoost();
            }
        }
    }

    private void RemoveAuraEffects(Player.PlayerController player)
    {
        foreach (var effect in auraEffects)
        {
            if (effect.AuraEffectType == AuraEffectTypes.Slow) {
                player.RemoveSlow();
            }
        }
    }
}

public enum AuraEffectTypes
{
    DamageOverTime,
    Slow,
    HealOverTime,
    SpeedBoost,
    Shield
}

[System.Serializable]
public struct AuraEffect
{
    public readonly bool IsIntensityMultiplier => this.AuraEffectType == AuraEffectTypes.SpeedBoost || this.AuraEffectType == AuraEffectTypes.Slow;

    [FoldoutGroup("Aura Effects")]
    [FoldoutGroup("Aura Effects/Type"), SerializeField, Tooltip("The type(s) of aura effect.")]
    public AuraEffectTypes AuraEffectType;

    [FoldoutGroup("Aura Effects/Stats"), SerializeField, Tooltip("The intensity of the aura effect."), ShowIf("IsIntensityMultiplier"), Min(0.1f)]
    public float AuraEffectIntensityMultiplier;
    [FoldoutGroup("Aura Effects/Stats"), SerializeField, Tooltip("The intensity of the aura effect."), HideIf("IsIntensityMultiplier")]
    public float AuraEffectIntensityValue;
    [FoldoutGroup("Aura Effects/Stats"), SerializeField, Tooltip("The tick rate of the aura effect."), ShowIf("@this.AuraEffectType == AuraEffectTypes.DamageOverTime || this.AuraEffectType == AuraEffectTypes.HealOverTime")]
    public float AuraEffectTickRate;
}
