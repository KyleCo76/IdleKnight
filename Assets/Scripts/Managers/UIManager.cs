using System.Collections.Generic;
using Game;
using Player;
using TMPro;
using Unity.Mathematics;
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
        private Slider superCooldownBar;
        private GameObject statsParent;
        private GameObject[] mainCanvasObjects;
        private GameObject uiCanvasObject;
        private GameObject settingsCanvasObject;
        private GameObject exitConfirmationObject;
        private GameObject levelFailedObject;
        private GameObject inventoryCanvasObject;
        private GameObject questCanvasObject;
        private GameObject skillsCanvasObject;
        private GameObject achievementsCanvasObject;
        private Toggle resumeButton;
        private Toggle quitButton;
        private Toggle settingsMenuToggle;
        private Toggle questMenuToggle;
        private Toggle inventoryMenuToggle;
        private Toggle skillsMenuToggle;
        private Toggle shopMenuToggle;
        private Toggle homeMenuToggle;
        private Toggle achievementsMenuToggle;
        private Button quitGameButton;
        private Button floatingResumeButton;
        private Button levelFailedHomeButton;
        private Button levelFailedRetryButton;
        private TMP_Dropdown cursorPointerDropdown;
        
        // Stats Panel Components
        private readonly List<(TextMeshProUGUI[], PowerUpType)> stats = new();

        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            // Don't destroy on load is handled by parent GameManager
        }

        private void OnEnable()
        {
            if (!GameManager.Instance) {
                Debug.LogError("GameManager not found.");
                return;
            }

            GameManager.Instance.OnGameOver += GameOver;
            if (!GameSceneManager.Instance) {
                Debug.LogError("GameSceneManager not found.");
                return;           
            }
            GameSceneManager.Instance.OnSceneLoaded += NewSceneLoaded;
        }

        private void Update()
        {
            if (GameSceneManager.Instance.CurrentScene is SceneNames.PlayerHome)
                PlayerHomeUpdate();
        }

        private void OnDisable()
        {
            if (!GameManager.Instance)
                return;
            GameManager.Instance.OnGameOver -= GameOver;
            GameSceneManager.Instance.OnSceneLoaded -= NewSceneLoaded;
            if (playerController)
                playerController.OnSuperCooldownChange -= UpdateSuperUI;
        }


        private void FindPlayerAndCache()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) {
                Debug.LogError("Player not found.");
                return;
            }

            if (player.TryGetComponent(out PlayerController controller)) {
                controller.OnSuperCooldownChange += UpdateSuperUI;
                playerController = controller;
            } else {
                Debug.LogError("Player not found.");
            }
        }

        private void FindStatsScreen()
        {
            statsParent = GameObject.Find("StatsPanel");
            if (!statsParent) {
                Debug.LogError("No StatsPanel GameObject found in the scene.");
                return;
            }
            
            for (int i = 0; i < statsParent.transform.childCount; i++) {
                var child = statsParent.transform.GetChild(i);
                var baseParent = child.transform.Find("BaseScore");
                if (!baseParent) {
                    Debug.LogError("No BaseScore GameObject found under StatsPanel.");
                    return;
                }
                var baseText = baseParent.GetComponentInChildren<TextMeshProUGUI>();
                if (!baseText) {
                    Debug.LogError("No TextMeshProUGUI component found on BaseScore GameObject.");
                    return;
                }
                var buffParent = child.transform.Find("BuffScore");
                if (!buffParent) {
                    Debug.LogError("No BuffScore GameObject found under StatsPanel.");
                }
                var buffText = buffParent.GetComponentInChildren<TextMeshProUGUI>();
                if (!buffText) {
                    Debug.LogError("No TextMeshProUGUI component found on BuffScore GameObject.");
                }
                var tempParent = child.transform.Find("TempScore");
                if (!tempParent) {
                    Debug.LogError("No TempScore GameObject found under StatsPanel.");
                    return;
                }
                var tempText = tempParent.GetComponentInChildren<TextMeshProUGUI>();
                if (!tempText) {
                    Debug.LogError("No TextMeshProUGUI component found on TempScore GameObject.");
                    return;
                }
                
                PowerUpType type;
                switch (child.name) {
                    case "AttackSpeed":
                        type = PowerUpType.AttackSpeedBoost;
                        break;
                    case "RangedDamage":
                        type = PowerUpType.RangedDamageBoost;
                        break;
                    case "MeleeDamage":
                        type = PowerUpType.MeleeDamageBoost;
                        break;
                    case "HealthRegen":
                        type = PowerUpType.HealthRegenAmount;
                        break;
                    case "ManaRegen":
                        type = PowerUpType.ManaRegenAmount;
                        break;
                    case "Speed":
                        type = PowerUpType.SpeedBoost;
                        break;
                    case "AuraDamage":
                        type = PowerUpType.AuraDamageBoost;
                        break;
                    case "AuraRange":
                        type = PowerUpType.AuraRangeBoost;
                        break;
                    case "SuperDamage":
                        type = PowerUpType.SuperDamageBoost;
                        break;
                    case "SuperCooldown":
                        type = PowerUpType.SuperCooldownReduction;
                        break;
                    default:
                        Debug.LogError("Unknown PowerUpType in StatsPanel.");
                        enabled = false;
                        return;
                }

                var tmProGUIArray = new TextMeshProUGUI[3];
                tmProGUIArray[0] = baseText;
                tmProGUIArray[1] = buffText;
                tmProGUIArray[2] = tempText;
                stats.Add((tmProGUIArray, type));
            }
        }

        private void FindUIComponents()
        {
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
            
            var levelFailedHomeButtonObject = GameObject.Find("LevelFailedHomeButton");
            if (!levelFailedHomeButtonObject) {
                Debug.LogError("No LevelFailedHomeButton GameObject found under LevelFailed.");
                return;           
            }
            levelFailedHomeButton = levelFailedHomeButtonObject.GetComponent<Button>();
            levelFailedHomeButton.onClick.RemoveAllListeners();
            levelFailedHomeButton.onClick.AddListener(() => GameSceneManager.Instance.LoadScene(SceneNames.PlayerHome));
            
            var levelFailedRetryButtonObject = GameObject.Find("LevelFailedRetryButton");
            if (!levelFailedRetryButtonObject) {
                Debug.LogError("No LevelFailedRetryButton GameObject found under LevelFailed.");
                return;           
            }
            levelFailedRetryButton = levelFailedRetryButtonObject.GetComponent<Button>();
            levelFailedRetryButton.onClick.RemoveAllListeners();
            levelFailedRetryButton.onClick.AddListener(() => GameSceneManager.Instance.ReloadScene());
        }

        private void GameOver()
        {
            HideAllMenus();
            uiCanvasObject.SetActive(false);
            levelFailedObject.SetActive(true);
        }
        
        private float3 GetPlayerStats(PowerUpType _powerUpType)
        {
            switch (_powerUpType) {
                case PowerUpType.AttackSpeedBoost:
                    return new float3(playerController.BaseAttackSpeed, playerController.AttackSpeedBuff,
                        playerController.AttackSpeedBuffTemp);
                case PowerUpType.RangedDamageBoost:
                    return new float3(playerController.BaseRangedDamage, playerController.RangedDamageBuff,
                        playerController.RangedDamageBuffTemp);
                case PowerUpType.MeleeDamageBoost:
                    return new float3(playerController.BaseMeleeDamage, playerController.MeleeDamageBuff,
                        playerController.MeleeDamageBuffTemp);
                case PowerUpType.HealthRegenAmount:
                    return new float3(playerController.BaseHealthRegenAmount, playerController.HealthRegenAmountBuff,
                        playerController.HealthRegenAmountTempBuff);
                case PowerUpType.ManaRegenAmount:
                    return new float3(playerController.BaseManaRegenRate, playerController.ManaRegenRateBuff,
                        playerController.ManaRegenRateTempBuff);
                case PowerUpType.SpeedBoost:
                    return new float3(playerController.BaseSpeed, playerController.SpeedBuff,
                        playerController.SpeedBuffTemp);
                case PowerUpType.AuraDamageBoost:
                    return new float3(playerController.GetAuraDamageStats());
                case PowerUpType.AuraRangeBoost:
                    return new float3(playerController.GetAuraRangeStats());
                case PowerUpType.SuperDamageBoost:
                    return new float3(playerController.BaseSuperDamage, playerController.SuperDamageBuff, 0f);
                case PowerUpType.SuperCooldownReduction:
                    return new float3(playerController.BaseSuperCooldown, playerController.SuperCooldownBuff, 0f);
                default:
                    Debug.LogError("Unknown PowerUpType in StatsPanel.");
                    return new float3();
            }
        }

        public void HideAllMenus()
        {
            settingsCanvasObject.SetActive(false);
            if (mainCanvasObjects is { Length: > 0 }) {
                mainCanvasObjects[0].SetActive(false);
                mainCanvasObjects[1].SetActive(false);
            }
            if (levelFailedObject)
                levelFailedObject.SetActive(false);
            if (exitConfirmationObject)
                exitConfirmationObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            questCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            shopCanvasObject.SetActive(false);
            if (achievementsCanvasObject)
                achievementsCanvasObject.SetActive(false);
        }

        private void NewSceneLoaded(int _sceneIndex)
        {
            if (_sceneIndex is SceneNames.PlayerHome) {
                InitializePlayerHome();
                return;
            }

            if (_sceneIndex is SceneNames.MainMenu)
                return;
            
            SetupSettingsScreen();
            FindUIComponents();
            SetupScoreTexts();
            FindStatsScreen();
            SetupHealthAndMana();
            FindPlayerAndCache();
            if (!string.IsNullOrEmpty(chosenAbility))
                playerController.SetAbility(chosenAbility);
            AwakeShopManager();
            HideAllMenus();
            ShopHandleSceneLoaded();
            uiCanvasObject.SetActive(true);
        }

        private void PauseMenuToggles()
        {
            // Get Menu Buttons
            var settingsButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleSettings");
            if (!settingsButtonObject) {
                Debug.LogError("No PauseMenuToggleSettings GameObject found under MainMenuItems.");
                return;
            }
            if (!settingsButtonObject.TryGetComponent(out settingsMenuToggle)) {
                Debug.LogError("No PauseMenuToggleSettings GameObject found under MainMenuItems.");
                return;           
            }
            
            var questButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleQuests");
            if (!questButtonObject) {
                Debug.LogError("No PauseMenuToggleQuests GameObject found under MainMenuItems.");
                return;
            }
            if (!questButtonObject.TryGetComponent(out questMenuToggle)) {
                Debug.LogError("No PauseMenuToggleQuests GameObject found under MainMenuItems.");
                return;
            }
            
            var inventoryButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleInventory");
            if (!inventoryButtonObject) {
                Debug.LogError("No PauseMenuToggleInventory GameObject found under MainMenuItems.");
                return;
            }

            if (!inventoryButtonObject.TryGetComponent(out inventoryMenuToggle)) {
                Debug.LogError("No PauseMenuToggleInventory GameObject found under MainMenuItems.");
                return;
            }
            
            var skillsButtonObject = mainCanvasObjects[1].transform.Find("PauseMenuToggleSkills");
            if (!skillsButtonObject) {
                Debug.LogError("No PauseMenuToggleSkills GameObject found under MainMenuItems.");
                return;
            }
            if (!skillsButtonObject.TryGetComponent(out skillsMenuToggle)) {
                Debug.LogError("No PauseMenuToggleSkills GameObject found under MainMenuItems.");
                return;
            }
            
            var quitButtonObject = GameObject.Find("QuitGameButton");
            if (!quitButtonObject) {
                Debug.LogError("No QuitGameButton GameObject found in the scene.");
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

        private void PlayerMenuToggles()
        {
            var shopToggleObject = GameObject.Find("ShopToggle");
            if (!shopToggleObject || !shopToggleObject.TryGetComponent(out shopMenuToggle)) {
                Debug.LogError("Failed to find ShopToggle GameObject or ShopMenuToggle component.");
                return;
            }
            
            var homeToggleObject = GameObject.Find("HomeToggle");
            if (!homeToggleObject || !homeToggleObject.TryGetComponent(out homeMenuToggle)) {
                Debug.LogError("Failed to find HomeToggle GameObject or HomeMenuToggle component.");
                return;
            }
            
            var achievementsToggleObject = GameObject.Find("AchievementsToggle");
            if (!achievementsToggleObject || !achievementsToggleObject.TryGetComponent(out achievementsMenuToggle)) {
                Debug.LogError("Failed to find AchievementsToggle GameObject or AchievementsMenuToggle component.");
                return;
            }
            
            var settingsToggleObject = GameObject.Find("SettingsToggle");
            if (!settingsToggleObject || !settingsToggleObject.TryGetComponent(out settingsMenuToggle)) {
                Debug.LogError("Failed to find SettingsToggle GameObject or SettingsMenuToggle component.");
                return;
            }
            
            var questToggleObject = GameObject.Find("QuestsToggle");
            if (!questToggleObject || !questToggleObject.TryGetComponent(out questMenuToggle)) {
                Debug.LogError("Failed to find QuestsToggle GameObject or QuestMenuToggle component.");
                return;
            }
            
            var inventoryToggleObject = GameObject.Find("InventoryToggle");
            if (!inventoryToggleObject || !inventoryToggleObject.TryGetComponent(out inventoryMenuToggle)) {
                Debug.LogError("Failed to find InventoryToggle GameObject or InventoryMenuToggle component.");
                return;
            }

            var skillsToggleObject = GameObject.Find("SkillsToggle");
            if (!skillsToggleObject || !skillsToggleObject.TryGetComponent(out skillsMenuToggle)) {
                Debug.LogError("Failed to find SkillsToggle GameObject or SkillsMenuToggle component.");
                return;
            }
            shopMenuToggle.onValueChanged.RemoveAllListeners();
            homeMenuToggle.onValueChanged.RemoveAllListeners();
            achievementsMenuToggle.onValueChanged.RemoveAllListeners();
            settingsMenuToggle.onValueChanged.RemoveAllListeners();
            questMenuToggle.onValueChanged.RemoveAllListeners();
            inventoryMenuToggle.onValueChanged.RemoveAllListeners();
            skillsMenuToggle.onValueChanged.RemoveAllListeners();
            
            shopMenuToggle.onValueChanged.AddListener(_isOn =>
            {
                if (_isOn) SetupShopUI(true);
            });
            homeMenuToggle.onValueChanged.AddListener(_isOn =>
            {
                if (_isOn) ShowHomeMenu();
            });
            achievementsMenuToggle.onValueChanged.AddListener(_isOn =>
            {
                if (_isOn) ShowAchievementsMenu(); 
            });
            settingsMenuToggle.onValueChanged.AddListener(_isOn =>
            {
                if (_isOn) ShowSettingsMenu();
            });
            questMenuToggle.onValueChanged.AddListener( _isOn =>
            {
                if (_isOn) ShowQuestMenu();
            });
            inventoryMenuToggle.onValueChanged.AddListener(_isOn =>
            {
                if (_isOn) ShowInventoryMenu();
            });
            skillsMenuToggle.onValueChanged.AddListener(_isOn =>
            {
                if (_isOn) ShowSkillsMenu();
            });
            
        }

        private void SetupHealthAndMana()
        {
            var manaBubbleObject = uiCanvasObject.transform.Find("ManaBubble");
            if (manaBubbleObject == null) {
                Debug.LogError("No ManaBubble GameObject found under Canvas.");
                return;
            }
            if (!manaBubbleObject.TryGetComponent(out manaBubble)) {
                Debug.LogError("No Slider component found on ManaBubble GameObject.");
                return;
            }
            manaText = manaBubbleObject.GetComponentInChildren<TextMeshProUGUI>();
            if (manaText == null) {
                Debug.LogError("No TextMeshProUGUI component found on ManaText GameObject.");
                return;
            }

            var healthBubbleObject = uiCanvasObject.transform.Find("HealthBubble");
            if (healthBubbleObject == null) {
                Debug.LogError("No HealthBubble GameObject found under Canvas.");
                return;
            }
            if (!healthBubbleObject.TryGetComponent(out healthBubble)) {
                Debug.LogError("No HealthBubble GameObject found under Canvas.");
                return;
            }

            healthText = healthBubbleObject.GetComponentInChildren<TextMeshProUGUI>();
            if (healthText == null) {
                Debug.LogError("No TextMeshProUGUI component found on HealthText GameObject.");
                return;
            }

            var superCooldownBarObject = uiCanvasObject.transform.Find("SuperCooldownBar");
            if (superCooldownBarObject == null) {
                Debug.LogError("No SuperCooldownBar GameObject found under Canvas.");
                return;
            }
            
            if (!superCooldownBarObject.TryGetComponent(out superCooldownBar)) {
                Debug.LogError("No SuperCooldownBar GameObject found under Canvas.");
            }
        }

        private void SetupMainMenuSelections(bool _isPlayerMenu = false)
        {
            // Get Menu Screens
            inventoryCanvasObject = GameObject.Find("InventoryMenu");
            if (!inventoryCanvasObject) {
                Debug.LogError("No InventoryMenu GameObject found under MainMenuItems.");
                return;
            }
            questCanvasObject = GameObject.Find("QuestsMenu");
            if (!questCanvasObject) {
                Debug.LogError("No QuestsMenu GameObject found under MainMenuItems.");
                return;
            }
            skillsCanvasObject = GameObject.Find("SkillsMenu");
            if (!skillsCanvasObject) {
                Debug.LogError("No SkillsMenu GameObject found under MainMenuItems.");
                return;           
            }
            settingsCanvasObject = GameObject.Find("SettingsMenu");
            if (!settingsCanvasObject) {
                Debug.LogError("No SettingsMenu GameObject found under MainMenuItems.");
                return;
            }


            if (_isPlayerMenu) {
                achievementsCanvasObject = GameObject.Find("AchievementsMenu");
                if (!achievementsCanvasObject) {
                    Debug.LogError("No AchievementsMenu GameObject found under MainMenuItems.");
                    return;
                }

                PlayerMenuToggles();
            } else {
                PauseMenuToggles();
            }
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

        private void SetupSettingsScreen()
        {
            var cursorPointerDropdownObject =  GameObject.Find("CursorPointerDropdown");
            if (!cursorPointerDropdownObject) {
                Debug.LogError("No cursorPointerDropdown GameObject found in the scene.");
                return;
            }

            if (!cursorPointerDropdownObject.TryGetComponent(out cursorPointerDropdown)) {
                Debug.LogError("No cursorPointerDropdown GameObject found in the scene.");
            }
            cursorPointerDropdown.onValueChanged.RemoveAllListeners();
            cursorPointerDropdown.onValueChanged.AddListener(GameManager.Instance.SetCursorImage);
        }

        private void SetupStatsScreen()
        {
            foreach (var (textArray, powerUpType) in stats) {
                var stats1 = GetPlayerStats(powerUpType);

                var useSecondStat = false;
                float3 stats2 = float3.zero;
                switch (powerUpType) {
                    case PowerUpType.HealthRegenAmount:
                        stats2 = new float3(playerController.BaseHealthRegenInterval,
                            playerController.HealthRegenIntervalBuff, playerController.HealthRegenIntervalTempBuff);
                        useSecondStat = true;
                        break;
                    case PowerUpType.ManaRegenAmount:
                        stats2 = new float3(playerController.BaseManaRegenInterval,
                            playerController.ManaRegenIntervalBuff, playerController.ManaRegenIntervalTempBuff);
                        useSecondStat = true;
                        break;
                    case PowerUpType.AuraDamageBoost:
                        stats2 = new float3(playerController.GetAuraDamageStats());
                        useSecondStat = true;
                        break;
                }

                var baseText = $"{stats1.x}";
                var buffText = $"{stats1.y}";
                var tempText = $"{stats1.z}";
                if (useSecondStat) {
                    baseText = $"{baseText}/{stats2.x}s";
                    buffText = $"{buffText}/{stats2.y}s";
                    tempText = $"{tempText}/{stats2.z}s";
                }
                textArray[0].text = baseText;
                textArray[1].text = buffText;
                textArray[2].text = tempText;
            }
        }

        private void ShowInventoryMenu()
        {
            settingsCanvasObject.SetActive(false);
            questCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(true);
            if (mainCanvasObjects is { Length: > 0 }) {
                mainCanvasObjects[0].SetActive(true);
                mainCanvasObjects[1].SetActive(true);
            }
            if (statsParent)
                statsParent.SetActive(true);
            if (achievementsCanvasObject)
                achievementsCanvasObject.SetActive(false);
            SetupResumeButton(inventoryCanvasObject);
        }

        public void ShowPauseMenu()
        {
            shopCanvasObject.SetActive(false);
            isShopOpen = false;
            pauseScoreText.text = (RunScoreManager.Instance.RunScore).ToString();
            pausePowerUpText.text = (RunScoreManager.Instance.PowerUpScore).ToString();
            pauseGemsText.text = (RunScoreManager.Instance.GemsCount).ToString();
            settingsMenuToggle.isOn = true;
            inventoryMenuToggle.isOn = false;
            questMenuToggle.isOn = false;
            skillsMenuToggle.isOn = false;
            ShowSettingsMenu();
            SetupStatsScreen();
            statsParent.SetActive(true);
        }

        private void ShowSettingsMenu()
        {
            settingsCanvasObject.SetActive(true);
            if (mainCanvasObjects is { Length: > 0 }) {
                mainCanvasObjects[0].SetActive(true);
                mainCanvasObjects[1].SetActive(true);
            }
            questCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            if (statsParent)
                statsParent.SetActive(true);
            if (achievementsCanvasObject)
                achievementsCanvasObject.SetActive(false);
            SetupResumeButton(settingsCanvasObject);
        }

        private void ShowSkillsMenu()
        {
            settingsCanvasObject.SetActive(false);
            questCanvasObject.SetActive(false);
            inventoryCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(true);
            if (mainCanvasObjects is { Length: > 0 }) {
                mainCanvasObjects[0].SetActive(true);
                mainCanvasObjects[1].SetActive(true);
            }
            if (statsParent)
                statsParent.SetActive(false);
            if (achievementsCanvasObject)
                achievementsCanvasObject.SetActive(false);
            SetupResumeButton(skillsCanvasObject);
        }

        private void ShowQuestMenu()
        {
            settingsCanvasObject.SetActive(false);
            questCanvasObject.SetActive(true);
            inventoryCanvasObject.SetActive(false);
            skillsCanvasObject.SetActive(false);
            if (mainCanvasObjects is { Length: > 0 }) {
                mainCanvasObjects[0].SetActive(true);
                mainCanvasObjects[1].SetActive(true);
            }
            if (statsParent)
                statsParent.SetActive(true);
            if (achievementsCanvasObject)
                achievementsCanvasObject.SetActive(false);
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
            manaText.text = $"{Mathf.RoundToInt(_currentMana)}/{Mathf.RoundToInt(_maxMana)}";
        }

        public void UpdateScoreText(int _score = -1)
        {
            if (_score >= 0) {
                scoreText.text = _score.ToString();
                return;
            }
            scoreText.text = Mathf.FloorToInt(RunScoreManager.Instance.RunScore).ToString();
        }

        private void UpdateSuperUI(float _cooldownAmount, float _cooldownTimer)
        {
            superCooldownBar.value = Mathf.Clamp01((_cooldownTimer - _cooldownAmount)/_cooldownTimer);
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
