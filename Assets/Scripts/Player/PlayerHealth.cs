using Managers;
using UnityEngine;

namespace Player
{
    public partial class PlayerController
    {
        [SerializeField, Tooltip("The maximum health of the player")]
        private float maxHealth = 100f;
        [SerializeField, Tooltip("The length of time in seconds the player is invincible after taking damage")]
        private float invincibilityDuration = 1.0f;
        [SerializeField, Tooltip("The time in seconds between health regeneration ticks")]
        private float healthRegenInterval = 5.0f;
        [SerializeField, Tooltip("The amount of health regenerated each tick")]
        private float healthRegenAmount = 2.0f;

        private float currentHealth;
        private float invincibilityTimer;
        private float healthRegenTimer;
        private bool allowOverHeal;


        private void HealthAwake()
        {
            currentHealth = maxHealth;
            UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
            healthRegenTimer = healthRegenInterval;
        }

        private void HealthUpdate()
        {
            if (invincibilityTimer > 0f) {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0f)
                    playerAnimatorHelper.SetHurt(false);
            }
            if (currentHealth < maxHealth && invincibilityTimer <= 0f) {
                healthRegenTimer -= Time.deltaTime;
                if (healthRegenTimer <= 0f) {
                    ChangeHealth(healthRegenAmount);
                    healthRegenTimer = healthRegenInterval;
                }
            }
        }

        public void ChangeHealth(float _amount, bool _ignoreHurt = false)
        {
            if (_amount < 0 && !_ignoreHurt) {
                if (invincibilityTimer > 0f)
                    return; // Ignore damage if invincible

                invincibilityTimer = invincibilityDuration; // Reset invincibility timer
                playerAnimatorHelper.SetHurt(true); // Animate player as if taking damage;
            }

            currentHealth += _amount;
            if (!allowOverHeal)
                currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            if (currentHealth <= 0f) {
                Debug.Log($"{gameObject.name} has died.");
                // Handle player death (e.g., trigger game over, respawn, etc.)
                return;
            }
            UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }

        private void ChangeMaxHealth(float _amount)
        {
            maxHealth += _amount;
            maxHealth = Mathf.Max(1f, maxHealth); // Ensure max health is at least 1
            currentHealth = Mathf.Min(currentHealth, maxHealth); // Adjust current health if necessary
            UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }
    }
}