using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public partial class UIManager
    {
        private TextMeshProUGUI timeText;
        private bool useMilitaryTime;
        private readonly Dictionary<string, (Button, Button)> abilities = new(); // Value1: ToggleOn, Value2: ToggleOff
        private string chosenAbility;
        
        private void InitializePlayerHome()
        {
            var timeObject = GameObject.Find("TimeText");
            if (!timeObject) {
                Debug.LogError("No TimeText GameObject found in the scene.");
                return;
            }
            timeText = timeObject.GetComponent<TextMeshProUGUI>();
            SetupSettingsScreen();
            SetupMainMenuSelections(true);
            FindShopTextFields();
            FindSharedShopObjects();
            ShopHandleSceneLoaded();
            HideAllMenus();
            FindAbilities();
            
            // Must be loaded after components are initialized
        }

        private void PlayerHomeUpdate()
        {
            timeText.text = System.DateTime.Now.ToString(useMilitaryTime ? "HH:mm:ss" : "hh:mm:ss tt");
        }


        private void FindAbilities()
        {
            var abilitiesObj = GameObject.Find("AbilitiesContent");
            var allAbilities = new List<Transform>();

            foreach (Transform child in abilitiesObj.transform) {
                allAbilities.Add(child);
            }
            
            abilities.Clear();
            foreach (var ability in allAbilities) {
                if (ability == abilitiesObj.transform)
                    continue;
                
                var texts = ability.GetComponentsInChildren<TextMeshProUGUI>();
                var abilityName = "";
                foreach (var text in texts) {
                    if (text.gameObject.name != "Name")
                        continue;
                    
                    abilityName = text.text;
                    
                    var toggleParent = ability.Find("Toggle") as RectTransform;
                    if (!toggleParent) {
                        Debug.LogError("No Toggle GameObject found under " + ability.name);
                        return;
                    }
                    var buttons = toggleParent.GetComponentsInChildren<Button>(true);
                    if (buttons.Length != 2) {
                        Debug.LogError("No Toggle GameObject found under " + ability.name);
                        return;
                    }
                    

                    abilities.Add(abilityName,
                        buttons[0].name == "On" ? (buttons[0], buttons[1]) : (buttons[1], buttons[0]));
                    
                }
                abilities[abilityName].Item1.gameObject.SetActive(ability.name != chosenAbility);
                abilities[abilityName].Item2.gameObject.SetActive(ability.name == chosenAbility);
                abilities[abilityName].Item1.onClick.AddListener(() => SelectAbility(abilityName) );
            }
        }

        private void SelectAbility(string _abilityName)
        {
            foreach (var ability in abilities) {
                if (ability.Key != _abilityName) {
                    ability.Value.Item1.gameObject.SetActive(true);
                    ability.Value.Item2.gameObject.SetActive(false);
                } else {
                    ability.Value.Item1.gameObject.SetActive(false);
                    ability.Value.Item2.gameObject.SetActive(true);
                }
            }
            chosenAbility = _abilityName;
        }
        
        private void ShowAchievementsMenu()
        {
            questCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            settingsCanvasObject.SetActive(false);
            shopCanvasObject.SetActive(false);
            achievementsCanvasObject.SetActive(true);
        }

        private void ShowHomeMenu()
        {
            questCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            settingsCanvasObject.SetActive(false);
            shopCanvasObject.SetActive(false);
            achievementsCanvasObject.SetActive(false);
        }
    }
}
