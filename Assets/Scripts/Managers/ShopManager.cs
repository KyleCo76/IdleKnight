using UnityEngine;
using Game;
using NUnit.Framework;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Player;

public class ShopManager : MonoBehaviour
{
    [FoldoutGroup("Item Costs"), SerializeField, Tooltip("The base cost of the item.")]
    private int baseItemCost = 10;
    [FoldoutGroup("Item Costs"), SerializeField, Tooltip("The cost increase per item level.")]
    private int itemCostPerLevel = 25;

    [FoldoutGroup("Item Stats"), SerializeField, Tooltip("The base damage of the item.")]
    private float baseItemDamage = 25f;
    [FoldoutGroup("Item Stats"), SerializeField, Tooltip("The damage increase per item level.")]
    private float itemDamagePerLevel = 5f;
    [FoldoutGroup("Item Stats"), SerializeField, Tooltip("The base speed of the item.")]
    private float baseItemSpeed = 100f;

    public static ShopManager Instance;

    // Cached Components
    private PlayerController playerController;
    private SuperDatabase superDatabase;

    private readonly List<SuperStats> shopItems = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }

        Instance = this;

        superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
        if (superDatabase == null) {
            Debug.LogError("SuperDatabase not found in Resources/ScriptableObjects.");
            enabled = false;
            return;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null || !playerObj.TryGetComponent<PlayerController>(out playerController)) {
            Debug.LogError("Player Controller or PlayerObject missing in ShopManger");
            enabled = false;
            return;
        }
    }


    public void BuyItem(int _indexItem)
    {
        if (_indexItem < 1 || _indexItem > shopItems.Count) {
            Debug.LogError($"Invalid item index: {_indexItem}");
            return;
        }

        var selectedItem = shopItems[_indexItem - 1];
        if (RunScoreManager.Instance.CurrentScore >= selectedItem.Cost) {
            RunScoreManager.Instance.AddScore(-selectedItem.Cost);
            playerController.SetSuper(selectedItem.Prefab, selectedItem.Damage, selectedItem.Speed);
            UIManager.Instance.ToggleShopSkipButton(false);
        } else {
            Debug.Log("Not enough score to buy this item.");
        }
    }

    private (GameObject, SuperType, Sprite, RuntimeAnimatorController, int) ChooseShopItem(int _playerLevel)
    {
        List<SuperType> possibleSupers = new();

        foreach (var superType in System.Enum.GetValues(typeof(SuperType))) {
            var superPowerLevel = superDatabase.GetPowerLevelForSuper((SuperType)superType);
            if (superPowerLevel <= _playerLevel) {
                possibleSupers.Add((SuperType)superType);
            }
        }

        if (possibleSupers.Count == 0) {
            Debug.LogWarning($"No available super types for player level {_playerLevel}");
            return (null, SuperType.None, null, null, 0);
        }

        var selectedSuper = possibleSupers[Random.Range(0, possibleSupers.Count)];
        while (selectedSuper == SuperType.None) {
            selectedSuper = possibleSupers[Random.Range(0, possibleSupers.Count)];
        }
        var itemPrefab = superDatabase.GetPrefabForSuper(selectedSuper);
        var itemSprite = superDatabase.GetSpriteForSuper(selectedSuper);
        var itemAnimator = superDatabase.GetAnimatorForSuper(selectedSuper);
        var itemLevel = superDatabase.GetPowerLevelForSuper(selectedSuper);

        return (itemPrefab, selectedSuper, itemSprite, itemAnimator, itemLevel);
    }

    public void OpenShop(int _playerLevel)
    {
        shopItems.Clear();
        var (item1, item1SuperType, item1Sprite, item1Animator, item1Level) = ChooseShopItem(_playerLevel);
        var (item2, item2SuperType, item2Sprite, item2Animator, item2Level) = ChooseShopItem(_playerLevel);
        var (item3, item3SuperType, item3Sprite, item3Animator, item3Level) = ChooseShopItem(_playerLevel);

        UIManager.Instance.SetShopItemIcon(item1Sprite, item1Animator, 1);
        UIManager.Instance.SetShopItemIcon(item2Sprite, item2Animator, 2);
        UIManager.Instance.SetShopItemIcon(item3Sprite, item3Animator, 3);

        shopItems.Add(new SuperStats {
            Prefab = item1,
            Type = item1SuperType,
            Damage = GetItemDamage(_playerLevel, item1Level),
            Speed = superDatabase.GetSpeedMultiplierForSuper(item1SuperType) * baseItemSpeed,
        });
        shopItems.Add(new SuperStats {
            Prefab = item2,
            Type = item2SuperType,
            Damage = GetItemDamage(_playerLevel, item2Level),
            Speed = superDatabase.GetSpeedMultiplierForSuper(item2SuperType) * baseItemSpeed,
        });
        shopItems.Add(new SuperStats {
            Prefab = item3,
            Type = item3SuperType,
            Damage = GetItemDamage(_playerLevel, item3Level),
            Speed = superDatabase.GetSpeedMultiplierForSuper(item3SuperType) * baseItemSpeed,
        });

        shopItems[0].Cost = GetItemCost(_playerLevel, item1Level);
        shopItems[1].Cost = GetItemCost(_playerLevel, item2Level);
        shopItems[2].Cost = GetItemCost(_playerLevel, item3Level);

        UIManager.Instance.SetShopItemCost(shopItems[0].Cost, 1);
        UIManager.Instance.SetShopItemCost(shopItems[1].Cost, 2);
        UIManager.Instance.SetShopItemCost(shopItems[2].Cost, 3);
    }

    private int GetItemCost(int _playerLevel, int _powerLevel)
    {
        int cost = (baseItemCost + _playerLevel) + (_powerLevel * itemCostPerLevel);
        return cost;
    }

    private float GetItemDamage(int _playerLevel, int _powerLevel)
    {
        float damage = (baseItemDamage + _playerLevel) * (_powerLevel * itemDamagePerLevel);
        return damage;
    }
}

public class SuperStats
{
    public GameObject Prefab;
    public SuperType Type;
    public float Damage;
    public float Speed;
    public int Cost;
}
