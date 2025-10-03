using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Player
{
    public class AuraManager : MonoBehaviour
    {
        private float baseAuraDamage;
        private float auraDamageBuff = 1f;
        private float auraDamageBuffTemp = 1f;

        private float baseAuraDamageInterval;
        private float auraDamageIntervalBuff = 1f;
        private float auraDamageIntervalBuffTemp = 1f;

        private const float BaseAuraRange = 1f;
        private float auraRangeBuff = 1f;
        private float auraRangeBuffTemp = 1f;
        
        private float damageTimer; // Timer to track damage application
        private readonly HashSet<Enemies.Controller> enemiesInAura = new();

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (_other.CompareTag("Enemy")) {
                if (_other.TryGetComponent<Enemies.Controller>(out var enemy)) {
                    enemiesInAura.Add(enemy);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            if (_other.CompareTag("Enemy")) {
                if (_other.TryGetComponent<Enemies.Controller>(out var enemy)) {
                    enemiesInAura.Remove(enemy);
                }
            }
        }

        void Start()
        {
            baseAuraDamage = PlayerDataStorage.BaseAuraDamage;
            baseAuraDamageInterval = PlayerDataStorage.BaseAuraInterval;
            
            damageTimer = baseAuraDamageInterval / auraDamageIntervalBuff / auraDamageIntervalBuffTemp;
        }

        void Update()
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0.0f) {
                foreach (var enemy in new List<Enemies.Controller>(enemiesInAura)) {
                    if (enemy != null) {
                        enemy.ChangeHealth(-baseAuraDamage * auraDamageBuff * auraDamageBuffTemp);
                    }
                }
                damageTimer = baseAuraDamageInterval / auraDamageIntervalBuff / auraDamageIntervalBuffTemp;
            }
        }


        public void ChangeAuraDamage(float _multiplier)
        {
            auraDamageBuff *= _multiplier;
        }

        public void ChangeAuraDamage(float _multiplier, float _duration)
        {
            StartCoroutine(TemporaryAuraDamageCoroutine(_multiplier, _duration));
        }

        public void ChangeAuraRange(float _multiplier)
        {
            auraRangeBuff *= _multiplier;
            transform.localScale *= _multiplier;
        }

        public void ChangeAuraRange(float _multiplier, float _duration)
        {
            StartCoroutine(TemporaryAuraRangeCoroutine(_multiplier, _duration));
        }

        public void ChangeAuraTickRate(float _multiplier)
        {
            auraDamageIntervalBuff *= _multiplier;
        }

        public void ChangeAuraTickRate(float _multiplier, float _duration)
        {
            StartCoroutine(TemporaryAuraTickRateCoroutine(_multiplier, _duration));
        }

        public float3 GetDamageStats()
        {
            return new float3(baseAuraDamage, auraDamageBuff, auraDamageBuffTemp);
        }

        public float3 GetIntervalStats()
        {
            return new float3(baseAuraDamageInterval, auraDamageIntervalBuff, auraDamageIntervalBuffTemp);
        }

        public float3 GetRangeStats()
        {
            return new float3(BaseAuraRange, auraRangeBuff, auraRangeBuffTemp);
        }

        private IEnumerator TemporaryAuraDamageCoroutine(float _multiplier, float _duration)
        {
            auraDamageBuffTemp *= _multiplier;
            yield return new WaitForSeconds(_duration);
            auraDamageBuffTemp /= _multiplier;
        }

        private IEnumerator TemporaryAuraRangeCoroutine(float _multiplier, float _duration)
        {
            auraRangeBuffTemp *= _multiplier;
            Vector3 originalRadius = transform.localScale;
            transform.localScale *= _multiplier; // Increase range
            yield return new WaitForSeconds(_duration);
            transform.localScale = originalRadius; // Reset to original radius\
            auraRangeBuffTemp /= _multiplier;
        }

        private IEnumerator TemporaryAuraTickRateCoroutine(float _multiplier, float _duration)
        {
            auraDamageIntervalBuffTemp *= _multiplier;
            yield return new WaitForSeconds(_duration);
            auraDamageIntervalBuffTemp /= _multiplier;
        }
    }
}
