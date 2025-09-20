using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies
{
    public class EnemyAuraEffects : MonoBehaviour
    {
        [FoldoutGroup("Settings"), SerializeField, Tooltip("The radius of the aura effect.")]
        private float auraRadius;

        [SerializeField, Tooltip("The effects of the aura.")]
        private AuraEffect[] auraEffects;

        private bool damagePlayer;
        private float playerTickTimer;
        private float enemyTickTimer;


        // Cached Components
        private readonly List<Controller> affectedEnemies = new();
        private Player.PlayerController playerController;


        void Start()
        {
            var auraCollider = GetComponentInChildren<CircleCollider2D>();
            if (!auraCollider) {
                GameObject auraObjectCollider = new("AuraCollider");
                auraObjectCollider.transform.SetParent(transform);
                auraObjectCollider.layer = LayerMask.NameToLayer("EnemyAura");
                auraCollider = auraObjectCollider.AddComponent<CircleCollider2D>();
            }
            auraCollider.isTrigger = true;
            auraCollider.radius = auraRadius;

            if (System.Array.Exists(auraEffects, _effect => _effect.AuraEffectType == AuraEffectTypes.DamageOverTime)) {
                damagePlayer = true;
            }
        }

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

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (_other.TryGetComponent<Controller>(out var enemy)) {
                if (!affectedEnemies.Contains(enemy)) {
                    affectedEnemies.Add(enemy);
                    ApplyAuraEffects(enemy);
                }
            } else if (_other.TryGetComponent<Player.PlayerController>(out var player)) {
                playerController = player;
                ApplyAuraEffects(playerController);
            }
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            if (_other.TryGetComponent<Controller>(out var enemy)) {
                if (affectedEnemies.Contains(enemy)) {
                    RemoveAuraEffects(enemy);
                    affectedEnemies.Remove(enemy);
                } else if (_other.TryGetComponent<Player.PlayerController>(out var player)) {
                    if (playerController == player) {
                        playerController = null;
                        RemoveAuraEffects(player);
                    }
                }
            }
        }


        private void ApplyAuraEffects(Controller _enemy)
        {
            foreach (var effect in auraEffects)
            {
                switch (effect.AuraEffectType)
                {
                    case AuraEffectTypes.SpeedBoost:
                        _enemy.ApplySpeedBoost(effect.AuraEffectIntensityMultiplier);
                        break;
                    case AuraEffectTypes.Shield:
                        _enemy.ApplyShield(effect.AuraEffectIntensityValue);
                        break;
                }
            }
        }

        private void ApplyAuraEffects(Player.PlayerController _player)
        {
            foreach (var effect in auraEffects)
            {
                if (effect.AuraEffectType == AuraEffectTypes.Slow) {
                    _player.ApplySlow(effect.AuraEffectIntensityMultiplier);
                }
            }
        }

        private void RemoveAuraEffects(Controller _enemy)
        {
            foreach (var effect in auraEffects)
            {
                if (effect.AuraEffectType == AuraEffectTypes.SpeedBoost) {
                    _enemy.RemoveSpeedBoost();
                }
            }
        }

        private void RemoveAuraEffects(Player.PlayerController _player)
        {
            foreach (var effect in auraEffects)
            {
                if (effect.AuraEffectType == AuraEffectTypes.Slow) {
                    _player.RemoveSlow();
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
}