using System.Collections.Generic;
using Game;
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
        [FoldoutGroup("Super Stats"), SerializeField, Tooltip("The base speed of the item.")]
        private float baseItemSpeed = 100f;

        [FoldoutGroup("Shop Items"), SerializeField, Tooltip("The max number of supers to display in the shop.")]
        private int maxShopSuperCount = 5;
        [FoldoutGroup("Shop Items"), SerializeField, Tooltip("The max number of items to display in the shop.")]
        private int maxShopItemCount = 9;

        private readonly List<ShopItemDatabase.ShopSuperEntry> shopSupers = new();
        private readonly List<ShopItemDatabase.ShopItemEntry> shopItems = new();
        
        private readonly List<ShopItemView> shopItemViews = new();
        
        private bool isShopOpen;

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
            shopCanvasObject = GameObject.Find("ShopMenu");
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
            if (isShopOpen || !shopDatabase || !shopItemsParent || !shopItemPrefab || !shopPointsText || !shopPowerUpText ||
                !shopGemText || !RunScoreManager.Instance) {
                return;           
            }

            isShopOpen = true;
            
            shopPointsText.text = RunScoreManager.Instance.RunScore.ToString();
            shopPowerUpText.text = RunScoreManager.Instance.PowerUpScore.ToString();
            shopGemText.text = RunScoreManager.Instance.GemsCount.ToString();

            var playerLevel = RunScoreManager.Instance.GetPlayerLevel();
            var shopItemCounts = GetShopItemCounts(maxShopSuperCount, maxShopItemCount, playerLevel * 2 + 2);
            shopSupers.Clear();
            shopItems.Clear();
            for (int i = 0; i < shopItemCounts.Item1; i++) {
                shopSupers.Add(shopDatabase.GetRandomShopSuper(playerLevel));
            }

            for (int i = 0; i < shopItemCounts.Item2; i++) {
                shopItems.Add(shopDatabase.GetRandomShopItem(playerLevel));
            }

            foreach (var view in shopItemViews) {
                Destroy(view.gameObject);
            }
            shopItemViews.Clear();

            var superCount = 0;
            var itemCount = 0;
            for (int i = 0; i < shopItemCounts.Item1 + shopItemCounts.Item2; i++) {
                bool useSuper = false;
                if (superCount >= shopItemCounts.Item2 && itemCount >= shopItemCounts.Item1)
                    return;
                else if (superCount < shopItemCounts.Item2 && itemCount < shopItemCounts.Item1)
                    useSuper = Random.value < 0.5f;
                else if (superCount < shopItemCounts.Item2)
                    useSuper = true;
                
                var itemIndex = itemCount;
                var superIndex = superCount;
                if (useSuper && superIndex >= shopSupers.Count)
                    superIndex = Random.Range(0, shopSupers.Count);
                else if (!useSuper && itemIndex >= shopItems.Count)
                    itemIndex = Random.Range(0, shopItems.Count);

                var view = Instantiate(shopItemPrefab, shopItemsParent);
                if (useSuper) {
                    view.Bind(shopSupers[superIndex], OnSuperPurchased);
                } else {
                    view.Bind(shopItems[itemIndex], OnItemPurchased);
                }
                shopItemViews.Add(view);

                if (useSuper)
                    superCount++;
                else
                    itemCount++;
            }
        }
        
        private void ExitShop()
        {
            uiCanvasObject.SetActive(true);
            shopCanvasObject.SetActive(false);
            ShopSpawner.Instance.LeaveShop(openShop);
            openShop = null;
            Time.timeScale = 1f;
            isShopOpen = false;
        }

        private (int, int) GetShopItemCounts(int _maxSuperCount, int _maxItemCount, int _maxTotalCount)
        {
            if (_maxTotalCount >= (_maxSuperCount + _maxItemCount))
                return (PickMiddleHeavyRandom(1, _maxSuperCount), PickMiddleHeavyRandom(1, _maxItemCount));
            
            var itemCount = PickMiddleHeavyRandom(1, _maxItemCount);
            var newSuperMax = _maxTotalCount - itemCount;
            if (newSuperMax <= 0)
                return (0, Mathf.Min(itemCount, _maxTotalCount)); // Clamp to item count
            
            var superCount = PickMiddleHeavyRandom(1, newSuperMax);
            return (superCount, itemCount);
        }
        
        private void OnItemPurchased(ShopItemDatabase.ShopItemEntry _item)
        {
            if (!RunScoreManager.Instance || RunScoreManager.Instance.PowerUpScore < _item.Cost) {
                return;
                // Will spawn a message here later
            }
            
            RunScoreManager.Instance.AddPowerUpScore(-_item.Cost);

            var multiplierAndAmount = _item.GetItemMultiplierAndAmount();
            
            playerController.ActivatePowerUp(new PowerUpData(_item.Id, 0f, multiplierAndAmount.Item1, multiplierAndAmount.Item2));

            foreach (var view in shopItemViews) {
                if (view.Item.Id == _item.Id) {
                    Destroy(view.gameObject);
                    shopItemViews.Remove(view);
                    break;
                }
            }
        }

        private void OnSuperPurchased(ShopItemDatabase.ShopSuperEntry _super)
        {
            if (!RunScoreManager.Instance || RunScoreManager.Instance.RunScore < _super.Cost) {
                return;
                // Will spawn a message here later
            }

            RunScoreManager.Instance.AddScore(-_super.Cost);
            
            // Check for Secondary effect data
            if (superDatabase.GetSecondaryDataForSuper(_super.Id, out var frequency, out var damage, out var maxCount))
                playerController.SetSuper(superDatabase.GetPrefabForSuper(_super.Id),
                    superDatabase.GetBaseDamageForSuper(_super.Id),
                    superDatabase.GetSpeedMultiplierForSuper(_super.Id) * baseItemSpeed, frequency, damage, maxCount);
            else
                playerController.SetSuper(superDatabase.GetPrefabForSuper(_super.Id),
                    superDatabase.GetBaseDamageForSuper(_super.Id),
                    superDatabase.GetSpeedMultiplierForSuper(_super.Id) * baseItemSpeed);

            foreach (var view in shopItemViews) {
                if (view.Super.Id == _super.Id) {
                    Destroy(view.gameObject);
                    shopItemViews.Remove(view);
                    break;
                }
            }
        }

        private int PickMiddleHeavyRandom(int _min, int _max)
        {
            if (_min > _max) { (_min, _max) = (_max, _min); }

            int count = _max - _min + 1;
            if (count <= 1) return _min;

            float t = 0.5f * (Random.value + Random.value); // [0,1], peak at 0.5
            
            // map to [min, max] and round to nearest int inside bounds
            int value = Mathf.RoundToInt(Mathf.Lerp(_min, _max, t));
            
            return Mathf.Clamp(value, _min, _max);
        }

        private void SetupShopUI()
        {
            leaveShopButton.onClick.RemoveAllListeners();
            leaveShopButton.onClick.AddListener(ExitShop);
            shopCanvasObject.SetActive(false);
        }
    }
}
