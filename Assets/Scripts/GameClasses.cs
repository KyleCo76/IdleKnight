
namespace Game
{
    public class PowerUpData
    {
        public PowerUpType Type { get; private set; }
        public float Duration { get; private set; }
        public float Multiplier { get; private set; }
        public PowerUpData(PowerUpType type, float duration = 0f, float multiplier = 1f)
        {
            Type = type;
            Duration = duration;
            Multiplier = multiplier;
        }
    }

    public enum AttackType
    {
        None,
        PlayerAttack,
        Environment,
        Other
    }

    public enum PowerUpType
    {
        None,
        Invincibility,
        DoublePoints,
        CoinMagnet,
        AttackSpeedBoost,
        TempAttackSpeedBoost,
        MeleeDamageBoost,
        TempMeleeDamageBoost,
        RangedDamageBoost,
        TempRangedDamageBoost,
        Shield,
        TempShield,
        HealthRegenTickRate,
        TempHealthRegenTickRate,
        HealthRegenAmount,
        TempHealthRegenAmount,
        MaxHealthBoost,
        TempMaxHealthBoost,
        HealAmount,
        ManaRegen,
        TempManaRegen,
        ManaBoost,
        TempManaBoost,
        SpeedBoost,
        TempSpeedBoost,
        AuraTickSpeedBoost,
        TempAuraTickSpeedBoost,
        AuraRangeBoost,
        TempAuraRangeBoost,
        AuraDamageBoost,
        TempAuraDamageBoost
    }
}
