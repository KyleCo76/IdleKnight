using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public partial class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // Cached Components
        private Slider manaBubble;
        private TextMeshProUGUI manaText;
        private Slider healthBubble;
        private TextMeshProUGUI healthText;
        private GameObject uiCanvasObject;
        private GameObject pauseCanvasObject;
        private Toggle resumeButton;
        private Toggle quitButton;

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
                return;
            }

            pauseCanvasObject = GameObject.Find("PauseMenu");
            if (pauseCanvasObject == null) {
                Debug.LogError("No PauseMenu GameObject found in the scene.");
                enabled = false;
                return;
            }

            var menuButtonObject = pauseCanvasObject.transform.Find("Menu");
            if (menuButtonObject == null) {
                Debug.LogError("No Menu GameObject found in PauseMenu.");
                enabled = false;
                return;
            }
            var holderButtonObject = menuButtonObject.transform.Find("ButtonHolder");
            if (holderButtonObject == null) {
                Debug.LogError("No ButtonHolder GameObject found in PauseMenu.");
                enabled = false;
                return;
            }
            var resumeButtonObject = holderButtonObject.Find("ResumeGame");
            if (resumeButtonObject == null) {
                Debug.LogError("No ResumeGame GameObject found in PauseMenu.");
                enabled = false;
                return;
            }
            resumeButton = resumeButtonObject.GetComponent<Toggle>();
            if (resumeButton == null) {
                Debug.LogError("No ResumeGame found in PauseMenu.");
                enabled = false;
                return;
            }

            var quitButtonObject = holderButtonObject.transform.Find("QuitGame");
            if (quitButtonObject == null) {
                Debug.LogError("No QuitGame GameObject found in PauseMenu.");
                enabled = false;
                return;
            }
            quitButton = quitButtonObject.GetComponent<Toggle>();
            if (quitButton == null) {
                Debug.LogError("No QuitGame found in PauseMenu.");
                enabled = false;
                return;
            }
            
            SetupPauseMenu();

            uiCanvasObject.SetActive(true);
            pauseCanvasObject.SetActive(false);
            AwakeShopManager();
        }
        

        public void ResetResumeButton()
        {
            resumeButton.isOn = true;
        }

        private void SetupPauseMenu()
        {
            if (resumeButton == null || quitButton == null) {
                Debug.LogError("ResumeButton or QuitButton is null in UIManager");
                return;
            }
            resumeButton.onValueChanged.AddListener(_isOn => {
                if (!_isOn)
                    GameManager.Instance.ResumeGame();
            });
            quitButton.onValueChanged.AddListener(_isOn => {
                if (!_isOn)
                    GameManager.Instance.QuitGame();
            });
        }

        public void SetShopItemCost(int _itemCost, int _itemIndex)
        {
            // if (_itemIndex < 1 || _itemIndex > 3) {
            //     Debug.LogError("Invalid item index in SetShopItemCost");
            //     return;
            // }
            // switch (_itemIndex) {
            //     case 1:
            //         item1PriceText.text = _itemCost.ToString();
            //         break;
            //     case 2:
            //         item2PriceText.text = _itemCost.ToString();
            //         break;
            //     case 3:
            //         item3PriceText.text = _itemCost.ToString();
            //         break;
            // }
        }

        public void SetShopItemIcon(Sprite _icon, RuntimeAnimatorController _animator, int _itemIndex)
        {
            // if (_itemIndex < 1 || _itemIndex > 3) {
            //     Debug.LogError("Invalid item index in SetShopItemIcon");
            //     return;
            // }
            // switch (_itemIndex) {
            //     case 1:
            //         item1IconImage.sprite = _icon;
            //         if (item1IconImage.TryGetComponent<Animator>(out var item1IconAnimator)) {
            //             item1IconAnimator.runtimeAnimatorController = _animator;
            //         }
            //         break;
            //     case 2:
            //         item2IconImage.sprite = _icon;
            //         if (item2IconImage.TryGetComponent<Animator>(out var item2IconAnimator)) {
            //             item2IconAnimator.runtimeAnimatorController = _animator;
            //         }
            //         break;
            //     case 3:
            //         item3IconImage.sprite = _icon;
            //         if (item3IconImage.TryGetComponent<Animator>(out var item3IconAnimator)) {
            //             item3IconAnimator.runtimeAnimatorController = _animator;
            //         }
            //         break;
            // }
        }

        public void ShowPauseMenu(bool _show)
        {
            if (pauseCanvasObject == null || uiCanvasObject == null || shopCanvasObject == null) {
                Debug.LogError("Missing component for ShowPauseMenu");
                return;
            }
            pauseCanvasObject.SetActive(_show);
            //resumeButton.isOn = _show;
            uiCanvasObject.SetActive(!_show);
            shopCanvasObject.SetActive(false);
        }

        public void ToggleShopSkipButton(bool _showSkip)
        {
            // if (leaveShopButton == null) {
            //     Debug.LogError("leaveShopButton is null in UIManager");
            //     return;
            // }
            // leaveShopButtonText.text = _showSkip ? "Skip" : "Leave";
        }

        public void UpdateHealthUI(float _currentHealth, float _maxHealth)
        {
            if (healthBubble == null) {
                Debug.LogError("healthBubble is null in UIManager");
                return;
            }

            healthBubble.value = Mathf.Clamp01(_currentHealth / _maxHealth);
            healthText.text = $"{Mathf.RoundToInt(_currentHealth)} / {Mathf.RoundToInt(_maxHealth)}";
        }
        public void UpdateManaUI(float _currentMana, float _maxMana)
        {
            if (manaBubble == null) {
                Debug.LogError("manaBubble is null in UIManager");
                return;
            }
            manaBubble.value = Mathf.Clamp01(_currentMana / _maxMana);
            manaText.text = $"{_currentMana}/{_maxMana}";
        }
    }
}
