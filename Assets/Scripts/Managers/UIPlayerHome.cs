using TMPro;
using UnityEngine;

namespace Managers
{
    public partial class UIManager
    {
        private TextMeshProUGUI timeText;
        private bool useMilitaryTime;
        
        
        private void InitializePlayerHome()
        {
            var timeObject = GameObject.Find("TimeText");
            if (!timeObject) {
                Debug.LogError("No TimeText GameObject found in the scene.");
                return;
            }
            timeText = timeObject.GetComponent<TextMeshProUGUI>();
        }

        private void PlayerHomeUpdate()
        {
            timeText.text = System.DateTime.Now.ToString(useMilitaryTime ? "HH:mm:ss" : "hh:mm:ss tt");
        }
    }
}
