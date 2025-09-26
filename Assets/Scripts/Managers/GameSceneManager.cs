using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance { get; private set; }
        
        public delegate void SceneLoadedEventHandler(int _sceneIndex);

        public event SceneLoadedEventHandler OnSceneLoaded;
        
        public int CurrentScene { get; private set; }


        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad is handled by parent GameManager
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += NewSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= NewSceneLoaded;
        }
        
        
        private void NewSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
            CurrentScene = _scene.buildIndex;
            OnSceneLoaded?.Invoke(CurrentScene);
        }
        
        public void LoadScene(string _sceneName)
        {
            SceneManager.LoadScene(_sceneName);
        }

        public void LoadScene(int _sceneIndex)
        {
            SceneManager.LoadScene(_sceneIndex);
        }

        public void ReloadScene()
        {
            LoadScene(CurrentScene);
        }
    }
}
