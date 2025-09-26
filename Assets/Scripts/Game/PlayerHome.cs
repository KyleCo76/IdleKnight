using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class PlayerHome : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene(2);
        }
    }
}
