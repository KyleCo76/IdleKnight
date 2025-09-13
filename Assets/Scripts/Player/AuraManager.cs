using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AuraManager : MonoBehaviour
{
    [SerializeField, Tooltip("Time interval between damage ticks")]
    private float damageSpeed = 1.0f;
    [SerializeField, Tooltip("Amount of damage dealt by the aura")]
    private float damageAmount = 1.0f;

    private float damageTimer = 0.0f; // Timer to track damage application
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
        damageTimer = damageSpeed;
    }

    void Update()
    {
        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0.0f) {
            foreach (var enemy in new List<Enemies.Controller>(enemiesInAura)) {
                if (enemy != null) {
                    enemy.TakeDamage(damageAmount);
                }
            }
            damageTimer = damageSpeed; // Reset the timer
        }
    }


    public void ChangeAuraDamage(float _multiplier)
    {
        damageAmount *= _multiplier;
    }

    public void ChangeAuraDamage(float _multiplier, float _duration)
    {
        StartCoroutine(TemporaryAuraDamageCoroutine(_multiplier, _duration));
    }

    public void ChangeAuraRange(float _multiplier)
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null) {
            collider.radius *= _multiplier;
        }
    }

    public void ChangeAuraRange(float _multiplier, float _duration)
    {
        StartCoroutine(TemporaryAuraRangeCoroutine(_multiplier, _duration));
    }

    public void ChangeAuraTickRate(float _multiplier)
    {
        damageSpeed /= _multiplier;
    }

    public void ChangeAuraTickRate(float _multiplier, float _duration)
    {
        StartCoroutine(TemporaryAuraTickRateCoroutine(_multiplier, _duration));
    }


    private IEnumerator TemporaryAuraDamageCoroutine(float _multiplier, float _duration)
    {
        float originalDamage = damageAmount;
        damageAmount *= _multiplier; // Increase damage
        yield return new WaitForSeconds(_duration);
        damageAmount = originalDamage; // Reset to original damage
    }

    private IEnumerator TemporaryAuraRangeCoroutine(float _multiplier, float _duration)
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null) {
            float originalRadius = collider.radius;
            collider.radius *= _multiplier; // Increase range
            yield return new WaitForSeconds(_duration);
            collider.radius = originalRadius; // Reset to original radius
        }
    }

    private IEnumerator TemporaryAuraTickRateCoroutine(float _multiplier, float _duration)
    {
        float originalSpeed = damageSpeed;
        damageSpeed /= _multiplier; // Increase tick rate
        yield return new WaitForSeconds(_duration);
        damageSpeed = originalSpeed; // Reset to original speed
    }
}
