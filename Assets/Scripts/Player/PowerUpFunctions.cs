using UnityEngine;
using System.Collections;
using Game;

namespace Player
{
    public partial class PlayerController
    {
        public void ActivatePowerUp(PowerUpData _powerUp)
        {
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
                        attackCooldown /= _powerUp.Multiplier;
                    break;
                case PowerUpType.MeleeDamageBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryMeleeDamageBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        meleeDamage *= _powerUp.Multiplier;
                    break;
                case PowerUpType.RangedDamageBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryRangedDamageBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        rangedDamage *= _powerUp.Multiplier;
                    break;

                case PowerUpType.HealthRegenTickRate:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryHealthRegenTickRateCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        healthRegenInterval /= _powerUp.Multiplier;
                    break;
                case PowerUpType.HealthRegenAmount:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporaryHealthRegenAmountCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        healthRegenAmount *= _powerUp.Multiplier;
                    break;
                case PowerUpType.MaxHealthBoost:
                    float healthIncrease = (maxHealth * _powerUp.Amount) - maxHealth;
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
                        ChangeHealth(maxHealth * _powerUp.Amount); // Heal by a percentage of max health
                    break;

                case PowerUpType.SpeedBoost:
                    if (_powerUp.Duration > 0)
                        StartCoroutine(TemporarySpeedBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    else
                        movementSpeed *= _powerUp.Multiplier;
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
                default:
                    Debug.LogWarning("Unknown power-up type: " + _powerUp.Type);
                    break;
            }

            Debug.Log($"Activated Power-Up: {_powerUp.Type} with Duration: {_powerUp.Duration}, Multiplier: {_powerUp.Multiplier}, Amount: {_powerUp.Amount}");
        }


        private IEnumerator CoinMagnetCoroutine(float duration)
        {
            //float originalMagnetRadius = coinMagnetRadius;
            //coinMagnetRadius = 10f; // Example increased radius
            yield return new WaitForSeconds(duration);
            //coinMagnetRadius = originalMagnetRadius; // Reset to original radius
        }

        private IEnumerator TemporaryAttackSpeedBoostCoroutine(float _multiplier, float _duration)
        {
            float originalCooldown = attackCooldown;
            attackCooldown /= _multiplier; // Increase attack speed
            yield return new WaitForSeconds(_duration);
            attackCooldown = originalCooldown; // Reset to original cooldown
        }

        private IEnumerator TemporaryHealthBoostCoroutine(float _amount, float _duration)
        {
            ChangeHealth(_amount); // Heal the player
            yield return new WaitForSeconds(_duration);
            ChangeHealth(-_amount); // Reset health adjustment
        }

        private IEnumerator TemporaryHealthRegenAmountCoroutine(float _multiplier, float _duration)
        {
            float originalAmount = healthRegenAmount;
            healthRegenAmount *= _multiplier; // Increase regen amount
            yield return new WaitForSeconds(_duration);
            healthRegenAmount = originalAmount; // Reset to original amount
        }

        private IEnumerator TemporaryHealthRegenTickRateCoroutine(float _multiplier, float _duration)
        {
            float originalInterval = healthRegenInterval;
            healthRegenInterval /= _multiplier; // Increase tick rate
            yield return new WaitForSeconds(_duration);
            healthRegenInterval = originalInterval; // Reset to original interval
        }

        private IEnumerator TemporaryMaxHealthBoostCoroutine(float _healthIncrease, float _duration)
        {
            ChangeMaxHealth(_healthIncrease);
            ChangeHealth(_healthIncrease); // Heal the player by the increase amount
            yield return new WaitForSeconds(_duration);
            ChangeHealth(-_healthIncrease); // Adjust current health if necessary
            ChangeMaxHealth(-_healthIncrease); // Reduce max health
        }

        private IEnumerator TemporaryMeleeDamageBoostCoroutine(float _multiplier, float _duration)
        {
            float originalDamage = meleeDamage;
            meleeDamage *= _multiplier; // Increase melee damage
            yield return new WaitForSeconds(_duration);
            meleeDamage = originalDamage; // Reset to original damage
        }

        private IEnumerator TemporaryRangedDamageBoostCoroutine(float _multiplier, float _duration)
        {
            float originalDamage = rangedDamage;
            rangedDamage *= _multiplier; // Increase ranged damage
            yield return new WaitForSeconds(_duration);
            rangedDamage = originalDamage; // Reset to original damage
        }

        private IEnumerator TemporarySpeedBoostCoroutine(float _multiplier, float _duration)
        {
            float originalSpeed = movementSpeed;
            movementSpeed *= _multiplier; // Increase speed
            yield return new WaitForSeconds(_duration);
            movementSpeed = originalSpeed; // Reset to original speed
        }
    }
}
