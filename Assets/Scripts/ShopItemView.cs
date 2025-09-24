using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [SerializeField, Tooltip("The button that will be used to interact with the item")]
    private Button button;
    [SerializeField, Tooltip("The Icon for the currency used on the button")]
    private Image currencyIcon;
    [SerializeField, Tooltip("The icon for the currency of supers used on the button")]
    private Sprite silverIcon;
    [SerializeField, Tooltip("The icon of the item")]
    private Image icon;
    [SerializeField, Tooltip("The TextMeshProUGUI that will display the item's name")]
    private TMPro.TMP_Text nameText;
    [SerializeField, Tooltip("The TextMeshProUGUI that will display the item's price")]
    private TMPro.TMP_Text priceText;

    [SerializeField, Tooltip("The Badge that can be displayed with the item")]
    private GameObject badge;
    [SerializeField, Tooltip("the TextMeshProUGUI that will display next to the badge")]
    private Text badgeText;

    [FormerlySerializedAs("Item")] [HideInInspector] public ShopItemDatabase.ShopSuperEntry Super;
    public ShopItemDatabase.ShopItemEntry Item;
    
    private SuperDatabase superDatabase;

    private void Awake()
    {
        superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
        if (!superDatabase) {
            Debug.LogError("No SuperDatabase found in Resources/ScriptableObjects.");
            enabled = false;
        }
    }

    public void Bind(ShopItemDatabase.ShopSuperEntry _superData, System.Action<ShopItemDatabase.ShopSuperEntry> _onItemClicked)
    {
        superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
        if (!superDatabase) {
            Debug.LogError("No SuperDatabase found in Resources/ScriptableObjects.");
            enabled = false;
        }
        Super = _superData;
        icon.sprite = superDatabase.GetSpriteForSuper(_superData.Id);
        nameText.text = _superData.DisplayName;
        priceText.text = _superData.Cost.ToString();
        
        currencyIcon.sprite = silverIcon;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onItemClicked?.Invoke(Super));
    }
    
    public void Bind(ShopItemDatabase.ShopItemEntry _itemData, System.Action<ShopItemDatabase.ShopItemEntry> _onItemClicked)
    {
        superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
        if (!superDatabase) {
            Debug.LogError("No SuperDatabase found in Resources/ScriptableObjects.");
            enabled = false;
        }
        Item = _itemData;
        icon.sprite = _itemData.Sprite;
        nameText.text = _itemData.DisplayName;

        var multiplier = 1f;
        var amount = 0f;
        if (!Mathf.Approximately(1f, _itemData.MultiplierMax)) {
            multiplier = PickMiddleHeavyRandom(1f, _itemData.MultiplierMax);
        } else {
            amount = PickMiddleHeavyRandom(1f, _itemData.AmountMax);
        }
        
        Item.SetItemMultiplierAndAmount(multiplier, amount);
        
        priceText.text = Mathf.Approximately(0f, _itemData.AmountMax)
            ? GetItemCost(_itemData.Cost, _itemData.MultiplierMax, multiplier)
            : GetItemCost(_itemData.Cost, _itemData.AmountMax, amount);
        
        badge.SetActive(true);
        badgeText.text = Mathf.Approximately(0f, _itemData.AmountMax) ? multiplier.ToString("0.00") : amount.ToString("0");
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onItemClicked?.Invoke(Item));
    }

    private string GetItemCost(float _baseCost, float _maxAmount, float _givenAmount)
    {
        _givenAmount = Mathf.Clamp(_givenAmount, 1f, _maxAmount);
        float factor = 1f + ((_givenAmount - 1f) * 1.5f) / (_maxAmount - 1f);
        return (_baseCost * factor).ToString("0");
    }
    
    private float PickMiddleHeavyRandom(float _min, float _max)
    {
        if (_min > _max) (_min, _max) = (_max, _min);
        if (Mathf.Approximately(_min, _max)) return _min;

        float t = 0.5f * (Random.value + Random.value); // triangular on [0,1], peak at 0.5
        return Mathf.Lerp(_min, _max, t);
    }
}
