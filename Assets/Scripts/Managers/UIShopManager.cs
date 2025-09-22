using System.Collections.Generic;
using Player;
using ScriptableObjects;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public partial class UIManager
    {
        [SerializeField,Tooltip("The prefab for all shop items")]
        private ShopItemView shopItemPrefab;
        [FoldoutGroup("Item Stats"), SerializeField, Tooltip("The base speed of the item.")]
        private float baseItemSpeed = 100f;

        private readonly List<ShopItemDatabase.ShopItemEntry> shopItems = new();
        private readonly List<ShopItemView> shopItemViews = new();

        // Cached Components
        private ShopItemDatabase shopDatabase;
        private SuperDatabase superDatabase;
        private PlayerController playerController;
        private GameObject shopCanvasObject;
        private Transform shopItemsParent;
        private Button leaveShopButton;
        private TextMeshProUGUI shopPointsText;
        private TextMeshProUGUI shopPowerUpText;
        private TextMeshProUGUI shopGemText;

        private GameObject openShop;

        private void AwakeShopManager()
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null || !playerObject.TryGetComponent(out playerController)) {
                Debug.LogError("Player not found or missing PlayerController component.");
                return;
            }
            shopCanvasObject = GameObject.Find("Shop");
            if (!shopCanvasObject) {
                Debug.LogError("No Shop GameObject found in scene.");
                return;           
            }
            
            shopDatabase = Resources.Load<ShopItemDatabase>("ScriptableObjects/ShopItemDatabase");
            if (!shopDatabase) {
                Debug.LogError("ShopItemDatabase not found in Resources/ScriptableObjects.");
                return;           
            }
            
            superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
            if (!superDatabase) {
                Debug.LogError("SuperDatabase not found in Resources/ScriptableObjects.");
                return;           
            }
            
            var shopItemsParentObj = GameObject.Find("ShopItemHolder");
            if (!shopItemsParentObj) {
                Debug.LogError("No ShopItemHolder GameObject found in scene.");
                return;
            }
            shopItemsParent = shopItemsParentObj.transform;
            
            var leaveShopButtonObj = GameObject.Find("LeaveShopButton");
            if (!leaveShopButtonObj) {
                Debug.LogError("No LeaveShopButton GameObject found in scene.");
                return;
            }
            if (!leaveShopButtonObj.TryGetComponent(out leaveShopButton)) {
                Debug.LogError("No LeaveShopButton found in scene.");
                return;           
            }
            
            var shopPointsTextObj = GameObject.Find("ShopPointsText");
            if (!shopPointsTextObj) {
                Debug.LogError("No ShopPointsText GameObject found in scene.");
                return;
            }
            if (!shopPointsTextObj.TryGetComponent(out shopPointsText)) {
                Debug.LogError("No ShopPointsText found in scene.");
                return;           
            }
            
            var shopPowerUpTextObj = GameObject.Find("ShopPowerUpText");
            if (!shopPowerUpTextObj) {
                Debug.LogError("No ShopPowerUpText GameObject found in scene.");
            }

            if (!shopPowerUpTextObj.TryGetComponent(out shopPowerUpText)) {
                Debug.LogError("No ShopPowerUpText found in scene.");
                return;           
            }
            
            var shopGemTextObj = GameObject.Find("ShopGemsText");
            if (!shopGemTextObj) {
                Debug.LogError("No ShopGemsText GameObject found in scene.");
            }

            if (!shopGemTextObj.TryGetComponent(out shopGemText)) {
                Debug.LogError("No ShopGemText found in scene.");
                return;           
            }
            
            SetupShopUI();
            DisplayShopItems();
        }
        
        
        public void ActivateShop(bool _activate, GameObject _shop = null)
        {
            openShop = _shop;
            if (_activate) {
                DisplayShopItems();
                shopCanvasObject.SetActive(true);
                uiCanvasObject.SetActive(false);
            } else {
                uiCanvasObject.SetActive(true);
                shopCanvasObject.SetActive(false);
            }
            Time.timeScale = _activate ? 0f : 1f;
        }
        
        private void DisplayShopItems()
        {
            shopPointsText.text = RunScoreManager.Instance.CurrentScore.ToString();
            
            if (!shopDatabase || !shopItemsParent || !shopItemPrefab || !shopPointsText || !shopPowerUpText ||
                !shopGemText || !RunScoreManager.Instance) {
                return;           
            }

            var playerLevel = RunScoreManager.Instance.GetPlayerLevel();
            var shopItemCount = Mathf.Min(9, playerLevel * 2);
            shopItems.Clear();
            for (int i = 0; i < shopItemCount; i++) {
                shopItems.Add(shopDatabase.GetRandomShopItem(playerLevel));
            }

            foreach (var view in shopItemViews) {
                Destroy(view.gameObject);
            }
            shopItemViews.Clear();

            foreach (var item in shopItems) {
                var view = Instantiate(shopItemPrefab, shopItemsParent);
                view.Bind(item, OnItemPurchased);
                shopItemViews.Add(view);
            }
        }

        private void OnItemPurchased(ShopItemDatabase.ShopItemEntry _item)
        {
            if (!RunScoreManager.Instance || RunScoreManager.Instance.CurrentScore < _item.Cost) {
                return;
                // Will spawn a message here later
            }
            
            RunScoreManager.Instance.AddScore(-_item.Cost);
            playerController.SetSuper(superDatabase.GetPrefabForSuper(_item.Id),
                superDatabase.GetBaseDamageForSuper(_item.Id), superDatabase.GetSpeedMultiplierForSuper(_item.Id) * baseItemSpeed);

            foreach (var view in shopItemViews) {
                if (view.Item.Id == _item.Id) {
                    Destroy(view.gameObject);
                    shopItemViews.Remove(view);
                    break;
                }
            }
        }

        private void SetupShopUI()
        {
            leaveShopButton.onClick.RemoveAllListeners();
            leaveShopButton.onClick.AddListener(ExitShop);
        }

        private void ExitShop()
        {
            uiCanvasObject.SetActive(true);
            shopCanvasObject.SetActive(false);
            ShopSpawner.Instance.LeaveShop(openShop);
            openShop = null;
            Time.timeScale = 1f;
        }
    }
}
