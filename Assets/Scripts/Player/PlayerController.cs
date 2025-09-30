using System;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Effects.Projectiles;
using UnityEngine;
using UnityEngine.InputSystem;
using Game;
using Managers;
using Unity.Mathematics;

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
        private float superSpeed = 600f;
        private const float ArrowSpreadAmount = 15f;
        private float currentMovementSpeed;
        private float speedDebuffs;
        private float superSecondaryFrequency;
        private int superSecondaryDamage;
        private int maxSecondaryEffects;

        public delegate void HandleSuperCooldownUI(float _amount, float _timer);
        public event HandleSuperCooldownUI OnSuperCooldownChange;

        // Public getters for player stats
        public float BaseAttackSpeed => attackCooldown;
        public float AttackSpeedBuff { get; private set; }
        public float AttackSpeedBuffTemp { get; private set; }
        
        public float BaseRangedDamage => rangedDamage;
        public float RangedDamageBuff { get; private set; }
        public float RangedDamageBuffTemp { get; private set; }
        
        public float BaseMeleeDamage => meleeDamage;
        public float MeleeDamageBuff { get; private set; }
        public float MeleeDamageBuffTemp { get; private set; }
        
        public float BaseSpeed => movementSpeed;
        public float SpeedBuff { get; private set; }
        public float SpeedBuffTemp { get; private set; }
        
        public float BaseSuperDamage => superDamage;
        public float SuperDamageBuff { get; private set; }
        
        public float BaseSuperCooldown => superCooldown;
        public float SuperCooldownBuff { get; private set; }

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
        [SerializeField]
        private GameObject currentSuperPrefab;

        // Ability variables
        private Action abilityMethod;


        /*
         * Begin Input System methods
         */
        public void OnAbility(InputAction.CallbackContext _context)
        {
            if (!gamePaused) {
                if (_context.performed && abilityMethod != null) {
                    abilityMethod.Invoke();
                }
            }
        }
        
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

            if (!gamePaused && !isDead && _context.performed) {
                ActivateSuper();
            }
        }

        /*
         * Begin MonoBehaviour methods
        */
        private void Awake()
        {
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

            AttackSpeedBuff = 1f;
            AttackSpeedBuffTemp = 1f;
            RangedDamageBuff = 1f;
            RangedDamageBuffTemp = 1f;
            MeleeDamageBuff = 1f;
            MeleeDamageBuffTemp = 1f;
            SpeedBuff = 1f;
            SpeedBuffTemp = 1f;
            SuperDamageBuff = 1f;
            SuperCooldownBuff = 1f;
        }
        
        private void OnEnable()
        {
            if (GameManager.Instance == null) {
                Debug.LogError("GameManager instance not found in the scene. Please ensure a GameManager is present.");
                return;
            }
            GameManager.Instance.OnGamePaused += HandleGamePause;
            GameManager.Instance.OnGameResumed += HandleGamePause;
            GameSceneManager.Instance.OnSceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
                return;
            GameManager.Instance.OnGamePaused -= HandleGamePause;
            GameManager.Instance.OnGameResumed -= HandleGamePause;
            GameSceneManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
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
            HealthStart();
            StaminaStart();
        }

        private void Update()
        {
            if (gamePaused || isDead)
                return;

            currentMovementSpeed = Mathf.Approximately(speedDebuffs, 0f)
                ? BaseSpeed * SpeedBuff * SpeedBuffTemp
                : BaseSpeed * speedDebuffs;
            
            HealthUpdate();
            StaminaUpdate();

            if (attackCooldownTimer > 0f) {
                attackCooldownTimer -= Time.deltaTime;
            }

            if (superCooldownTimer > 0f) {
                superCooldownTimer -= Time.deltaTime;
                OnSuperCooldownChange?.Invoke(superCooldownTimer, BaseSuperCooldown / SuperCooldownBuff);
            }

            attackDoubleAttack = BaseAttackSpeed * AttackSpeedBuff * AttackSpeedBuffTemp >= doubleAttackDamageActivationPoint;
            attackTripleAttack = BaseAttackSpeed * AttackSpeedBuff * AttackSpeedBuffTemp >= tripleAttackDamageActivationPoint;
        }

        private void FixedUpdate()
        {
            if (gamePaused || isDead)
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

        private void OnDestroy()
        {
            GameManager.InputActions.Player.SetCallbacks(null);
        }


        private void AbilityArtemis()
        {
            if (projectiles.Count == 0) {
                Debug.LogError("No Projectile prefab found in Artemis ability.");
                return;
            }

            const int degreeSeparation = 24;
            StartCoroutine(ArtemisBow(degreeSeparation));
        }

        private void ActivateSuper()
        {
            if (!TryChangeMana(-specialAttackManaCost))
                return;

            superCooldownTimer = (BaseSuperCooldown - SuperCooldownBuff);
            var (rotation, attackPoint) = GetProjectileData();

            var projectile = Instantiate(currentSuperPrefab, playerTransform.position, Quaternion.Euler(0f, 0f, rotation));
            if (projectile.TryGetComponent<Projectile>(out var projComponent)) {
                projComponent.Initialize(attackPoint.normalized, superSpeed,
                    BaseSuperDamage * SuperDamageBuff, AttackType.PlayerAttack, true);
            }

            if (projectile.TryGetComponent(out EnergySuperEffects energyComponent))
                energyComponent.Initialize(superSecondaryFrequency, superSecondaryDamage, maxSecondaryEffects);
        }

        public void ApplySlow(float _multiplier)
        {
            speedDebuffs = _multiplier;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "Unity.PreferNonAllocApi")]
        private void Attack()
        {
            attackCooldownTimer = BaseAttackSpeed / AttackSpeedBuff / AttackSpeedBuffTemp;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(playerTransform.position, attackRange, Vector2.zero);
            foreach (var hit in hits) {
                if (hit.collider && hit.collider.CompareTag("Enemy")) {
                    if (hit.collider.TryGetComponent<Enemies.Controller>(out var enemyHealth)) {
                        enemyHealth.ChangeHealth(-BaseMeleeDamage * MeleeDamageBuff * MeleeDamageBuffTemp , AttackType.PlayerAttack);
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
                    var projectileLeft = Instantiate(projectiles[0], playerTransform.position,
                        Quaternion.Euler(0f, 0f, rotation + 15f));
                    if (projectileLeft.TryGetComponent<Projectile>(out var projComponentLeft)) {
                        projComponentLeft.Initialize(leftDirection, 400f,
                            BaseRangedDamage * RangedDamageBuff * RangedDamageBuffTemp, AttackType.PlayerAttack, false);
                    }
                }
                if (attackTripleAttack) {
                    var projectileRight = Instantiate(projectiles[0], playerTransform.position,
                        Quaternion.Euler(0f, 0f, rotation - 15f));
                    if (projectileRight.TryGetComponent<Projectile>(out var projComponentRight)) {
                        projComponentRight.Initialize(rightDirection, 400f,
                            BaseRangedDamage * RangedDamageBuff * RangedDamageBuffTemp, AttackType.PlayerAttack, false);
                    }
                }

                var projectile = Instantiate(projectiles[0], playerTransform.position,
                    Quaternion.Euler(0f, 0f, rotation));
                if (projectile.TryGetComponent<Projectile>(out var projComponent)) {
                    projComponent.Initialize(mainDirection, 400f,
                        BaseRangedDamage * RangedDamageBuff * RangedDamageBuffTemp, AttackType.PlayerAttack, false);
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

        public float3 GetAuraDamageStats()
        {
            return playerAuraManager.GetDamageStats();
        }

        public float3 GetAuraRangeStats()
        {
            return playerAuraManager.GetRangeStats();
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
        }

        private void HandleSceneLoaded(int _sceneIndex)
        {
            OnSuperCooldownChange?.Invoke(superCooldown, BaseSuperCooldown / SuperCooldownBuff);
        }

        private void MovePlayer()
        {
            playerTransform.position = Vector2.MoveTowards(playerTransform.position,
                playerTransform.position + (Vector3)moveInput,
                Time.deltaTime * (sprintPressed
                    ? (sprintSpeedMultiplier * currentMovementSpeed)
                    : currentMovementSpeed));
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
            speedDebuffs = 0f;
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

        public void SetAbility(string _abilityName)
        {
            switch (_abilityName) {
                case "Apollo":

                    break;
                case "Artemis":
                    abilityMethod = AbilityArtemis;
                    break;
                case "Athena":

                    break;
                default:
                    Debug.LogError("Invalid ability name " + _abilityName + " provided to PlayerController.SetAbility.");
                    break;
            }
        }

        public void SetSuper(GameObject _superPrefab, float _damage, float _speed)
        {
            currentSuperPrefab = _superPrefab;
            superDamage = _damage;
            superSpeed = _speed;
        }
        public void SetSuper(GameObject _superPrefab, float _damage, float _speed, float _secondaryFrequency, int _secondaryDamage, int _maxSecondaryCount)
        {
            currentSuperPrefab = _superPrefab;
            superDamage = _damage;
            superSpeed = _speed;
            superSecondaryFrequency = _secondaryFrequency;
            superSecondaryDamage = _secondaryDamage;
            maxSecondaryEffects = _maxSecondaryCount;
        }

        private IEnumerator ArtemisBow(int _degreeSeparation)
        {
            for (int angle = 0; angle < 360; angle += _degreeSeparation) {
                var rotation = Quaternion.Euler(0f, 0f, angle);
                
                float angleInRadians = angle * Mathf.Deg2Rad;
                var targetPosition = new Vector2(
                    transform.position.x + 2f * Mathf.Cos(angleInRadians),
                    transform.position.y + 2f * Mathf.Sin(angleInRadians)
                );
                var arrow = Instantiate(projectiles[0], targetPosition, rotation);
                if (!arrow.TryGetComponent(out Projectile projectileManager)) {
                    Debug.LogError("Projectile prefab in Artemis ability does not have a Projectile component.");
                    Destroy(arrow);
                    yield break;
                }
    
                var direction = (targetPosition - (Vector2)transform.position).normalized;
                Debug.DrawLine(transform.position, targetPosition, Color.red, 2f);
                projectileManager.Initialize(direction, 300f,
                    BaseRangedDamage * RangedDamageBuff * RangedDamageBuffTemp, AttackType.PlayerAttack, false);
                
                yield return new WaitForSeconds(0.1f);
            }
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