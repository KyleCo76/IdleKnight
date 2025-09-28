using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Ensures a single instance of MainManager exists throughout the game and that the original instance is not destroyed on loading new scenes.
    /// </summary>
    public class MainManager : MonoBehaviour
    {
        private static MainManager _instance;
        
        
        private void Awake()
        {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
