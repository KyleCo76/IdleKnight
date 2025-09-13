using UnityEngine;

/// <summary>
/// Ensures a single instance of MainManager exists throughout the game and that the original instance is not destroyed on loading new scenes.
/// </summary>
public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
