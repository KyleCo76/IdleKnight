using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game;
using Managers;

namespace Player
{
    public partial class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        [FoldoutGroup("Movement Settings"), SerializeField, Tooltip("Movement speed of the player.")]
        private float movementSpeed = 5f;
        [FoldoutGroup("Movement Settings"), SerializeField, Tooltip("Sprint speed multiplier.")]
        private float sprintSpeedMultiplier = 1.4f;
        [FoldoutGroup("Attack Settings"), SerializeField, Tooltip("Attack cooldown in seconds.")]
        private float attackCooldown = 1f;
        [FoldoutGroup("Attack Settings"), SerializeField, Tooltip("Damage dealt per ranged attack.")]
        private float rangedDamage = 5f;
        [FoldoutGroup("Attack Settings"), SerializeField, Tooltip("Damage dealt per melee attack.")]
        private float meleeDamage = 10f;
        [FoldoutGroup("Attack Settings"), SerializeField, Tooltip("Range of the melee attack.")]
        private float attackRange = 1f;
        [FoldoutGroup("Attack Settings"), SerializeField, Tooltip("Ranged damage required to unlock double attack.")]
        private float doubleAttackDamageActivationPoint = 20f;
        [FoldoutGroup("Attack Settings"), SerializeField, Tooltip("Ranged damage required to unlock triple attack.")]
        private float tripleAttackDamageActivationPoint = 40f;
        [FoldoutGroup("Super Settings"), SerializeField, Tooltip("Cooldown time for the super ability in seconds.")]
        private float superCooldown = 10f;
        [FoldoutGroup("Super Settings"), SerializeField, Tooltip("Damage of the super ability")]
        private float superDamage = 100f;

        private bool gamePaused;
        private bool isFlipped;
        private float attackCooldownTimer;
        private float superDamageMultiplier = 1f;
        private float superSpeed = 600f;
        private const float ArrowSpreadAmount = 15f;
        private float currentMovementSpeed;
        private float currentSprintSpeedMultiplier;


        // Attack type variables
        private bool attackTripleAttack;
        private bool attackDoubleAttack;

        // Input values
        private Vector2 moveInput;
        private bool sprintPressed;
        private bool attackPressed;
        private bool isInteracting;
        private float superCooldownTimer;

        // Cached components
        private Transform playerTransform;
        private Animator playerAnimator;
        private readonly PlayerAnimatorHelper playerAnimatorHelper = new();
        private readonly List<GameObject> projectiles = new();
        private AuraManager playerAuraManager;

        // Cached in runtime
        private GameObject currentSuperPrefab;


        /*
         * Begin Input System methods
         */
        public void OnAttack(InputAction.CallbackContext _context)
        {
            if (!gamePaused) {
                attackPressed = _context.performed || _context.started;
            }
        }

        public void OnAttackPoint(InputAction.CallbackContext _context)
        {

        }

        public void OnInteract(InputAction.CallbackContext _context)
        {
            if (!gamePaused) {
                isInteracting = _context.performed;
            }
        }

        public void OnMove(InputAction.CallbackContext _context)
        {
            if (!gamePaused) {
                moveInput = _context.ReadValue<Vector2>();
            }
        }

        public void OnSprint(InputAction.CallbackContext _context)
        {
            if (!gamePaused) {
                sprintPressed = _context.performed;
            }
        }

        public void OnSuper(InputAction.CallbackContext _context)
        {
            if (superCooldownTimer > 0f || currentSuperPrefab == null)
                return;

            if (!gamePaused && _context.performed) {
                ActivateSuper();
            }
        }

