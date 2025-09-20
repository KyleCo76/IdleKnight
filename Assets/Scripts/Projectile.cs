using UnityEngine;
using Game;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D projectileBody;

    private float timeToLive = 4.0f; // Time in seconds before the projectile is destroyed
    private bool isSuper;

    // Projectile Damage Variables
    private int damageAmount = 1;
    private AttackType typeOfAttack = AttackType.None;


    void Awake()
    {
        if (!TryGetComponent(out projectileBody)) {
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


    public void Initialize(Vector2 _direction, float _force, float _damage, AttackType _attackType, bool _isSuper)
    {
        damageAmount = (int)_damage;
        projectileBody.AddForce(_direction * _force);
        typeOfAttack = _attackType;
        isSuper = _isSuper;
    }

    private void OnTriggerEnter2D (Collider2D _other)
    {
        if (_other.CompareTag("Enemy")) {
            if (_other.TryGetComponent<Enemies.Controller>(out var enemyHealth)) {
                enemyHealth.ChangeHealth(-damageAmount, typeOfAttack);
            }
        }
        if (!isSuper)
            Destroy(gameObject);
    }
}
