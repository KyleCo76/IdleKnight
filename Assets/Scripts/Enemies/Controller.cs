using System.Collections.Generic;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using Game;
using Managers;

namespace Enemies
{
    public partial class Controller : MonoBehaviour, IPooledResettable
    {
        public static event System.Action<AttackType, int, float, Vector2, GameObject> OnEnemyDeath;

        [FoldoutGroup("Movement Settings"), SerializeField, Tooltip("The movement speed of the enemy")]
        private float movementSpeed = 3.0f;
        [FoldoutGroup("Movement Settings"), SerializeField, Tooltip("The distance to the next waypoint before moving to the next one")]
        private float nextWaypointDistance = 0.5f;
        [FoldoutGroup("Damage Settings"), SerializeField, Tooltip("The damage dealt to the player on contact")]
        private int contactDamage = 10;
        [FoldoutGroup("Damage Settings"), SerializeField, Tooltip("The time in seconds between damage ticks")]
        private float damageInterval = 1.0f;
        [FoldoutGroup("Death Values"), SerializeField, Tooltip("The value of the enemy when it dies")]
        private int deathValue = 1;
        [FoldoutGroup("Death Values"), SerializeField, Tooltip("The chance (0 to 1) to spawn an item on enemy death"), Range(0f, 1f)]
        private float itemSpawnChance;
        [FoldoutGroup("Animation Settings"), SerializeField, Tooltip("Does the enemy have an attack animation?")]
        private bool hasAttackAnimation;
        [FoldoutGroup("Animation Settings"), SerializeField, Tooltip(("Does the enemy have a death animation?"))]
        private bool hasDeathAnimation;


        // Cached components
        private Transform playerTransform;
        private Seeker seeker;
        private Animator enemyAnimator;
        private readonly Dictionary<string, int> animatorHashes = new();

        private Path path;
        private readonly float pathUpdateRate = 0.5f; // How often to update the path
        private int currentWaypoint;
        private float attackTimer;
        private bool isFlipped;
        private float moveSpeed;
        private bool isPooled;
        private MinionSpawner parentSpawner;
        
        private GameObject sourcePrefab;

        private void Awake()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                if (!playerObj.TryGetComponent(out playerTransform)) {
                    Debug.LogError("Player GameObject does not have a Transform component.");
                    enabled = false;
                }
            } else {
                Debug.LogError("No GameObject tagged 'Player' found. Please assign the player tag.");
                enabled = false;
            }
            if (!TryGetComponent(out seeker)) {
                Debug.LogError("Enemy GameObject does not have a Seeker component.");
                enabled = false;
            }
            
            if ((hasAttackAnimation || hasDeathAnimation) && !TryGetComponent(out enemyAnimator)) {
                Debug.LogError("Enemy is set to have an attack and/or death animation but does not have an Animator component.");
                enabled = false;
            }

            InvokeRepeating(nameof(UpdatePath), 0f, pathUpdateRate);
            
            currentHealth = maxHealth * GameManager.Instance.DifficultyLevel * Mathf.Max(1, RunScoreManager.Instance.GetPlayerLevel() / 2);
            moveSpeed = movementSpeed;
            animatorHashes["Attack"] = Animator.StringToHash("Attack");
            animatorHashes["Die"] = Animator.StringToHash("Die");
        }

        private void Update()
        {
            if (!GameManager.Instance || GameManager.Instance.IsPaused || isDead)
                return;
            
            if (attackTimer > 0f) {
                attackTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (path == null || !playerTransform || currentWaypoint >= path.vectorPath.Count
                || !GameManager.Instance || GameManager.Instance.IsPaused || isDead) {
                return;
            }

            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - (Vector2)transform.position).normalized;
            Vector2 force = moveSpeed * Time.fixedDeltaTime * direction;
            transform.position += (Vector3)force;
            float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (direction.x < 0f && !isFlipped) {
                FlipSprite(true);
            } else if (direction.x > 0f && isFlipped) {
                FlipSprite(false);
            }

            if (distance < nextWaypointDistance) {
                currentWaypoint++;
            }
        }

        private void OnCollisionEnter2D(Collision2D _other)
        {
            if (!GameManager.Instance || GameManager.Instance.IsPaused || isDead)
                return;
            
            if (_other.collider.CompareTag("Player") && attackTimer <= 0f)
            {
                if (_other.collider.TryGetComponent<Player.PlayerController>(out var player))
                {
                    if (hasAttackAnimation && enemyAnimator != null) {
                        enemyAnimator.SetTrigger(animatorHashes["Attack"]);
                    }
                    player.ChangeHealth(-contactDamage);
                    attackTimer = damageInterval;
                }
            }
        }
        
        public void OnTakenFromPool(GameObject _sourcePrefab)
        {
            sourcePrefab = _sourcePrefab;
            currentHealth = maxHealth;
            isDead = false;
            if (TryGetComponent(out Collider2D enemyCollider))
                enemyCollider.enabled = true;
        }

        public void OnTakenFromPool(GameObject _sourcePrefab, MinionSpawner _spawner)
        {
            isPooled = true;
            parentSpawner = _spawner;
            sourcePrefab = _sourcePrefab;
            currentHealth = maxHealth;
            isDead = false;
            if (TryGetComponent(out Collider2D enemyCollider))
                enemyCollider.enabled = true;
        }

        public GameObject GetSourcePrefab()
        {
            return sourcePrefab;
        }
        

        public void ApplySpeedBoost(float _multiplier)
        {
            moveSpeed *= _multiplier;
        }

        private void FlipSprite(bool _flipLeft)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1);
            isFlipped = _flipLeft;
        }

        private void OnPathComplete(Path _p)
        {
            if (!_p.error)
            {
                path = _p;
                currentWaypoint = 0;
            }
        }

        public void RemoveSpeedBoost()
        {
            moveSpeed = movementSpeed;
        }

        public void SetPlayerTransform(Transform _player)
        {
            playerTransform = _player;
        }
        
        private void UpdatePath()
        {
            if (!GameManager.Instance || GameManager.Instance.IsPaused || isDead)
                return;
            if (seeker.IsDone())
            {
                seeker.StartPath(transform.position, playerTransform.position, OnPathComplete);
            }
        }
    }
}