using System;
using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public static InputSystem_Actions InputActions { get; private set; }

        public static bool ReadyToLoadScene { get; set; } = false;

        public bool IsPaused { get; private set; }

        //private static readonly HashSet<string> AssignedIDs = new();

        public delegate void GamePausedEventHandler();
        public event GamePausedEventHandler OnGamePaused;
        public delegate void GameResumedEventHandler();
        public event GameResumedEventHandler OnGameResumed;
        public delegate void GameOverEventHandler();
        public event GameOverEventHandler OnGameOver;


        public int DifficultyLevel { get; private set; } = 1;

        private int attackTrigger;
        private bool animateCursor;

        private readonly Vector2 basicCursorHotspot = new Vector2(-15f, 18f);
        private readonly Vector2 swingingSwordCursorHotspot = new Vector2(-25f, 17f);
        private readonly Vector2 wobbleCursorHotspot = new Vector2(0f, 20f);
        
        // Cached Components
        private RectTransform cursorTransform;
        private Slider cursorSizeSlider;
        private Animator cursorAnimator;
        private CursorUI cursorUI;


        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad is handled by the parent object

            attackTrigger = Animator.StringToHash("Attack");
            if (!TryGetComponent(out cursorUI)) {
                Debug.LogError("Could not find CursorUI");
            }
        }

        private void OnEnable()
        {
            GameSceneManager.Instance.OnSceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            InputActions.Player.Attack.performed -= TriggerCursorAnimator;
            if (GameSceneManager.Instance)
                GameSceneManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this) {
                Instance = null;
            }
        }


        public Bounds GetCameraWorldBounds(Camera _camera = null)
        {
            if (!_camera)
                _camera = Camera.main;

            if (_camera != null) {
                float height = 2f * _camera.orthographicSize;
                float width = height * _camera.aspect;
                return new Bounds(_camera.transform.position, new Vector3(width, height, _camera.transform.position.z));
            }
            Debug.LogError("Could not find camera");
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        public uint GetEntropy(uint _seed = 1059161518u)
        {
            int shift1 = GetShiftPoint();
            uint byteValue = (uint)(attackTrigger >> shift1) & 0xFFu; // Constrain to 1 byte
            int shift2 = GetShiftPoint();
            _seed = unchecked(_seed + (byteValue << shift2));
            
            shift1 = GetShiftPoint();
            byteValue = (uint)(Mathf.RoundToInt(cursorTransform.position.x) >> shift1) & 0xFFu;
            shift2 = GetShiftPoint();
            _seed = unchecked(_seed + (byteValue << shift2));
            
            shift1 = GetShiftPoint();
            byteValue = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() >> shift1) & 0xFFu;
            shift2 = GetShiftPoint();
            _seed = unchecked(_seed + (byteValue << shift2));
            
            shift1 = GetShiftPoint();
            byteValue = (uint)(Mathf.RoundToInt(cursorTransform.position.y) >> shift1) & 0xFFu;
            shift2 = GetShiftPoint();
            _seed = unchecked(_seed + (byteValue << shift2));
            return _seed;
        }

        private int GetShiftPoint()
        {
            var x = Random.Range(0, 4);
            switch (x) {
                case 0:
                    x = 0;
                    break;
                case 1:
                    x = 8;
                    break;
                case 2:
                    x = 16;
                    break;
                case 3:
                    x = 24;
                    break;
                default:
                    Debug.LogError("Invalid shift point");
                    return 0;
            }

            return x;
        }
        
        private void HandleSceneLoaded(int _sceneIndex)
        {
            if (_sceneIndex is SceneNames.MainMenu or SceneNames.PlayerHome)
                return;
            
            if (InputActions != null) {
                InputActions.Player.Disable();
                InputActions.UI.Disable();
                InputActions.Dispose();
                InputActions = null;           
            }
            Time.timeScale = 1.0f;
            StartupComponents();
            SetCursorImage(1);
        }

        private void PauseGame()
        {
            UIManager.Instance.ShowPauseMenu();
            IsPaused = true;
            OnGamePaused?.Invoke();
            Time.timeScale = 0.0f;
        }

        public void PlayerDied()
        {
            OnGameOver?.Invoke();
            Time.timeScale = 0.0f;
        }

        /// <summary>
        /// Exits the application or stops play mode if running in the Unity Editor.
        /// </summary>
        /// <remarks>
        /// This method is used to terminate the application when called. In the Unity Editor, it stops play mode instead of completely quitting the application.
        /// </remarks>
        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// Resumes the game after a short delay.
        /// </summary>
        /// <remarks>This method initiates a coroutine to resume the game after a specified delay.  The delay
        /// duration is fixed and cannot be customized through this method.</remarks>
        public void ResumeGame()
        {
            //UIManager.Instance.ResetResumeButton();
            StartCoroutine(ResumeDelay(0.5f));
            OnGameResumed?.Invoke();
        }

        public void SetCursorImage(int _dropdownSelection)
        {
            if (!cursorTransform.gameObject.TryGetComponent(out Image cursorImage)) {
                Debug.LogError("No Sprite Renderer found on CursorTransform");
                return;
            }
            switch (_dropdownSelection) {
                case 0:
                    cursorImage.sprite = Resources.Load<Sprite>("CursorImages/BloodiedSword");
                    cursorAnimator.enabled = false;
                    animateCursor = false;
                    cursorUI.SetHotspot(basicCursorHotspot);
                    break;
                case 1:
                    cursorImage.sprite = Resources.Load<Sprite>("CursorImages/RedCursor");
                    cursorAnimator.enabled = false;
                    animateCursor = false;
                    cursorUI.SetHotspot(basicCursorHotspot);
                    break;
                case 2:
                    cursorImage.sprite = Resources.Load<Sprite>("CursorImages/SwingingSword");
                    //cursorImage.overrideSprite = Resources.Load<Sprite>("CursorImages/SwingingSword");
                    cursorAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("CursorImages/SwingingSwordController");
                    cursorAnimator.enabled = true;
                    animateCursor = true;
                    cursorUI.SetHotspot(swingingSwordCursorHotspot);
                    break;
                case 3:
                    cursorAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("CursorImages/WobbleShieldController");
                    cursorImage.sprite = Resources.Load<Sprite>("CursorImages/WobbleShield");
                    //cursorImage.overrideSprite = Resources.Load<Sprite>("CursorImages/WobbleShield");
                    cursorAnimator.enabled = true;
                    animateCursor = false;
                    cursorUI.SetHotspot(wobbleCursorHotspot);
                    break;
                default:
                    Debug.LogWarning("Unknown Sprite Selection " + _dropdownSelection);
                    break;
            }
        }

        private void StartupComponents()
        {
            
            InputActions = new InputSystem_Actions();
            InputActions.Player.Enable();
            InputActions.Player.Attack.performed += TriggerCursorAnimator;
            InputActions.UI.Enable();
            InputActions.UI.Pause.performed += _ => {
                if (IsPaused) {
                    ResumeGame();
                } else {
                    PauseGame();
                }
            };
            InputActions.UI.SlowTime.performed += _ => { Time.timeScale = Mathf.Clamp01(Time.timeScale - 0.1f); };
            InputActions.UI.SpeedTime.performed += _ => { Time.timeScale = Mathf.Clamp01(Time.timeScale + 0.1f); };
            
            cursorTransform = GameObject.Find("CursorImage").GetComponent<RectTransform>();
            var cursorSizeSliderParent = GameObject.Find("CursorSizeSlider");
            if (!cursorSizeSliderParent) {
                Debug.LogError("No CursorSizeSlider GameObject found in scene.");
                return;
            }
            if (!cursorSizeSliderParent.TryGetComponent(out cursorSizeSlider)) {
                Debug.LogError("No CursorSizeSlider found in scene.");
                return;           
            }
            cursorSizeSlider.onValueChanged.AddListener(_value => {
                cursorTransform.localScale = new Vector3(_value + 1, _value + 1, 1);
            });
            if (!cursorTransform.gameObject.TryGetComponent(out cursorAnimator)) {
                Debug.LogError("No CursorAnimator GameObject found in scene.");
                return;
            }
            
            cursorAnimator.enabled = animateCursor;
        }

        private void TriggerCursorAnimator(InputAction.CallbackContext _action)
        {
            if (_action.performed && animateCursor)
                cursorAnimator.SetTrigger(attackTrigger);
        }

        /// <summary>
        /// Introduces a delay before resuming the game.
        /// </summary>
        /// <param name="_delayTime">The duration of the delay in seconds before the game resumes.</param>
        /// <returns>An IEnumerator to be used in a coroutine for handling the delay.</returns>
        private IEnumerator ResumeDelay(float _delayTime)
        {
            yield return new WaitForSecondsRealtime(_delayTime);
            UIManager.Instance.HideAllMenus();
            IsPaused = false;
            Time.timeScale = 1.0f;
        }

        /// <summary>
        /// Updates the A* pathfinding grid within a specified radius around the object.
        /// </summary>
        /// <remarks>This method recalculates and updates the A* graph in the area defined by the given radius,
        /// ensuring that pathfinding data is accurate based on changes in the environment.</remarks>
        /// <param name="_paintRadius">The radius around the object within which the A* grid should be updated, specified in world units.</param>
        public void UpdateAStarGrid(float _paintRadius)
        {
            // var updateBounds = new Bounds(transform.position, new(_paintRadius, _paintRadius, 1));
            // var graphBounds = new GraphUpdateObject(updateBounds);
            // AstarPath.active.UpdateGraphs(graphBounds);
            AstarPath.active.Scan();
        }
    }
}
