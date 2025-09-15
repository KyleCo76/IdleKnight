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

        pauseCanvasObject.SetActive(false);
        shopCanvasObject.SetActive(false);
        
    }



    public void ActivateShop(bool _activate)
    {
        if (shopCanvasObject == null || uiCanvasObject == null) {
            Debug.LogError("ShopCanvasObject or UICanvasObject is null in UIManager");
            return;
        }
        shopCanvasObject.SetActive(_activate);
        uiCanvasObject.SetActive(!_activate);
    }

    public void ShowPauseMenu(bool _show)
    {
        if (pauseCanvasObject == null || uiCanvasObject == null || shopCanvasObject == null) {
            Debug.LogError("Missing component for ShowPauseMenu");
            return;
        }
        pauseCanvasObject.SetActive(_show);
        uiCanvasObject.SetActive(!_show);
        shopCanvasObject.SetActive(false);
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