        /*
         * Begin MonoBehaviour methods
        */
        private void Awake()
        {
            //ColliderAwake();
            HealthAwake();
            StaminaAwake();

            if (!this.TryGetComponent(out playerTransform)) {
                Debug.LogError("Player Controller requires a Transform component.");
                enabled = false;
                return;
            }
            if (!this.TryGetComponent(out playerAnimator)) {
                Debug.LogError("Player Controller requires an Animator component.");
                enabled = false;
                return;
            }
            playerAuraManager = this.GetComponentInChildren<AuraManager>();
            if (playerAuraManager == null) {
                Debug.LogError("Player Controller requires an AuraManager component.");
                enabled = false;
                return;
            }

            var allProjectiles = Resources.LoadAll<GameObject>("Projectiles/GoldenArrow");
            foreach (var proj in allProjectiles) {
                projectiles.Add(proj);
            }
        }
        
        private void OnEnable()
        {
            if (GameManager.Instance == null) {
                Debug.LogError("GameManager instance not found in the scene. Please ensure a GameManager is present.");
                return;
            }
            GameManager.Instance.OnGamePaused += HandleGamePause;
            GameManager.Instance.OnGameResumed += HandleGamePause;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
                return;
            GameManager.Instance.OnGamePaused -= HandleGamePause;
            GameManager.Instance.OnGameResumed -= HandleGamePause;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (GameManager.Instance == null) {
                Debug.LogError("GameManager instance not found in the scene. Please ensure a GameManager is present.");
            } else {
                GameManager.InputActions.Player.SetCallbacks(this);
            }
            currentMovementSpeed = movementSpeed;
            currentSprintSpeedMultiplier = sprintSpeedMultiplier;
        }

        private void Update()
        {
            if (gamePaused)
                return;

            HealthUpdate();
            StaminaUpdate();

            if (attackCooldownTimer > 0f) {
                attackCooldownTimer -= Time.deltaTime;
            }

            if (superCooldownTimer > 0f) {
                superCooldownTimer -= Time.deltaTime;
            }

            if (rangedDamage >= doubleAttackDamageActivationPoint) {
                attackDoubleAttack = true;
            } else {
                attackDoubleAttack = false;
            }
            if (rangedDamage >= tripleAttackDamageActivationPoint) {
                attackTripleAttack = true;
            } else {
                attackTripleAttack = false;
            }
        }

        private void FixedUpdate()
        {
            if (gamePaused)
                return;

            if (moveInput != Vector2.zero) {
                MovePlayer();
                playerAnimatorHelper.SetWalking(true, playerAnimator);
            } else {
                playerAnimatorHelper.SetWalking(false, playerAnimator);
            }
            if (attackPressed && attackCooldownTimer <= 0f) {
                Attack();
            }
        }


        private void ActivateSuper()
        {
            if (!ChangeMana(-specialAttackManaCost))
                return;

            superCooldownTimer = superCooldown;
            var (rotation, attackPoint) = GetProjectileData();

            var projectile = Instantiate(currentSuperPrefab, playerTransform.position, Quaternion.Euler(0f, 0f, rotation));
            if (projectile.TryGetComponent<Projectile>(out var projComponent)) {
                projComponent.Initialize(attackPoint.normalized, superSpeed, superDamage * superDamageMultiplier, AttackType.PlayerAttack, true);
            }
        }

        public void ApplySlow(float _multiplier)
        {
            currentMovementSpeed *= _multiplier;
            currentSprintSpeedMultiplier *= _multiplier;
        }

