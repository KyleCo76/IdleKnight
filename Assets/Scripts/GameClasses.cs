
namespace Game
{
    public class PowerUpData
    {
        public PowerUpType Type { get; private set; }
        public float Duration { get; private set; }
        public float Multiplier { get; private set; }
        public float Amount { get; private set; }
        public PowerUpData(PowerUpType _type, float _duration = 0f, float _multiplier = 1f, float _amount = 0f)
        {
            Type = _type;
            Duration = _duration;
            Multiplier = _multiplier;
            Amount = _amount;
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
        SuperCooldownReduction,
        SuperDamageBoost
    }

    public enum SuperType
    {
        None,
        BlobLarge,
        BlobSmall,
        ElectricLarge,
        ElectricSmall,
        EnergyLarge,
        EnergySmall,
        FireBallLarge,
        FireBallSmall,
        LaserLarge,
        LaserSmall,
        MagicMissileLarge,
        MagicMissileSmall,
        RockLarge,
        RockSmall,
        SlashLarge,
        SlashSmall,
    }
}
