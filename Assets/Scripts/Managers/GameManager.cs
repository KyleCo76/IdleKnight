using System.Collections;
using Pathfinding;
using UnityEngine;

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

        public int DifficultyLevel { get; private set; } = 1;


        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad is handled by parent object

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
        }

        private void OnDestroy()
        {
            if (Instance == this) {
                Instance = null;
            }
        }

        private void PauseGame()
        {
            UIManager.Instance.ShowPauseMenu();
            IsPaused = true;
            OnGamePaused?.Invoke();
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
