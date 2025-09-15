using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public partial class PlayerController : MonoBehaviour
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
        private float invincibilityTimer = 0f;
        private float healthRegenTimer = 0f;
        private bool allowOverHeal = false;

        // Cached components
        private Slider healthBubble;
        private TextMeshProUGUI healthText;


        private void HealthAwake()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) {
                Debug.LogError("No Canvas GameObject found in the scene.");
                enabled = false;
                return;
            }
            
            var healthBubbleObject = canvas.transform.Find("HealthBubble");
            if (healthBubbleObject == null) {
                Debug.LogError("No HealthBubble GameObject found under Canvas.");
                enabled = false;
                return;
            }
            if (!healthBubbleObject.TryGetComponent<Slider>(out healthBubble)) {
                Debug.LogError("No HealthBubble GameObject found under Canvas.");
                enabled = false;
                return;
            }

            healthText = healthBubbleObject.GetComponentInChildren<TextMeshProUGUI>();
            if (healthText == null) {
                Debug.LogError("No TextMeshProUGUI component found on HealthText GameObject.");
                enabled = false;
                return;
            }

            currentHealth = maxHealth;
            UpdateHealthUI();
            healthRegenTimer = healthRegenInterval;
        }

        private void HealthUpdate()
        {
            if (invincibilityTimer > 0f) {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0f)
                    playerAnimator.SetBool("isHurt", false);
            }
            if (currentHealth < maxHealth) {
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
                playerAnimator.SetBool("isHurt", true);
            }

            currentHealth += _amount;
            if (!allowOverHeal)
                currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            if (currentHealth <= 0f) {
                Debug.Log($"{gameObject.name} has died.");
                // Handle player death (e.g., trigger game over, respawn, etc.)
                return;
            }
            UpdateHealthUI();
        }

        private void ChangeMaxHealth(float _amount)
        {
            maxHealth += _amount;
            maxHealth = Mathf.Max(1f, maxHealth); // Ensure max health is at least 1
            currentHealth = Mathf.Min(currentHealth, maxHealth); // Adjust current health if necessary
            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            if (healthBubble == null) {
                Debug.LogError("healthBubble is null in PlayerHealth");
                return;
            }
            if (!allowOverHeal)
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            healthBubble.value = Mathf.Clamp01(currentHealth / maxHealth);
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
        }
    }
}