using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public partial class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // Cached Components
        private TextMeshProUGUI manaText;
        private TextMeshProUGUI healthText;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI powerUpText;
        private TextMeshProUGUI gemsText;
        private TextMeshProUGUI pauseScoreText;
        private TextMeshProUGUI pausePowerUpText;
        private TextMeshProUGUI pauseGemsText;
        private Slider manaBubble;
        private Slider healthBubble;
        private GameObject[] mainCanvasObjects;
        private GameObject uiCanvasObject;
        private GameObject settingsCanvasObject;
        private GameObject exitConfirmationObject;
        private GameObject levelFailedObject;
        private GameObject inventoryCanvasObject;
        private GameObject questCanvasObject;
        private GameObject skillsCanvasObject;
        private Toggle resumeButton;
        private Toggle quitButton;
        private Toggle settingsMenuToggle;
        private Toggle questMenuToggle;
        private Toggle inventoryMenuToggle;
        private Toggle skillsMenuToggle;
        private Button quitGameButton;
        private Button floatingResumeButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            // Don't destroy on load is handled by parent GameManager

            uiCanvasObject = GameObject.Find("UI");
            if (!uiCanvasObject) {
                Debug.LogError("No UI GameObject found in the scene.");
                enabled = false;
                return;
            }

            mainCanvasObjects = new GameObject[2];
            var menuBackground = GameObject.Find("MenuBackground");
            var menuItems = GameObject.Find("MainMenuItems");
            if (!menuBackground || !menuItems) {
                Debug.LogError("No MenuBackground or MainMenuMenuItems GameObject found under Canvas.");
                enabled = false;
                return;
            }
            mainCanvasObjects[0] = menuBackground.gameObject;
            mainCanvasObjects[1] = menuItems.gameObject;

            SetupMainMenuSelections();
            
            exitConfirmationObject = GameObject.Find("ExitConfirmation");
            if (!exitConfirmationObject) {
                Debug.LogError("No ExitConfirmation GameObject found in the scene.");
                enabled = false;
                return;           
            }
            
            levelFailedObject = GameObject.Find("LevelFailed");
            if (!levelFailedObject) {
                Debug.LogError("No LevelFailed GameObject found in the scene.");
                enabled = false;
                return;           
            }

            SetupScoreTexts();
            SetupHealthAndMana();
            AwakeShopManager();
            
            HideAllMenus();
            uiCanvasObject.SetActive(true);
        }
        

        public void HideAllMenus()
        {
            settingsCanvasObject.SetActive(false);
            mainCanvasObjects[0].SetActive(false);
            mainCanvasObjects[1].SetActive(false);
            levelFailedObject.SetActive(false);
            exitConfirmationObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            questCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            shopCanvasObject.SetActive(false);
        }

        private void SetupHealthAndMana()
        {
            var manaBubbleObject = uiCanvasObject.transform.Find("ManaBubble");
            if (manaBubbleObject == null) {
                Debug.LogError("No ManaBubble GameObject found under Canvas.");
                enabled = false;
                return;
            }
            if (!manaBubbleObject.TryGetComponent(out manaBubble)) {
                Debug.LogError("No Slider component found on ManaBubble GameObject.");
                enabled = false;
                return;
            }
            manaText = manaBubbleObject.GetComponentInChildren<TextMeshProUGUI>();
            if (manaText == null) {
                Debug.LogError("No TextMeshProUGUI component found on ManaText GameObject.");
                enabled = false;
                return;
            }

            var healthBubbleObject = uiCanvasObject.transform.Find("HealthBubble");
            if (healthBubbleObject == null) {
                Debug.LogError("No HealthBubble GameObject found under Canvas.");
                enabled = false;
                return;
            }
            if (!healthBubbleObject.TryGetComponent(out healthBubble)) {
                Debug.LogError("No HealthBubble GameObject found under Canvas.");
                enabled = false;
                return;
            }

            healthText = healthBubbleObject.GetComponentInChildren<TextMeshProUGUI>();
            if (healthText == null) {
                Debug.LogError("No TextMeshProUGUI component found on HealthText GameObject.");
                enabled = false;
            }

        }

        private void SetupMainMenuSelections()
        {
            // Get Menu Screens
            inventoryCanvasObject = GameObject.Find("InventoryMenu");
            if (!inventoryCanvasObject) {
                Debug.LogError("No InventoryMenu GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            questCanvasObject = GameObject.Find("QuestsMenu");
            if (!questCanvasObject) {
                Debug.LogError("No QuestsMenu GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            skillsCanvasObject = GameObject.Find("SkillsMenu");
            if (!skillsCanvasObject) {
                Debug.LogError("No SkillsMenu GameObject found under MainMenuItems.");
                enabled = false;
                return;           
            }

            settingsCanvasObject = GameObject.Find("SettingsMenu");
            // Get Menu Buttons
            var settingsButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleSettings");
            if (!settingsButtonObject) {
                Debug.LogError("No PauseMenuToggleSettings GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            if (!settingsButtonObject.TryGetComponent(out settingsMenuToggle)) {
                Debug.LogError("No PauseMenuToggleSettings GameObject found under MainMenuItems.");
                enabled = false;
                return;           
            }
            
            var questButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleQuests");
            if (!questButtonObject) {
                Debug.LogError("No PauseMenuToggleQuests GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            if (!questButtonObject.TryGetComponent(out questMenuToggle)) {
                Debug.LogError("No PauseMenuToggleQuests GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            
            var inventoryButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleInventory");
            if (!inventoryButtonObject) {
                Debug.LogError("No PauseMenuToggleInventory GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }

            if (!inventoryButtonObject.TryGetComponent(out inventoryMenuToggle)) {
                Debug.LogError("No PauseMenuToggleInventory GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            
            var skillsButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleSkills");
            if (!skillsButtonObject) {
                Debug.LogError("No PauseMenuToggleSkills GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            if (!skillsButtonObject.TryGetComponent(out skillsMenuToggle)) {
                Debug.LogError("No PauseMenuToggleSkills GameObject found under MainMenuItems.");
                enabled = false;
                return;
            }
            
            var quitButtonObject = GameObject.Find("QuitGameButton");
            if (!quitButtonObject) {
                Debug.LogError("No QuitGameButton GameObject found in the scene.");
                enabled = false;
                return;           
            }
            quitGameButton = quitButtonObject.GetComponent<Button>();
            
            settingsMenuToggle.onValueChanged.RemoveAllListeners();
            questMenuToggle.onValueChanged.RemoveAllListeners();
            inventoryMenuToggle.onValueChanged.RemoveAllListeners();
            skillsMenuToggle.onValueChanged.RemoveAllListeners();
            
            settingsMenuToggle.onValueChanged.AddListener(_isOn => { if (_isOn) ShowSettingsMenu(); });
            questMenuToggle.onValueChanged.AddListener( _isOn => { if (_isOn) ShowQuestMenu(); });
            inventoryMenuToggle.onValueChanged.AddListener(_isOn => { if (_isOn) ShowInventoryMenu(); });
            skillsMenuToggle.onValueChanged.AddListener(_isOn => { if (_isOn) ShowSkillsMenu(); });
            
            quitGameButton.onClick.RemoveAllListeners();
            quitGameButton.onClick.AddListener(GameManager.Instance.QuitGame);
        }

        private void SetupResumeButton(GameObject _parentObject)
        {
            var buttonX = _parentObject.transform.Find("ButtonX");
            if (!buttonX) {
                Debug.LogError("No ButtonX GameObject found under Canvas.");
                return;
            }

            if (!buttonX.TryGetComponent(out Button button)) {
                Debug.LogError("No Button component found on ButtonX GameObject.");
                return;
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(GameManager.Instance.ResumeGame);
        }

        private void SetupScoreTexts()
        {
            scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
            if (scoreText == null) {
                Debug.LogError("ScoreText UI element not found in the scene.");
                enabled = false;
                return;
            }
            scoreText.text = Mathf.FloorToInt(RunScoreManager.Instance.RunScore).ToString();
            
            powerUpText = GameObject.Find("PowerUpText").GetComponent<TextMeshProUGUI>();
            if (!powerUpText) {
                Debug.LogError("PowerUpText UI element not found in the scene.");
                enabled = false;
                return;
            }
            powerUpText.text = "0";
            
            gemsText = GameObject.Find("GemsText").GetComponent<TextMeshProUGUI>();
            if (!gemsText) {
                Debug.LogError("GemsText UI element not found in the scene.");
                enabled = false;
                return;
            }
            gemsText.text = "0";
            
            pauseScoreText = GameObject.Find("PauseScoreText").GetComponent<TextMeshProUGUI>();
            if (!pauseScoreText) {
                Debug.LogError("PauseScoreText UI element not found in the scene.");
                enabled = false;
            }
            pausePowerUpText = GameObject.Find("PausePowerUpText").GetComponent<TextMeshProUGUI>();
            if (!pausePowerUpText) {
                Debug.LogError("PausePowerUpText UI element not found in the scene.");
            }
            pauseGemsText = GameObject.Find("PauseGemsText").GetComponent<TextMeshProUGUI>();
            if (!pauseGemsText) {
                Debug.LogError("PauseGemsText UI element not found in the scene.");
            }
        }
        
        private void ShowInventoryMenu()
        {
            settingsCanvasObject.SetActive(false);
            questCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(true);
            mainCanvasObjects[0].SetActive(true);
            mainCanvasObjects[1].SetActive(true);
            SetupResumeButton(inventoryCanvasObject);
        }

        public void ShowSettingsMenu()
        {
            settingsCanvasObject.SetActive(true);
            mainCanvasObjects[0].SetActive(true);
            mainCanvasObjects[1].SetActive(true);
            questCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            SetupResumeButton(settingsCanvasObject);
            settingsMenuToggle.isOn = true;
            inventoryMenuToggle.isOn = false;
            questMenuToggle.isOn = false;
            skillsMenuToggle.isOn = false;
            shopCanvasObject.SetActive(false);
            isShopOpen = false;
            pauseScoreText.text = (RunScoreManager.Instance.RunScore).ToString();
            pausePowerUpText.text = (RunScoreManager.Instance.PowerUpScore).ToString();
            pauseGemsText.text = (RunScoreManager.Instance.GemsCount).ToString();
        }

        private void ShowSkillsMenu()
        {
            settingsCanvasObject.SetActive(false);
            questCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(true);
            mainCanvasObjects[0].SetActive(true);
            mainCanvasObjects[1].SetActive(true);
            SetupResumeButton(skillsCanvasObject);
        }

        private void ShowQuestMenu()
        {
            settingsCanvasObject.SetActive(false);
            questCanvasObject.SetActive(true);
            inventoryCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            mainCanvasObjects[0].SetActive(true);
            mainCanvasObjects[1].SetActive(true);
            SetupResumeButton(questCanvasObject);
        }
        
        public void UpdateHealthUI(float _currentHealth, float _maxHealth)
        {
            if (!healthBubble) {
                Debug.LogError("healthBubble is null in UIManager");
                return;
            }

            healthBubble.value = Mathf.Clamp01(_currentHealth / _maxHealth);
            healthText.text = $"{Mathf.RoundToInt(_currentHealth)} / {Mathf.RoundToInt(_maxHealth)}";
        }
        public void UpdateManaUI(float _currentMana, float _maxMana)
        {
            if (!manaBubble) {
                Debug.LogError("manaBubble is null in UIManager");
                return;
            }
            manaBubble.value = Mathf.Clamp01(_currentMana / _maxMana);
            manaText.text = $"{_currentMana}/{_maxMana}";
        }

        public void UpdateScoreText(int _score = -1)
        {
            if (_score >= 0) {
                scoreText.text = _score.ToString();
                return;
            }
            scoreText.text = Mathf.FloorToInt(RunScoreManager.Instance.RunScore).ToString();
        }

        public void UpdatePowerUpScoreText(int _score = -1)
        {
            if (_score >= 0) {
                powerUpText.text = _score.ToString();
                return;
            }
            powerUpText.text = RunScoreManager.Instance.PowerUpScore.ToString();
        }
    }
}
