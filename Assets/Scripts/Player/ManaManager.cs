using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player
{
    public partial class PlayerController
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

        // Public getters for player stats
        public float BaseManaRegenRate => manaRegenRate;
        public float ManaRegenRateBuff { get; private set; }
        public float ManaRegenRateTempBuff { get; private set; }
        public float BaseManaRegenInterval => manaRegenInterval;
        public float ManaRegenIntervalBuff { get; private set; }
        public float ManaRegenIntervalTempBuff { get; private set; }
        
        private float currentMana;
        private float manaRegenTimer;


        void StaminaAwake()
        {
            currentMana = startingMana;
            UIManager.Instance.UpdateManaUI(currentMana, maxMana);
        }

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

        private bool ChangeMana(float _amount)
        {
            if (currentMana + _amount < 0f) {
                return false; // Not enough mana
            }
            currentMana = Mathf.Clamp(currentMana + _amount, 0f, maxMana);
            UIManager.Instance.UpdateManaUI(currentMana, maxMana);
            return true;
        }
    }
}