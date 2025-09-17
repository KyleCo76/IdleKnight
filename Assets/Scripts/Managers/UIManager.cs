using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Cached Components
    private Slider manaBubble;
    private TextMeshProUGUI manaText;
    private Slider healthBubble;
    private TextMeshProUGUI healthText;
    private GameObject shopCanvasObject;
    private GameObject uiCanvasObject;
    private GameObject pauseCanvasObject;
    private Toggle resumeButton;
    private Toggle quitButton;

    private Button item1BuyIcon;
    private Button item2BuyIcon;
    private Button item3BuyIcon;
    private Toggle leaveShopButton;
    private TextMeshProUGUI leaveShopButtonText;
    private Image item1IconImage;
    private Image item2IconImage;
    private Image item3IconImage;
    private TextMeshProUGUI item1PriceText;
    private TextMeshProUGUI item2PriceText;
    private TextMeshProUGUI item3PriceText;


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
        if (!manaBubbleObject.TryGetComponent<Slider>(out manaBubble)) {
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
        if (!healthBubbleObject.TryGetComponent<Slider>(out healthBubble)) {
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

        shopCanvasObject = GameObject.Find("Shop");
        if (shopCanvasObject == null) {
            Debug.LogError("No Shop GameObject found in the scene.");
            enabled = false;
            return;
        }

        pauseCanvasObject = GameObject.Find("PauseMenu");
        if (pauseCanvasObject == null) {
            Debug.LogError("No PauseMenu GameObject found in the scene.");
            enabled = false;
            return;
        }

        var MenuButtonObject = pauseCanvasObject.transform.Find("Menu");
        if (MenuButtonObject == null) {
            Debug.LogError("No Menu GameObject found in PauseMenu.");
            enabled = false;
            return;
        }
        var holderButtonObject = MenuButtonObject.transform.Find("ButtonHolder");
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

        var shopItemHolderObject = shopCanvasObject.transform.Find("ItemHolder");
        if (shopItemHolderObject == null) {
            Debug.LogError("No ShopItemHolder GameObject found in Shop.");
            enabled = false;
            return;
        }
        var item1Object = shopItemHolderObject.transform.Find("Item1");
        if (item1Object == null) {
            Debug.LogError("No Item1 GameObject found in Shop.");
            enabled = false;
            return;
        }
        item1BuyIcon = item1Object.GetComponentInChildren<Button>();
        if (item1BuyIcon == null) {
            Debug.LogError("No Buy Button found in Item1.");
            enabled = false;
            return;
        }
        var item1IconObject = item1Object.Find("Item1Icon");
        if (item1IconObject == null) {
            Debug.LogError("No Item1Icon GameObject found in Item1.");
            enabled = false;
            return;
        }
        item1IconImage = item1IconObject.GetComponent<Image>();
        if (item1IconImage == null) {
            Debug.LogError("No Icon Image found in Item1.");
            enabled = false;
            return;
        }
        item1PriceText = item1Object.GetComponentInChildren<TextMeshProUGUI>();
        if (item1PriceText == null) {
            Debug.LogError("No Price Text found in Item1.");
            enabled = false;
            return;
        }

        var item2Object = shopItemHolderObject.transform.Find("Item2");
        if (item2Object == null) {
            Debug.LogError("No Item2 GameObject found in Shop.");
            enabled = false;
            return;
        }
        item2BuyIcon = item2Object.GetComponentInChildren<Button>();
        if (item2BuyIcon == null) {
            Debug.LogError("No Buy Button found in Item2.");
            enabled = false;
            return;
        }
        var item2IconObject = item2Object.Find("Item2Icon");
        if (item2IconObject == null) {
            Debug.LogError("No Item2Icon GameObject found in Item2.");
            enabled = false;
            return;
        }
        item2IconImage = item2IconObject.GetComponent<Image>();
        if (item2IconImage == null) {
            Debug.LogError("No Icon Image found in Item2.");
            enabled = false;
            return;
        }
        item2PriceText = item2Object.GetComponentInChildren<TextMeshProUGUI>();
        if (item2PriceText == null) {
            Debug.LogError("No Price Text found in Item2.");
            enabled = false;
            return;
        }
        var item3Object = shopItemHolderObject.transform.Find("Item3");
        if (item3Object == null) {
            Debug.LogError("No Item3 GameObject found in Shop.");
            enabled = false;
            return;
        }
        item3BuyIcon = item3Object.GetComponentInChildren<Button>();
        if (item3BuyIcon == null) {
            Debug.LogError("No Buy Button found in Item3.");
            enabled = false;
            return;
        }
        var item3IconObject = item3Object.Find("Item3Icon");
        if (item3IconObject == null) {
            Debug.LogError("No Item3Icon GameObject found in Item3.");
            enabled = false;
            return;
        }
        item3IconImage = item3IconObject.GetComponent<Image>();
        if (item3IconImage == null) {
            Debug.LogError("No Icon Image found in Item3.");
            enabled = false;
            return;
        }
        item3PriceText = item3Object.GetComponentInChildren<TextMeshProUGUI>();
        if (item3PriceText == null) {
            Debug.LogError("No Price Text found in Item3.");
            enabled = false;
            return;
        }
        var leaveShoshopItemHolderObject = shopCanvasObject.transform.Find("LeaveShopHolder");
        if (leaveShoshopItemHolderObject == null) {
            Debug.LogError("No LeaveShopHolder GameObject found in Shop.");
            enabled = false;
            return;
        }
        var leaveShopButtonObject = leaveShoshopItemHolderObject.transform.Find("LeaveShopButton");
        if (leaveShopButtonObject == null) {
            Debug.LogError("No LeaveShopButton GameObject found in Shop.");
            enabled = false;
            return;
        }
        leaveShopButton = leaveShopButtonObject.GetComponent<Toggle>();
        if (leaveShopButton == null) {
            Debug.LogError("No LeaveShopButton found in Shop.");
            enabled = false;
            return;
        }
        leaveShopButtonText = leaveShopButtonObject.GetComponentInChildren<TextMeshProUGUI>();
        if (leaveShopButtonText == null) {
            Debug.LogError("No LeaveShopButtonText found in Shop.");
            enabled = false;
            return;
        }

        SetupShopSkipFunction();
        SetupShopBuyFunctions();
        SetupPauseMenu();

        uiCanvasObject.SetActive(true);
        pauseCanvasObject.SetActive(false);
        shopCanvasObject.SetActive(false);
        
    }



    public void ActivateShop(bool _activate)
    {
        if (shopCanvasObject == null || uiCanvasObject == null) {
            Debug.LogError("ShopCanvasObject or UICanvasObject is null in UIManager");
            return;
        }
        if (ShopManager.Instance == null) {
            Debug.LogError("ShopManager instance is null in UIManager");
            return;
        }
        if (_activate) {
            int playerLevel = RunScoreManager.Instance.GetPlayerLevel();
            ShopManager.Instance.OpenShop(playerLevel);
        }
        if (_activate) {
            // Reset toggles to off state when activating shop
            if (leaveShopButton != null) leaveShopButton.isOn = true;
            if (item1BuyIcon != null) item1BuyIcon.interactable = true;
            if (item2BuyIcon != null) item2BuyIcon.interactable = true;
            if (item3BuyIcon != null) item3BuyIcon.interactable = true;
        }
        shopCanvasObject.SetActive(_activate);
        uiCanvasObject.SetActive(!_activate);
        //Time.timeScale = _activate ? 0.0f : 1.0f;
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
        resumeButton.onValueChanged.AddListener(isOn => {
            if (!isOn)
                GameManager.Instance.ResumeGame();
        });
        quitButton.onValueChanged.AddListener(isOn => {
            if (!isOn)
                GameManager.Instance.QuitGame();
        });
    }

    private void SetupShopBuyFunctions()
    {
        if (item1BuyIcon == null || item2BuyIcon == null || item3BuyIcon == null) {
            Debug.LogError("One or more item buy buttons are null in UIManager");
            return;
        }
        item1BuyIcon.onClick.AddListener(() => {
            ShopManager.Instance.BuyItem(1);
        });
        item2BuyIcon.onClick.AddListener(() => {
            ShopManager.Instance.BuyItem(2);
        });
        item3BuyIcon.onClick.AddListener(() => {
            ShopManager.Instance.BuyItem(3);
        });
    }

    public void SetShopItemCost(int _itemCost, int _itemIndex)
    {
        if (_itemIndex < 1 || _itemIndex > 3) {
            Debug.LogError("Invalid item index in SetShopItemCost");
            return;
        }
        switch (_itemIndex) {
            case 1:
                item1PriceText.text = _itemCost.ToString();
                break;
            case 2:
                item2PriceText.text = _itemCost.ToString();
                break;
            case 3:
                item3PriceText.text = _itemCost.ToString();
                break;
        }
    }

    public void SetShopItemIcon(Sprite _icon, RuntimeAnimatorController _animator, int _itemIndex)
    {
        if (_itemIndex < 1 || _itemIndex > 3) {
            Debug.LogError("Invalid item index in SetShopItemIcon");
            return;
        }
        switch (_itemIndex) {
            case 1:
                item1IconImage.sprite = _icon;
                if (item1IconImage.TryGetComponent<Animator>(out var item1IconAnimator)) {
                    item1IconAnimator.runtimeAnimatorController = _animator;
                }
                break;
            case 2:
                item2IconImage.sprite = _icon;
                if (item2IconImage.TryGetComponent<Animator>(out var item2IconAnimator)) {
                    item2IconAnimator.runtimeAnimatorController = _animator;
                }
                break;
            case 3:
                item3IconImage.sprite = _icon;
                if (item3IconImage.TryGetComponent<Animator>(out var item3IconAnimator)) {
                    item3IconAnimator.runtimeAnimatorController = _animator;
                }
                break;
        }
    }

    private void SetupShopSkipFunction()
    {
        if (leaveShopButton == null) {
            Debug.LogError("leaveShopButton is null in UIManager");
            return;
        }
        leaveShopButton.onValueChanged.AddListener(isOn => {
            ActivateShop(false);
        });
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
        if (leaveShopButton == null) {
            Debug.LogError("leaveShopButton is null in UIManager");
            return;
        }
        leaveShopButtonText.text = _showSkip ? "Skip" : "Leave";
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
