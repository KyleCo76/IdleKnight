using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class StartMenu : MonoBehaviour
    {
        public void ExitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        
        public void StartGame()
        {
            SceneManager.LoadScene(1);
        }

        public void LoadGame()
        {
            
        }
    }
}
