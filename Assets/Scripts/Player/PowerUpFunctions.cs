using UnityEngine;
using System.Collections;
using Game;
using Managers;

namespace Player
{
    public partial class PlayerController
    {
        private int powerUpCount;
        
        public void ActivatePowerUp(PowerUpData _powerUp)
        {
            RunScoreManager.Instance.AddPowerUpScore(!Mathf.Approximately(_powerUp.Duration, 0f) ? 1 : 2);
            
            switch (_powerUp.Type)
            {
                case PowerUpType.Invincibility:
                    invincibilityTimer = invincibilityDuration;
                    break;
                case PowerUpType.DoublePoints:
                    RunScoreManager.Instance.ModifyPointMultiplier(_powerUp.Duration);
                    break;
                case PowerUpType.CoinMagnet:
                    StartCoroutine(CoinMagnetCoroutine(_powerUp.Duration));
                    break;
                case PowerUpType.AttackSpeedBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryAttackSpeedBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        AttackSpeedBuff *= _powerUp.Multiplier;
                    break;
                case PowerUpType.MeleeDamageBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryMeleeDamageBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        MeleeDamageBuff *= _powerUp.Multiplier;
                    break;
                case PowerUpType.RangedDamageBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryRangedDamageBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        RangedDamageBuff *= _powerUp.Multiplier;
                    break;

                case PowerUpType.HealthRegenTickRate:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryHealthRegenTickRateCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        HealthRegenIntervalBuff *= _powerUp.Multiplier;
                    break;
                case PowerUpType.HealthRegenAmount:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryHealthRegenAmountCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        HealthRegenAmountBuff *= _powerUp.Multiplier;
                    break;
                case PowerUpType.MaxHealthBoost:
                    float healthIncrease = (maxHealth + _powerUp.Amount) - maxHealth;
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryMaxHealthBoostCoroutine(healthIncrease, _powerUp.Duration));
                    else {
                        ChangeMaxHealth(healthIncrease);
                        ChangeHealth(healthIncrease); // Heal the player by the increase amount
                    }
                    break;
                case PowerUpType.HealAmount:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryHealthBoostCoroutine(_powerUp.Amount, _powerUp.Duration));
                    else
                        ChangeHealth(maxHealth + _powerUp.Amount);
                    break;
                case PowerUpType.ManaBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryManaBoostCoroutine(_powerUp.Amount, _powerUp.Duration));
                    else
                        ChangeMana(_powerUp.Amount);
                    break;
                case PowerUpType.MaxManaBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryMaxManaBoostCoroutine(_powerUp.Amount, _powerUp.Duration));
                    else
                        ChangeMaxMana(_powerUp.Amount);
                    break;
                case PowerUpType.ManaRegenAmount:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryManaRegenAmountCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        ManaRegenRateBuff *= _powerUp.Multiplier;
                    break;
                case PowerUpType.ManaRegenTickRate:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryManaIntervalCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        ManaRegenIntervalBuff *= _powerUp.Multiplier;
                    break;
                case PowerUpType.SpeedBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporarySpeedBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        SpeedBuff *= _powerUp.Multiplier;
                    break;
                case PowerUpType.AuraTickSpeedBoost:
                    if (_powerUp.Duration > 0)
                        playerAuraManager.ChangeAuraTickRate(_powerUp.Multiplier, _powerUp.Duration);
                    else
                        playerAuraManager.ChangeAuraTickRate(_powerUp.Multiplier);
                    break;
                case PowerUpType.AuraRangeBoost:
                    if (_powerUp.Duration > 0)
                        playerAuraManager.ChangeAuraRange(_powerUp.Multiplier, _powerUp.Duration);
                    else
                        playerAuraManager.ChangeAuraRange(_powerUp.Multiplier);
                    break;
                case PowerUpType.AuraDamageBoost:
                    if (_powerUp.Duration > 0)
                        playerAuraManager.ChangeAuraDamage(_powerUp.Multiplier, _powerUp.Duration);
                    else
                        playerAuraManager.ChangeAuraDamage(_powerUp.Multiplier);
                    break;
                case PowerUpType.SuperCooldownReduction:
                    SuperCooldownBuff -= _powerUp.Amount;
                    break;
                case PowerUpType.SuperDamageBoost:
                    SuperDamageBuff *= _powerUp.Multiplier;
                    break;
                default:
                    Debug.LogWarning("Unknown power-up type: " + _powerUp.Type);
                    break;
            }

            Debug.Log($"Activated Power-Up: {_powerUp.Type} with Duration: {_powerUp.Duration}, Multiplier: {_powerUp.Multiplier}, Amount: {_powerUp.Amount}");
        }


        private IEnumerator CoinMagnetCoroutine(float _duration)
        {

            yield return new WaitForSeconds(_duration);

        }

        private IEnumerator TemporaryAttackSpeedBoostCoroutine(float _multiplier, float _duration)
        {
            AttackSpeedBuffTemp *= _multiplier;
            yield return new WaitForSeconds(_duration);
            AttackSpeedBuffTemp /= _multiplier;
        }

        private IEnumerator TemporaryHealthBoostCoroutine(float _amount, float _duration)
        {
            allowOverHeal = true;
            ChangeHealth(_amount); // Heal the player
            yield return new WaitForSeconds(_duration);
            allowOverHeal = false;
            ChangeHealth(-_amount); // Reset health adjustment
        }

        private IEnumerator TemporaryHealthRegenAmountCoroutine(float _multiplier, float _duration)
        {
            HealthRegenAmountTempBuff *= _multiplier;
            yield return new WaitForSeconds(_duration);
            HealthRegenAmountTempBuff /= _multiplier;
        }

        private IEnumerator TemporaryHealthRegenTickRateCoroutine(float _multiplier, float _duration)
        {
            HealthRegenIntervalTempBuff *= _multiplier;
            yield return new WaitForSeconds(_duration);
            HealthRegenIntervalTempBuff /= _multiplier;
        }

        private IEnumerator TemporaryManaBoostCoroutine(float _amount, float _duration)
        {
            ChangeMana(_amount);
            yield return new WaitForSeconds(_duration);
            ChangeMana(-_amount);
        }

        private IEnumerator TemporaryManaIntervalCoroutine(float _multiplier, float _duration)
        {
            ManaRegenIntervalTempBuff *= _multiplier;
            yield return new WaitForSeconds(_duration);
            ManaRegenIntervalTempBuff /= _multiplier;
        }

        private IEnumerator TemporaryManaRegenAmountCoroutine(float _multiplier, float _duration)
        {
            ManaRegenRateTempBuff *= _multiplier;
            yield return new WaitForSeconds(_duration);
            ManaRegenRateTempBuff /= _multiplier;
        }

        private IEnumerator TemporaryMaxManaBoostCoroutine(float _amount, float _duration)
        {
            ChangeMaxMana(_amount);
            yield return new WaitForSeconds(_duration);
            ChangeMaxMana(-_amount);
        }

        private IEnumerator TemporaryMaxHealthBoostCoroutine(float _healthIncrease, float _duration)
        {
            ChangeMaxHealth(_healthIncrease);
            ChangeHealth(_healthIncrease); // Heal the player by the increase amount
            yield return new WaitForSeconds(_duration);
            ChangeHealth(-_healthIncrease, true); // Adjust current health if necessary
            ChangeMaxHealth(-_healthIncrease); // Reduce max health
        }

        private IEnumerator TemporaryMeleeDamageBoostCoroutine(float _multiplier, float _duration)
        {
            MeleeDamageBuffTemp *= _multiplier;
            yield return new WaitForSeconds(_duration);
            MeleeDamageBuffTemp /= _multiplier;
        }

        private IEnumerator TemporaryRangedDamageBoostCoroutine(float _multiplier, float _duration)
        {
            RangedDamageBuffTemp *= _multiplier;
            yield return new WaitForSeconds(_duration);
            RangedDamageBuffTemp /= _multiplier;
        }

        private IEnumerator TemporarySpeedBoostCoroutine(float _multiplier, float _duration)
        {
            SpeedBuffTemp *= _multiplier;
            yield return new WaitForSeconds(_duration);
            SpeedBuffTemp /= _multiplier;
        }
    }
}
