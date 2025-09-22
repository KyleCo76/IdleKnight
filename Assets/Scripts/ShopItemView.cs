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

    public void Bind(ShopItemDatabase.ShopItemEntry _itemData, System.Action<ShopItemDatabase.ShopItemEntry> _onItemClicked)
    {
        Item = _itemData;
        icon.sprite = _itemData.Icon;
        nameText.text = _itemData.DisplayName;
        priceText.text = _itemData.Cost.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onItemClicked?.Invoke(Item));
    }
}
