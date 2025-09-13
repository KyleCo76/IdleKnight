using UnityEngine;
using System.Collections;
using Game;

namespace Player
{
    public partial class PlayerController
    {
        private void ActivatePowerUp(PowerUpData _powerUp)
        {
            switch (_powerUp.Type)
            {
                case PowerUpType.Invincibility:
                    invincibilityTimer = invincibilityDuration;
                    break;
                case PowerUpType.DoublePoints:
                    RunScoreManager.Instance.ModifyPointMultiplier(_powerUp.Multiplier, _powerUp.Duration);
                    break;
                case PowerUpType.CoinMagnet:
                    StartCoroutine(CoinMagnetCoroutine(_powerUp.Duration));
                    break;
                case PowerUpType.AttackSpeedBoost:
                    attackCooldown /= _powerUp.Multiplier;
                    break;
                case PowerUpType.TempAttackSpeedBoost:
                    StartCoroutine(TemporaryAttackSpeedBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    break;
                case PowerUpType.MeleeDamageBoost:
                    meleeDamage *= _powerUp.Multiplier;
                    break;
                case PowerUpType.TempMeleeDamageBoost:
                    StartCoroutine(TemporaryMeleeDamageBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    break;
                case PowerUpType.RangedDamageBoost:
                    rangedDamage *= _powerUp.Multiplier;
                    break;
                case PowerUpType.TempRangedDamageBoost:
                    StartCoroutine(TemporaryRangedDamageBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    break;

                case PowerUpType.HealthRegenTickRate:
                    healthRegenInterval /= _powerUp.Multiplier;
                    break;
                case PowerUpType.TempHealthRegenTickRate:
                    StartCoroutine(TemporaryHealthRegenTickRateCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    break;
                case PowerUpType.HealthRegenAmount:
                    healthRegenAmount *= _powerUp.Multiplier;
                    break;
                case PowerUpType.TempHealthRegenAmount:
                    StartCoroutine(TemporaryHealthRegenAmountCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    break;
                case PowerUpType.MaxHealthBoost:
                    float healthIncrease = (maxHealth * _powerUp.Multiplier) - maxHealth;
                    ChangeMaxHealth(healthIncrease);
                    ChangeHealth(healthIncrease); // Heal the player by the increase amount
                    break;
                case PowerUpType.TempMaxHealthBoost:
                    float tempHealthIncrease = (maxHealth * _powerUp.Multiplier) - maxHealth;
                    StartCoroutine(TemporaryMaxHealthBoostCoroutine(tempHealthIncrease, _powerUp.Duration));
                    break;
                case PowerUpType.HealAmount:
                    ChangeHealth(maxHealth * _powerUp.Multiplier); // Heal by a percentage of max health
                    break;

                case PowerUpType.SpeedBoost:
                    StartCoroutine(SpeedBoostCoroutine());
                    break;
                case PowerUpType.TempSpeedBoost:
                    StartCoroutine(TemporarySpeedBoostCoroutine(_powerUp.Multiplier, _powerUp.Duration));
                    break;
                case PowerUpType.AuraTickSpeedBoost:
                    playerAuraManager.ChangeAuraTickRate(_powerUp.Multiplier);
                    break;
                case PowerUpType.TempAuraTickSpeedBoost:
                    playerAuraManager.ChangeAuraTickRate(_powerUp.Multiplier, _powerUp.Duration);
                    break;
                case PowerUpType.AuraRangeBoost:
                    playerAuraManager.ChangeAuraRange(_powerUp.Multiplier);
                    break;
                case PowerUpType.TempAuraRangeBoost:
                    playerAuraManager.ChangeAuraRange(_powerUp.Multiplier, _powerUp.Duration);
                    break;
                case PowerUpType.AuraDamageBoost:
                    playerAuraManager.ChangeAuraDamage(_powerUp.Multiplier);
                    break;
                case PowerUpType.TempAuraDamageBoost:
                    playerAuraManager.ChangeAuraDamage(_powerUp.Multiplier, _powerUp.Duration);
                    break;
                default:
                    Debug.LogWarning("Unknown power-up type: " + _powerUp.Type);
                    break;
            }
        }


        private IEnumerator CoinMagnetCoroutine(float duration)
        {
            //float originalMagnetRadius = coinMagnetRadius;
            //coinMagnetRadius = 10f; // Example increased radius
            yield return new WaitForSeconds(duration);
            //coinMagnetRadius = originalMagnetRadius; // Reset to original radius
        }

        private IEnumerator SpeedBoostCoroutine()
        {
            float originalSpeed = movementSpeed;
            movementSpeed *= 2; // Double the speed
            yield return new WaitForSeconds(5); // Duration of the power-up
            movementSpeed = originalSpeed; // Reset to original speed
        }

        private IEnumerator TemporaryAttackSpeedBoostCoroutine(float _multiplier, float _duration)
        {
            float originalCooldown = attackCooldown;
            attackCooldown /= _multiplier; // Increase attack speed
            yield return new WaitForSeconds(_duration);
            attackCooldown = originalCooldown; // Reset to original cooldown
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
