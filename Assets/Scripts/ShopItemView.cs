using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [SerializeField, Tooltip("The button that will be used to interact with the item")]
    private Button button;
    [SerializeField, Tooltip("The icon of the item")]
    private Image icon;
    [SerializeField, Tooltip("The TextMeshProUGUI that will display the item's name")]
    private TMPro.TMP_Text nameText;
    [SerializeField, Tooltip("The TextMeshProUGUI that will display the item's price")]
    private TMPro.TMP_Text priceText;

    [HideInInspector] public ShopItemDatabase.ShopItemEntry Item;
    
    private SuperDatabase superDatabase;

    private void Awake()
    {
        superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
        if (!superDatabase) {
            Debug.LogError("No SuperDatabase found in Resources/ScriptableObjects.");
            enabled = false;
        }
    }

    public void Bind(ShopItemDatabase.ShopItemEntry _itemData, System.Action<ShopItemDatabase.ShopItemEntry> _onItemClicked)
    {
        superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
        if (!superDatabase) {
            Debug.LogError("No SuperDatabase found in Resources/ScriptableObjects.");
            enabled = false;
        }
        Item = _itemData;
        icon.sprite = superDatabase.GetSpriteForSuper(_itemData.Id);
        nameText.text = _itemData.DisplayName;
        priceText.text = _itemData.Cost.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onItemClicked?.Invoke(Item));
    }
}
