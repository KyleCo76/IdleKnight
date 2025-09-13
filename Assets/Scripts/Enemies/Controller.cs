using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using Game;

namespace Enemies
{
    public partial class Controller : MonoBehaviour
    {
        public static event System.Action<AttackType, float> OnEnemyDeath;

        [FoldoutGroup("Movement Settings"), SerializeField, Tooltip("The movement speed of the enemy")]
        private float movementSpeed = 3.0f;
        [FoldoutGroup("Movement Settings"), SerializeField, Tooltip("The distance to the next waypoint before moving to the next one")]
        private float nextWaypointDistance = 0.5f;
        [FoldoutGroup("Damage Settings"), SerializeField, Tooltip("The damage dealt to the player on contact")]
        private int contactDamage = 10;
        [FoldoutGroup("Damage Settings"), SerializeField, Tooltip("The time in seconds between damage ticks")]
        private float damageInterval = 1.0f;
        [FoldoutGroup("Death Values"), SerializeField, Tooltip("The value of the enemy when it dies")]
        private float deathValue = 1f;


        // Cached components
        private Transform playerTransform;
        private Seeker seeker;

        private Path path;
        private readonly float pathUpdateRate = 0.5f; // How often to update the path
        private int currentWaypoint = 0;
        private float attackTimer = 0f;
        private bool isFlipped = false;

        private void Awake()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                if (!playerObj.TryGetComponent<Transform>(out playerTransform)) {
                    Debug.LogError("Player GameObject does not have a Transform component.");
                    enabled = false;
                }
            } else {
                Debug.LogError("No GameObject tagged 'Player' found. Please assign the player tag.");
                enabled = false;
            }
            if (!TryGetComponent<Seeker>(out seeker)) {
                Debug.LogError("Enemy GameObject does not have a Seeker component.");
                enabled = false;
            }
            InvokeRepeating(nameof(UpdatePath), 0f, pathUpdateRate);
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (attackTimer > 0f) {
                attackTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (path == null || playerTransform == null || currentWaypoint >= path.vectorPath.Count) {
                return;
            }

            //transform.position = Vector2.MoveTowards(transform.position, path.vectorPath[currentWaypoint], movementSpeed * Time.fixedDeltaTime);
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - (Vector2)transform.position).normalized;
            Vector2 force = movementSpeed * Time.fixedDeltaTime * direction;
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
            if (_other.collider.CompareTag("Player") && attackTimer <= 0f)
            {
                if (_other.collider.TryGetComponent<Player.PlayerController>(out var player))
                {
                    player.ChangeHealth(-contactDamage);
                    attackTimer = damageInterval;
                }
            }
        }

        private void FlipSprite(bool _flipLeft)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1);
            if (_flipLeft) {
                isFlipped = true;
            } else {
                isFlipped = false;
            }
        }

        private void OnPathComplete(Path _p)
        {
            if (!_p.error)
            {
                path = _p;
                currentWaypoint = 0;
            }
        }

        public void SetPlayerTransform(Transform _player)
        {
            playerTransform = _player;
        }

        private void UpdatePath()
        {
            if (seeker.IsDone())
            {
                seeker.StartPath(transform.position, playerTransform.position, OnPathComplete);
            }
        }
    }
}