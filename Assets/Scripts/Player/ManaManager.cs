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


        private void StaminaAwake()
        {
            currentMana = startingMana;
            ManaRegenRateBuff = 1f;
            ManaRegenRateTempBuff = 1f;
            ManaRegenIntervalBuff = 1f;
            ManaRegenIntervalTempBuff = 1f;
            manaRegenTimer = BaseManaRegenInterval / ManaRegenRateBuff / ManaRegenRateTempBuff;
        }

        private void StaminaStart()
        {
            UIManager.Instance.UpdateManaUI(currentMana, maxMana);
        }
        private void StaminaUpdate()
        {
            if (manaRegenTimer > 0f) {
                manaRegenTimer -= Time.deltaTime;
            } else {
                if (sprintPressed && moveInput.magnitude > 0.1f) {
                    ChangeMana(-sprintManaCostPerTick);
                } else
                    RegenerateMana();
                manaRegenTimer = BaseManaRegenInterval / ManaRegenRateBuff / ManaRegenRateTempBuff;
            }
        }


        private void ChangeMana(float _amount)
        {
            currentMana = Mathf.Clamp(currentMana + _amount, 0f, maxMana);
            UIManager.Instance.UpdateManaUI(currentMana, maxMana);
        }

        private void ChangeMaxMana(float _amount)
        {
            maxMana += _amount;
            UIManager.Instance.UpdateManaUI(currentMana, maxMana);
        }

        private void RegenerateMana()
        {
            ChangeMana(BaseManaRegenRate * ManaRegenRateBuff * ManaRegenRateTempBuff);
        }

        private bool TryChangeMana(float _amount)
        {
            if (currentMana + _amount < 0f) {
                return false; // Not enough mana
            }
            ChangeMana(_amount);
            return true;
        }
    }
}