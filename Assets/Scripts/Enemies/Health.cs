using UnityEngine;
using Game;

namespace Enemies
{
    public partial class Controller : MonoBehaviour
    {
        [SerializeField, Tooltip("The maximum amount of health the enemy has")]
        private float maxHealth = 10f;

        private float currentHealth;
        private bool isDead = false;

        public void TakeDamage(float _amount, AttackType _attackType = AttackType.None)
        {
            currentHealth -= _amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth <= 0) {
                Die(_attackType);
            }
        }

        private void Die(AttackType _attackType)
        {
            if (isDead) return;
            isDead = true;
            OnEnemyDeath?.Invoke(_attackType, deathValue, itemSpawnChance, transform.position);
            Destroy(gameObject);
        }
    }
}