using UnityEngine;
using Game;

namespace Enemies
{
    public partial class Controller : MonoBehaviour
    {
        [SerializeField, Tooltip("The maximum amount of health the enemy has")]
        private float maxHealth = 10f;

        private float shieldHealth = 0f;
        private float currentHealth;
        private bool isDead = false;


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
            currentHealth += _amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth <= 0) {
                Die(_attackType);
            }
        }

        private void Die(AttackType _attackType)
        {
            if (isDead) return;
            isDead = true;
            OnEnemyDeath?.Invoke(_attackType, deathValue, itemSpawnChance, transform.position, gameObject);
            Destroy(gameObject);
        }
    }
}