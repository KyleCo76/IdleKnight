using UnityEngine;
using Game;
using Sirenix.OdinInspector;

namespace Enemies
{
    public partial class Controller
    {
        [FoldoutGroup("Health Settings"), SerializeField, Tooltip("The maximum amount of health the enemy has")]
        private float maxHealth = 10f;
        [FoldoutGroup("Health Settings"), SerializeField, Tooltip("The amount of resistance the enemy has the player's basic attack")]
        private float armourValue;

        private float shieldHealth;
        [SerializeField]
        private float currentHealth;
        private bool isDead;
        private AttackType deathShotType;


        public void ApplyShield(float _value)
        {
            shieldHealth += _value;
        }

        public void ChangeHealth(float _amount, AttackType _attackType = AttackType.None, bool _ignoreShield = false)
        {
            if (shieldHealth > 0f && _amount < 0f && !_ignoreShield) {
                shieldHealth += _amount;
                if (shieldHealth < 0f) {
                    _amount = shieldHealth; // Remaining damage after shield is depleted
                    shieldHealth = 0f;
                } else {
                    return;
                }
            }
            if (armourValue > 0f && _amount < 0f && _attackType == AttackType.PlayerAttack) { // Only apply armour to player attacks, not environmental damage or aura
                _amount += armourValue;
                if (_amount > 0f)
                    return; // Prevent healing from armour
            }
            currentHealth += _amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth <= 0) {
                deathShotType = _attackType;
                if (hasDeathAnimation) {
                    enemyAnimator.SetTrigger(animatorHashes["Die"]);
                    return;
                }
                Die();
            }
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;
            if (hasDeathAnimation)
                enemyAnimator.SetTrigger(animatorHashes["Die"]);
            if (isMinion) {
                parentSpawner.ReleaseMinion(this.gameObject);
                return;
            }
            OnEnemyDeath?.Invoke(deathShotType, deathValue, itemSpawnChance, transform.position, this.gameObject);
        }
        
        public void ResetHealth()
        {
            isDead = false;
            currentHealth = maxHealth;
        }
    }
}