using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static InputSystem_Actions InputActions { get; private set; }

    public static bool ReadyToLoadScene { get; set; } = false;

    public bool IsPaused { get; private set; } = false;

    private static readonly HashSet<string> assignedIDs = new();

    public delegate void GamePausedEventHandler();
    public event GamePausedEventHandler OnGamePaused;
    public delegate void GameResumedEventHandler();
    public event GameResumedEventHandler OnGameResumed;

    public int DifficultyLevel { get; private set; } = 1;


    //public delegate void ItemCollectedEventHandler(CollectableData _itemData);
    //public event ItemCollectedEventHandler OnItemCollected;

    //private PlayerController playerController;

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
        InputActions.UI.Pause.performed += ctx => {
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

    public void PauseGame()
    {
        UIManager.Instance.ShowPauseMenu(true);
        IsPaused = true;
        OnGamePaused?.Invoke();
        Time.timeScale = 0.0f;
    }

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
        UIManager.Instance.ResetResumeButton();
        StartCoroutine(ResumeDelay(0.5f));
        OnGameResumed?.Invoke();
    }

    private IEnumerator ResumeDelay(float _delayTime)
    {
        yield return new WaitForSecondsRealtime(_delayTime);
        UIManager.Instance.ShowPauseMenu(false);
        IsPaused = false;
        Time.timeScale = 1.0f;
    }
}
