
namespace Game
{
    public class PowerUpData
    {
        public PowerUpType Type { get; private set; }
        public float Duration { get; private set; }
        public float Multiplier { get; private set; }
        public float Amount { get; private set; }
        public PowerUpData(PowerUpType type, float duration = 0f, float multiplier = 1f, float amount = 0f)
        {
            Type = type;
            Duration = duration;
            Multiplier = multiplier;
            Amount = amount;
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
        RangedDamageBoost,
        MeleeDamageBoost,
        HealthRegenTickRate,
        HealthRegenAmount,
        MaxHealthBoost,
        HealAmount,
        ManaRegenTickRate,
        ManaRegenAmount,
        ManaBoost,
        MaxManaBoost,
        SpeedBoost,
        AuraTickSpeedBoost,
        AuraRangeBoost,
        AuraDamageBoost,
    }
}