        private void Attack()
        {
            attackCooldownTimer = attackCooldown;
            int layerIndex = playerAnimator.GetLayerIndex("Attack Layer");
            playerAnimator.SetLayerWeight(layerIndex, 1f);
            layerIndex = playerAnimator.GetLayerIndex("Base Layer");
            playerAnimator.SetLayerWeight(layerIndex, 0f);

            RaycastHit2D[] hits = Physics2D.CircleCastAll(playerTransform.position, attackRange, Vector2.zero);
            foreach (var hit in hits) {
                if (hit.collider != null && hit.collider.CompareTag("Enemy")) {
                    if (hit.collider.TryGetComponent<Enemies.Controller>(out var enemyHealth)) {
                        enemyHealth.ChangeHealth(-meleeDamage, AttackType.PlayerAttack);
                    }
                }
            }

            if (projectiles.Count > 0) {
                var (rotation, attackPoint) = GetProjectileData();

                //Vector2 mainDirection = ((Vector3)attackPoint - transform.position).normalized;
                Vector2 mainDirection = attackPoint.normalized;
                Vector2 leftDirection = Quaternion.Euler(0, 0, ArrowSpreadAmount) * mainDirection;
                Vector2 rightDirection = Quaternion.Euler(0, 0, -ArrowSpreadAmount) * mainDirection;

                if (attackDoubleAttack) {
                    var projectileLeft = Instantiate(projectiles[0], playerTransform.position, Quaternion.Euler(0f, 0f, rotation + 15f));
                    if (projectileLeft.TryGetComponent<Projectile>(out var projComponentLeft)) {
                        projComponentLeft.Initialize(leftDirection, 400f, rangedDamage, AttackType.PlayerAttack, false);
                    }
                }
                if (attackTripleAttack) {
                    var projectileRight = Instantiate(projectiles[0], playerTransform.position, Quaternion.Euler(0f, 0f, rotation - 15f));
                    if (projectileRight.TryGetComponent<Projectile>(out var projComponentRight)) {
                        projComponentRight.Initialize(rightDirection, 400f, rangedDamage, AttackType.PlayerAttack, false);
                    }
                }
                var projectile = Instantiate(projectiles[0], playerTransform.position, Quaternion.Euler(0f, 0f, rotation));
                if (projectile.TryGetComponent<Projectile>(out var projComponent)) {
                    projComponent.Initialize(mainDirection, 400f, rangedDamage, AttackType.PlayerAttack, false);
                }
            }
        }

        public void AttackEnd()
        {
            int layerIndex = playerAnimator.GetLayerIndex("Attack Layer");
            playerAnimator.SetLayerWeight(layerIndex, 0f);
            layerIndex = playerAnimator.GetLayerIndex("Base Layer");
            playerAnimator.SetLayerWeight(layerIndex, 1f);
        }

        private void FlipSprite(bool _flipLeft)
        {
            if (_flipLeft) {
                playerTransform.localScale = new Vector3(-1, 1, 1);
                isFlipped = true;
            } else {
                playerTransform.localScale = new Vector3(1, 1, 1);
                isFlipped = false;
            }
        }

