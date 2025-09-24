using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ShopItemDatabase", menuName = "Scriptable Objects/ShopItemDatabase")]
    public class ShopItemDatabase : ScriptableObject
    {
        private SuperDatabase superDatabase;


        private void OnEnable()
        {
            superDatabase = Resources.Load<SuperDatabase>("ScriptableObjects/SuperDatabase");
            if (!superDatabase) {
                Debug.LogError("No SuperDatabase found in Resources/ScriptableObjects.");
            }
        }

        [Serializable]
        public struct ShopSuperEntry : IEquatable<ShopSuperEntry>
        {
            public SuperType Id;
            public string DisplayName;
            public int Cost;
        
            public bool Equals(ShopSuperEntry _otherEntry)
            {
                return Id == _otherEntry.Id;
            }
            public override bool Equals(object _obj)
            {
                return _obj is ShopSuperEntry other && Equals(other);
            }
            public override int GetHashCode()
            {
                return (int) Id;
            }
        }

        [Serializable]
        public struct ShopItemEntry : IEquatable<ShopItemEntry>
        {
            public PowerUpType Id;
            public string DisplayName;
            public int Cost;
            public float MultiplierMax;
            public float AmountMax;
            public Sprite Sprite;
            public bool UseTickIcon;
            public int LevelRequirement;
            public float SpawnWeight;
            
            private float setItemMultiplier;
            private float setItemAmount;
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
            
            public (float, float) GetItemMultiplierAndAmount()
            {
                return (setItemMultiplier, setItemAmount);
            }
            public void SetItemMultiplierAndAmount(float _multiplier, float _amount)
            {
                setItemMultiplier = _multiplier;
                setItemAmount = _amount;
            }
        }
    
        [FormerlySerializedAs("ShopItems")] public ShopSuperEntry[] ShopSupers;
        public ShopItemEntry[] ShopItems;
    
        public ShopSuperEntry GetRandomShopSuper(int _playerLevel)
        {
            if (ShopSupers.Length == 0 || !superDatabase)
                return new ShopSuperEntry();
        
            var possiblePrefabs = new Dictionary<ShopSuperEntry, float>();
            var totalWeight = 0;
            foreach (var entry in ShopSupers) {
                var levelRequirement = superDatabase.GetPowerLevelForSuper(entry.Id);
                if (levelRequirement <= _playerLevel) {
                    var weight = 1 + levelRequirement;
                    totalWeight += weight;
                    possiblePrefabs.Add(entry, weight);
                }
            }

            if (possiblePrefabs.Count == 0) {
                Debug.LogError("No supers found for player level.");
                return new ShopSuperEntry();
            } else if (possiblePrefabs.Count == 1) {
                return possiblePrefabs.First().Key;
            }
            
            return GetWeightedEntry(possiblePrefabs, totalWeight);
        }

        public ShopItemEntry GetRandomShopItem(int _playerLevel)
        {
            if (ShopItems.Length == 0)
                return new ShopItemEntry();
            
            var possibleItems = new Dictionary<ShopItemEntry, float>();
            var totalWeight = 0f;
            foreach (var item in ShopItems) {
                var levelRequirement = item.LevelRequirement;
                if (levelRequirement <= _playerLevel) {
                    totalWeight += item.SpawnWeight;
                    possibleItems.Add(item, item.SpawnWeight);
                }
            }

            if (possibleItems.Count == 0) {
                Debug.LogError("No items found for player level.");
                return new ShopItemEntry();
            } else if (possibleItems.Count == 1) {
                return possibleItems.First().Key;
            }
            
            return GetWeightedEntry(possibleItems, totalWeight);
        }

        private static T GetWeightedEntry<T>(Dictionary<T, float> _entries, float _totalWeight)
        {
            var roll = Random.Range(0f, _totalWeight);
            var cumulativeWeight = 0f;
            foreach (var entry in _entries)
            {
                cumulativeWeight += entry.Value;
                if (roll <= cumulativeWeight) {
                    return entry.Key;
                }
            }

            return _entries.First().Key;
        }
    }
}
