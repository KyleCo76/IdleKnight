using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public static InputSystem_Actions InputActions { get; private set; }

    public static bool ReadyToLoadScene { get; set; } = false;

    public bool IsPaused { get; private set; } = false;

    private static readonly HashSet<string> assignedIDs = new();


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
        InputActions.Player.Pause.performed += ctx => {
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PauseGame()
    {
        //UIManager.Instance.ShowPauseMenu(true);
        IsPaused = true;
        Time.timeScale = 0.0f;
    }

    /// <summary>
    /// Resumes the game after a short delay.
    /// </summary>
    /// <remarks>This method initiates a coroutine to resume the game after a specified delay.  The delay
    /// duration is fixed and cannot be customized through this method.</remarks>
    public void ResumeGame(bool _hideMenu = true)
    {
        StartCoroutine(ResumeDelay(0.2f, _hideMenu));
    }

    private IEnumerator ResumeDelay(float _delayTime, bool _hideMenu)
    {
        yield return new WaitForSecondsRealtime(_delayTime);
        //UIManager.Instance.ShowPauseMenu(!_hideMenu);
        IsPaused = false;
        Time.timeScale = 1.0f;
    }
}
