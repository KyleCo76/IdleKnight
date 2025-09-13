using UnityEngine;
using Game;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D projectileBody;

    private float timeToLive = 10.0f; // Time in seconds before the projectile is destroyed

    // Projectile Damage Variables
    private int damageAmount = 1;
    private AttackType typeOfAttack = AttackType.None;


    void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out projectileBody)) {
            Debug.LogError("No Rigidbody2D component found on " + gameObject.name);
        }
    }

    private void Update()
    {
        //Destroy the projectile after a certain time to prevent memory leaks
        timeToLive -= Time.deltaTime;
        if (timeToLive <= 0f) {
            Destroy(gameObject);
        }
    }


    public void Initialize(Vector2 _direction, float _force, float _damage, AttackType _attackType)
    {
        damageAmount = (int)_damage;
        projectileBody.AddForce(_direction * _force);
        typeOfAttack = _attackType;
    }

    private void OnCollisionEnter2D(Collision2D _other)
    {
        if (_other.collider.CompareTag("Enemy")) {
            if (_other.collider.TryGetComponent<Enemies.Controller>(out var enemyHealth)) {
                enemyHealth.TakeDamage(damageAmount, typeOfAttack);
            }
        }

        Destroy(gameObject);
    }
}
