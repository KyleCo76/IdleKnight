using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.UI;

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
        
        // Cached Components
        private RectTransform cursorTransform;
        private Slider cursorSizeSlider;


        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad is handled by parent object
        }

        private void OnEnable()
        {
            GameSceneManager.Instance.OnSceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            GameSceneManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this) {
                Instance = null;
            }
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

        private void StartupComponents()
        {
            InputActions = new InputSystem_Actions();
            InputActions.Player.Enable();
            InputActions.UI.Enable();
            InputActions.UI.Pause.performed += _ => {
                if (IsPaused) {
                    ResumeGame();
                } else {
                    PauseGame();
                }
            };
            
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