        private (float, Vector2) GetProjectileData()
        {
            var attackPoint = GameManager.InputActions.Player.AttackPoint.ReadValue<Vector2>();
            if (Camera.main != null) 
                attackPoint = Camera.main.ScreenToWorldPoint(attackPoint);

            Vector2 direction = (attackPoint - (Vector2)transform.position).normalized;
            int rotation = Mathf.RoundToInt(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return (rotation, attackPoint - (Vector2)transform.position);
        }

        private void HandleGamePause()
        {
            gamePaused = !gamePaused;
            //if (!gamePaused)
            //    return;
            //moveInput = Vector2.zero;
            //sprintPressed = false;
            //attackPressed = false;
            //isInteracting = false;
            //playerAnimatorHelper.SetWalking(false, playerAnimator);
            //ResetRotation();
        }

        private void MovePlayer()
        {
            playerTransform.position = Vector2.MoveTowards(playerTransform.position, playerTransform.position + (Vector3)moveInput, Time.deltaTime * (sprintPressed ? (currentSprintSpeedMultiplier * currentMovementSpeed) : currentMovementSpeed));
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)) {
                // Moving horizontally
                ResetRotation();
                if (moveInput.x > 0) {
                    playerAnimatorHelper.SetRight(true, playerAnimator);
                    if (isFlipped)
                        FlipSprite(false);
                } else if (moveInput.x < 0) {
                    playerAnimatorHelper.SetLeft(true, playerAnimator);
                    if (!isFlipped)
                        FlipSprite(true);
                }
            } else {
                // Moving vertically
                if (moveInput.y > 0) {
                    playerAnimatorHelper.SetUp(true, playerAnimator);
                } else if (moveInput.y < 0) {
                    playerAnimatorHelper.SetDown(true, playerAnimator);
                }
                // Check for diagonal movement
                if (Mathf.Approximately(moveInput.x, 0f)) {
                    ResetRotation();
                } else if (moveInput.x > 0) {
                    RotateSprite(true);
                    if ((isFlipped && moveInput.y > 0f) || (!isFlipped && moveInput.y < 0f))
                        FlipSprite(moveInput.y < 0f);
                } else if (moveInput.x < 0) {
                    RotateSprite(false);
                    if ((!isFlipped && moveInput.y > 0f) || (isFlipped && moveInput.y < 0f))
                        FlipSprite(moveInput.y > 0f);
                }
            }
        }

        public void RemoveSlow()
        {
            currentMovementSpeed = movementSpeed;
            currentSprintSpeedMultiplier = sprintSpeedMultiplier;
        }

        private void ResetRotation()
        {
            playerTransform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        }

        private void RotateSprite(bool _lookLeft)
        {
            if (_lookLeft) {
                playerTransform.rotation = Quaternion.Euler(0f, moveInput.y > 0f ? 30f : 20f, 0f);
            } else {
                playerTransform.rotation = Quaternion.Euler(0f, moveInput.y > 0f ? 330f : 340f, 0f);
            }
        }

        public void SetSuper(GameObject _superPrefab, float _damage, float _speed)
        {
            currentSuperPrefab = _superPrefab;
            superDamage = _damage;
            superSpeed = _speed;
        }
    }

    internal class PlayerAnimatorHelper
    {
        public bool IsWalking = false;
        private bool isUp = false;
        private bool isDown = false;
        private bool isLeft = false;
        private bool isRight = false;

        public void SetUp(bool _value, Animator _animator, Direction _caller = Direction.none)
        {
            if (isUp == _value) return;
            isUp = _value;
            _animator.SetBool("Up", _value);
            if (_caller == Direction.none) {
                SetDown(!_value, _animator, Direction.up);
                SetLeft(!_value, _animator, Direction.up);
                SetRight(!_value, _animator, Direction.up);
            }
        }
        public void SetDown(bool _value, Animator _animator, Direction _caller = Direction.none)
        {
            if (isDown == _value) return;
            isDown = _value;
            _animator.SetBool("Down", _value);
            if (_caller == Direction.none) {
                SetUp(!_value, _animator, Direction.down);
                SetLeft(!_value, _animator, Direction.down);
                SetRight(!_value, _animator, Direction.down);
            }
        }
        public void SetLeft(bool _value, Animator _animator, Direction _caller = Direction.none)
        {
            if (isLeft == _value) return;
            isLeft = _value;
            _animator.SetBool("Left", _value);
            if (_caller == Direction.none) {
                SetUp(!_value, _animator, Direction.left);
                SetDown(!_value, _animator, Direction.left);
                SetRight(!_value, _animator, Direction.left);
            }
        }
        public void SetRight(bool _value, Animator _animator, Direction _caller = Direction.none)
        {
            if (isRight == _value) return;
            isRight = _value;
            _animator.SetBool("Right", _value);
            if (_caller == Direction.none) {
                SetUp(!_value, _animator, Direction.right);
                SetDown(!_value, _animator, Direction.right);
                SetLeft(!_value, _animator, Direction.right);
            }
        }
        public void ResetAll(Animator _animator, Direction _caller = Direction.none)
        {
            if (_caller != Direction.up)
                SetUp(false, _animator, _caller);
            if (_caller != Direction.down)
                SetDown(false, _animator, _caller);
            if (_caller != Direction.left)
                SetLeft(false, _animator, _caller);
            if (_caller != Direction.right)
                SetRight(false, _animator, _caller);
        }

        public void SetWalking(bool _value, Animator _animator)
        {
            if (IsWalking == _value) return;
            _animator.SetBool("isWalking", _value);
            IsWalking = _value;
        }
    }

    internal enum Direction
    {
        none,
        up,
        down,
        left,
        right
    }
}