using Game;

namespace Player
{
    public static class PlayerDataStorage
    {
        public static float BaseAttackSpeed;
        public static float BaseRangedDamage;
        public static float BaseMeleeDamage;
        public static float BaseSpeed;
        public static float BaseSuperCooldown;
        public static float BaseHealthRegenAmount;
        public static float BaseHealthRegenInterval;
        public static float BaseManaRegenAmount;
        public static float BaseManaRegenInterval;
        public static float BaseAuraRange;
        public static float BaseAuraDamage;
        public static float BaseAuraInterval;
        public static float BaseArmourValue;
        public static float AbilityDamage;
        public static float AbilityCooldown;
        public static float AbilityRange;
        public static float SuperDamage;
        public static float MagnetRange;
        public static float MagnetCooldown;
        
        
        public static float GetPlayerDataByType(PowerUpType _type)
        {
            switch (_type) {
                case PowerUpType.AttackSpeedBoost:
                    return BaseAttackSpeed;
                case PowerUpType.RangedDamageBoost:
                    return BaseRangedDamage;
                case PowerUpType.MeleeDamageBoost:
                    return BaseMeleeDamage;
                case PowerUpType.SpeedBoost:
                    return BaseSpeed;
                case PowerUpType.SuperCooldownReduction:
                    return BaseSuperCooldown;
                case PowerUpType.HealthRegenAmount:
                    return BaseHealthRegenAmount;
                case PowerUpType.HealthRegenTickRate:
                    return BaseHealthRegenInterval;
                case PowerUpType.ManaRegenAmount:
                    return BaseManaRegenAmount;
                case PowerUpType.ManaRegenTickRate:
                    return BaseManaRegenInterval;
                case PowerUpType.AuraDamageBoost:
                    return BaseAuraDamage;
                case PowerUpType.AuraRangeBoost:
                    return BaseAuraRange;
                default:
                    throw new System.ArgumentException("Invalid PowerUpType");
            }
        }

        public static void Initialize(float _attackSpeed, float _rangedDamage, float _meleeDamage, float _speed,
            float _superCooldown, float _healthRegenAmount, float _healthRegenInterval, float _manaRegenAmount,
            float _manaRegenInterval, float _auraRange, float _auraDamage, float _auraInterval, float _armourValue,
            float _abilityDamage, float _abilityCooldown, float _abilityRange, float _superDamage, float _magnetRange,
            float _magnetCooldown)
        {
            BaseAttackSpeed = _attackSpeed;
            BaseRangedDamage = _rangedDamage;
            BaseMeleeDamage = _meleeDamage;
            BaseSpeed = _speed;
            BaseSuperCooldown = _superCooldown;
            BaseHealthRegenAmount = _healthRegenAmount;
            BaseHealthRegenInterval = _healthRegenInterval;
            BaseManaRegenAmount = _manaRegenAmount;
            BaseManaRegenInterval = _manaRegenInterval;
            BaseAuraRange = _auraRange;
            BaseAuraDamage = _auraDamage;
            BaseAuraInterval = _auraInterval;
            BaseArmourValue = _armourValue;
            AbilityDamage = _abilityDamage;
            AbilityCooldown = _abilityCooldown;
            AbilityRange = _abilityRange;
            SuperDamage = _superDamage;
            MagnetRange = _magnetRange;
            MagnetCooldown = _magnetCooldown;
        }
    }
}
