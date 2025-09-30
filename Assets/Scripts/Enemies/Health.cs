using UnityEngine;
using Game;
using Managers;
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
            if (!GameManager.Instance || GameManager.Instance.IsPaused || isDead)
                return;
            
            if (shieldHealth > 0f && _amount < 0f && !_ignoreShield) {
                shieldHealth += _amount;
                if (shieldHealth < 0f) {
                    _amount = shieldHealth; // Remaining damage after the shield is depleted
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
                Vector2 position = transform.position;
                deathShotType = _attackType;
                isDead = true;
                
                if (TryGetComponent(out Collider2D enemyCollider))
                    enemyCollider.enabled = false;
                
                if (hasDeathAnimation) {
                    enemyAnimator.SetTrigger(animatorHashes["Die"]);
                    return;
                }
                Die(position);
            }
        }

        public void Die(Vector2 _position)
        {
            if (isPooled) {
                parentSpawner.ReleaseMinion(this.gameObject);
                return;
            }
            
            var handlers = OnEnemyDeath;
            if (handlers != null) {
                foreach (var d in handlers.GetInvocationList()) {
                    try {
                        ((System.Action<AttackType, int, float, Vector2, GameObject>)d).Invoke(deathShotType, deathValue, itemSpawnChance, _position, this.gameObject);
                    }
                    catch (System.Exception ex) {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        
        public void ResetHealth()
        {
            isDead = false;
            currentHealth = maxHealth;
        }
    }
}