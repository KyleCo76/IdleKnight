using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public partial class PlayerController : MonoBehaviour
    {
        [FoldoutGroup("Mana Settings"), SerializeField, Tooltip("The maximum mana the player can have.")]
        private float maxMana = 100f;
        [FoldoutGroup("Mana Settings"), SerializeField, Tooltip("The starting mana the player has.")]
        private float startingMana = 50f;
        [FoldoutGroup("Mana Settings"), SerializeField, Tooltip("The rate at which mana regenerates per tick.")]
        private float manaRegenRate = 1f;
        [FoldoutGroup("Mana Settings"), SerializeField, Tooltip("The interval in seconds between each mana regeneration tick.")]
        private float manaRegenInterval = 5f;
        [FoldoutGroup("Mana Settings"), SerializeField, Tooltip("The amount of mana consumed when performing a special attack.")]
        private float specialAttackManaCost = 20f;
        [FoldoutGroup("Mana Settings"), SerializeField, Tooltip("The amount of mana consumed per tick while sprinting.")]
        private float sprintManaCostPerTick = 2f;

        private float currentMana;
        private float manaRegenTimer;

        // Cached Components
        private Slider manaBubble;
        private TextMeshProUGUI manaText;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void StaminaAwake()
        {
            var canvas = GameObject.Find("Canvas");
            var manaBubbleObject = canvas.transform.Find("ManaBubble");
            if (manaBubbleObject == null) {
                Debug.LogError("No ManaBubble GameObject found under Canvas.");
                enabled = false;
                return;
            }
            if (!manaBubbleObject.TryGetComponent<Slider>(out manaBubble)) {
                Debug.LogError("No Slider component found on ManaBubble GameObject.");
                enabled = false;
                return;
            }
            manaText = manaBubbleObject.GetComponentInChildren<TextMeshProUGUI>();
            if (manaText == null) {
                Debug.LogError("No TextMeshProUGUI component found on ManaText GameObject.");
                enabled = false;
                return;
            }

            currentMana = startingMana;
            manaBubble.value = currentMana / maxMana;
            manaText.text = $"{currentMana}/{maxMana}";
        }

        // Update is called once per frame
        void StaminaUpdate()
        {
            if (manaRegenTimer < manaRegenInterval) {
                manaRegenTimer += Time.deltaTime;
            } else {
                if (sprintPressed && moveInput.magnitude > 0.1f) {
                    ChangeMana(-sprintManaCostPerTick);
                } else
                    RegenerateMana();
                manaRegenTimer = 0f;
            }
        }


        private void RegenerateMana()
        {
            ChangeMana(manaRegenRate);
        }

        private bool ChangeMana(float amount)
        {
            if (currentMana + amount < 0f) {
                return false; // Not enough mana
            }
            currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
            UpdateManaUI();
            return true;
        }

        private void UpdateManaUI()
        {
            manaBubble.value = currentMana / maxMana;
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }
}