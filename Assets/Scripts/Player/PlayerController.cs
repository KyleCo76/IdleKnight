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

        // Public getters for player stats
        public float BaseAttackSpeed { get; private set; }
        public float AttackSpeedBuff { get; private set; }
        public float AttackSpeedBuffTemp { get; private set; }
        
        public float BaseRangedDamage { get; private set; }
        public float RangedDamageBuff { get; private set; }
        public float RangedDamageBuffTemp { get; private set; }
        
        public float BaseMeleeDamage { get; private set; }
        public float MeleeDamageBuff { get; private set; }
        public float MeleeDamageBuffTemp { get; private set; }
        
        public float BaseSpeed => movementSpeed;
        public float SpeedBuff { get; private set; }
        public float SpeedBuffTemp { get; private set; }
        
        public float BaseAuraDamage { get; private set; }
        public float AuraDamageBuff { get; private set; }
        public float AuraDamageBuffTemp { get; private set; }
        
        public float BaseAuraDamageInterval { get; private set; }
        public float AuraDamageIntervalBuff { get; private set; }
        public float AuraDamageIntervalBuffTemp { get; private set; }
        
        public float BaseAuraRange { get; private set; }
        public float AuraRangeBuff { get; private set; }
        public float AuraRangeBuffTemp { get; private set; }
        
        public float BaseSuperDamage => superDamage;
        public float SuperDamageBuff { get; private set; }
        public float SuperDamageBuffTemp { get; private set; }
        
        public float BaseSuperCooldown => superCooldown;
        public float SuperCooldownBuff { get; private set; }
        public float SuperCooldownBuffTemp { get; private set; }
        

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
        private Camera mainCamera;

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
            
            playerAnimatorHelper.Init(playerAnimator);
            mainCamera = Camera.main;
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

            attackDoubleAttack = rangedDamage >= doubleAttackDamageActivationPoint;
            attackTripleAttack = rangedDamage >= tripleAttackDamageActivationPoint;
        }

        private void FixedUpdate()
        {
            if (gamePaused)
                return;

            if (moveInput != Vector2.zero) {
                MovePlayer();
                playerAnimatorHelper.SetWalking(true);
            } else {
                playerAnimatorHelper.SetWalking(false);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "Unity.PreferNonAllocApi")]
        private void Attack()
        {
            attackCooldownTimer = attackCooldown;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(playerTransform.position, attackRange, Vector2.zero);
            foreach (var hit in hits) {
                if (hit.collider && hit.collider.CompareTag("Enemy")) {
                    if (hit.collider.TryGetComponent<Enemies.Controller>(out var enemyHealth)) {
                        enemyHealth.ChangeHealth(-meleeDamage, AttackType.PlayerAttack);
                    }
                }
            }

            if (projectiles.Count > 0) {
                var (rotation, attackPoint) = GetProjectileData();

                Vector2 mainDirection = attackPoint.normalized;
                Vector2 leftDirection = Quaternion.Euler(0, 0, ArrowSpreadAmount) * mainDirection;
                Vector2 rightDirection = Quaternion.Euler(0, 0, -ArrowSpreadAmount) * mainDirection;

                if (moveInput != Vector2.zero) {
                    playerAnimatorHelper.SetAttack(true);
                } else {
                    if (Mathf.Abs(mainDirection.x) > Mathf.Abs(mainDirection.y)) {
                        playerAnimatorHelper.SetAttack(true, mainDirection.x > 0f ? Direction.Right : Direction.Left); // Horizontal Shot
                        if (mainDirection.x > 0f && isFlipped)
                            FlipSprite(false);
                        else if (mainDirection.x < 0f && !isFlipped)
                            FlipSprite(true);
                    } else {
                        playerAnimatorHelper.SetAttack(true, mainDirection.y > 0f ? Direction.Up : Direction.Down); // Vertical Shot
                    }
                }

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
            playerAnimatorHelper.SetAttack(false);
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
            if (mainCamera) 
                attackPoint = mainCamera.ScreenToWorldPoint(attackPoint);

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
                    playerAnimatorHelper.SetRight(true);
                    if (isFlipped)
                        FlipSprite(false);
                } else if (moveInput.x < 0) {
                    playerAnimatorHelper.SetLeft(true);
                    if (!isFlipped)
                        FlipSprite(true);
                }
            } else {
                // Moving vertically
                if (moveInput.y > 0) {
                    playerAnimatorHelper.SetUp(true);
                } else if (moveInput.y < 0) {
                    playerAnimatorHelper.SetDown(true);
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
            playerTransform.rotation = _lookLeft
                ? Quaternion.Euler(0f, moveInput.y > 0f ? 30f : 20f, 0f)
                : Quaternion.Euler(0f, moveInput.y > 0f ? 330f : 340f, 0f);
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
        private bool isWalking;
        private bool isUp;
        private bool isDown;
        private bool isLeft;
        private bool isRight;
        private readonly Dictionary<string, int> animatorHashes = new();
        private Animator animator;
        private int attackLayerIndex;
        private int baseLayerIndex;

        public void Init(Animator _animator)
        {
            animatorHashes["Up"] = Animator.StringToHash("Up");
            animatorHashes["Down"] = Animator.StringToHash("Down");
            animatorHashes["Left"] = Animator.StringToHash("Left");
            animatorHashes["Right"] = Animator.StringToHash("Right");
            animatorHashes["isWalking"] = Animator.StringToHash("isWalking");
            animatorHashes["isHurt"] = Animator.StringToHash("isHurt");
            animator = _animator;
            attackLayerIndex = animator.GetLayerIndex("Attack Layer");
            baseLayerIndex = animator.GetLayerIndex("Base Layer");
        }
        public void SetUp(bool _value, Direction _caller = Direction.None)
        {
            if (isUp == _value) return;
            isUp = _value;
            animator.SetBool(animatorHashes["Up"], _value);
            if (_caller == Direction.None) {
                SetDown(!_value, Direction.Up);
                SetLeft(!_value, Direction.Up);
                SetRight(!_value, Direction.Up);
            }
        }
        public void SetDown(bool _value, Direction _caller = Direction.None)
        {
            if (isDown == _value) return;
            isDown = _value;
            animator.SetBool(animatorHashes["Down"], _value);
            if (_caller == Direction.None) {
                SetUp(!_value, Direction.Down);
                SetLeft(!_value, Direction.Down);
                SetRight(!_value, Direction.Down);
            }
        }
        public void SetLeft(bool _value, Direction _caller = Direction.None)
        {
            if (isLeft == _value) return;
            isLeft = _value;
            animator.SetBool(animatorHashes["Left"], _value);
            if (_caller == Direction.None) {
                SetUp(!_value, Direction.Left);
                SetDown(!_value, Direction.Left);
                SetRight(!_value, Direction.Left);
            }
        }
        public void SetRight(bool _value, Direction _caller = Direction.None)
        {
            if (isRight == _value) return;
            isRight = _value;
            animator.SetBool(animatorHashes["Right"], _value);
            if (_caller == Direction.None) {
                SetUp(!_value, Direction.Right);
                SetDown(!_value, Direction.Right);
                SetLeft(!_value, Direction.Right);
            }
        }
        public void ResetAll(Direction _caller = Direction.None)
        {
            if (_caller != Direction.Up)
                SetUp(false, _caller);
            if (_caller != Direction.Down)
                SetDown(false, _caller);
            if (_caller != Direction.Left)
                SetLeft(false, _caller);
            if (_caller != Direction.Right)
                SetRight(false, _caller);
        }

        public void SetWalking(bool _value)
        {
            if (isWalking == _value) return;
            animator.SetBool(animatorHashes["isWalking"], _value);
            isWalking = _value;
        }

        public void SetHurt(bool _value)
        {
            animator.SetBool(animatorHashes["isHurt"], _value);
        }

        public void SetAttack(bool _value, Direction _direction = Direction.None)
        {
            animator.SetLayerWeight(attackLayerIndex, _value ? 1f : 0f);
            animator.SetLayerWeight(baseLayerIndex, _value ? 0f : 1f);
            switch (_direction)
            {
                case Direction.Up:
                    SetUp(true);
                    break;
                case Direction.Down:
                    SetDown(true);
                    break;
                case Direction.Left:
                    SetLeft(true);
                    break;
                case Direction.Right:
                    SetRight(true);
                    break;
            }
        }
    }

    internal enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
}