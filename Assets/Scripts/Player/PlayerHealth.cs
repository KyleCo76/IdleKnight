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

        // Public getters for player stats
        public float BaseHealthRegenAmount { get; private set; }
        public float HealthRegenAmountBuff { get; private set; }
        public float HealthRegenAmountTempBuff { get; private set; }
        public float BaseHealthRegenInterval { get; private set; }
        public float HealthRegenIntervalBuff { get; private set; }
        public float HealthRegenIntervalTempBuff { get; private set; }
        
        private float currentHealth;
        private float invincibilityTimer;
        private float healthRegenTimer;
        private bool allowOverHeal;
        private bool isDead;


        private void HealthAwake()
        {
            BaseHealthRegenAmount = PlayerDataStorage.BaseHealthRegenAmount;
            BaseHealthRegenInterval = PlayerDataStorage.BaseHealthRegenInterval;
            HealthRegenAmountBuff = 1f;
            HealthRegenAmountTempBuff = 1f;
            HealthRegenIntervalBuff = 1f;
            HealthRegenIntervalTempBuff = 1f;
            currentHealth = maxHealth;
        }

        private void HealthStart()
        {
            UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
            healthRegenTimer = BaseHealthRegenInterval / HealthRegenAmountBuff / HealthRegenAmountTempBuff;
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
                    ChangeHealth(BaseHealthRegenAmount * HealthRegenAmountBuff * HealthRegenAmountTempBuff);
                    healthRegenTimer = BaseHealthRegenInterval / HealthRegenAmountBuff / HealthRegenAmountTempBuff;
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
                PlayerDied();
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

        private void PlayerDied()
        {
            if (!GameManager.Instance) {
                Debug.LogError("GameManager instance missing from scene");
                return;
            }

            currentHealth = 0f;
            UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
            isDead = true;
            GameManager.Instance.PlayerDied();
        }
    }
}