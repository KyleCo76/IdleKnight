using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ShopItemDatabase", menuName = "Scriptable Objects/ShopItemDatabase")]
    public class ShopItemDatabase : ScriptableObject
    {
        [Serializable]
        public struct ShopItemEntry : IEquatable<ShopItemEntry>
        {
            public SuperType Id;
            public string DisplayName;
            public Sprite Icon;
            public int Cost;
            public int LevelRequirement;
        
            public bool Equals(ShopItemEntry _otherEntry)
            {
                return Id == _otherEntry.Id;
            }
            public override bool Equals(object _obj)
            {
                return _obj is ShopItemEntry other && Equals(other);
            }
            public override int GetHashCode()
            {
                return (int) Id;
            }
        }
    
        public ShopItemEntry[] ShopItems;
    
        public ShopItemEntry GetRandomShopItem(int _playerLevel)
        {
            if (ShopItems.Length == 0)
                return new ShopItemEntry();
        
            var possiblePrefabs = new Dictionary<ShopItemEntry, int>();
            var totalWeight = 0;
            foreach (var entry in ShopItems) {
                if (entry.LevelRequirement <= _playerLevel) {
                    var weight = 1 + entry.LevelRequirement;
                    totalWeight += weight;
                    possiblePrefabs.Add(entry, weight);
                }
            }
            var roll = Random.Range(0f, 1f);
            var bias = Mathf.Pow(roll, 2);
            roll = 1 + bias * (totalWeight - 1);
        
            var cumulativeWeight = 0f;
            foreach (var entry in possiblePrefabs) {
                cumulativeWeight += entry.Value;
                if (roll <= cumulativeWeight) {
                    return entry.Key;
                }
            }
        
            // Fallback to a random item if no item is found
            Debug.LogWarning("No item found for player level. Falling back to random item.");
            return ShopItems[Random.Range(0, ShopItems.Length)];
        }
    }
}
