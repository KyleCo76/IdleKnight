using Game;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PowerUpDatabase", menuName = "Scriptable Objects/PowerUpDatabase")]
    public class PowerUpDatabase : ScriptableObject
    {
        [System.Serializable]
        public struct ItemPrefabDatabaseEntry
        {
            public GameObject Prefab;
            public bool IsTemporary;
            public bool HasTickIcon;
        }

        [System.Serializable]
        public struct ItemSpriteEntry
        {
            public PowerUpType PowerUpType;
            public Sprite Sprite;
            public bool UseTickIcon;
            public bool IsTemporary;
        }

        [System.Serializable]
        public struct ItemSpawnWeightEntry
        {
            public PowerUpType PowerUpType;
            [Range(0f, 1f)]
            public float Weight;
        }

        public ItemSpriteEntry[] Sprites;
        public ItemPrefabDatabaseEntry[] Prefabs;
        public ItemSpawnWeightEntry[] SpawnWeights;


        public GameObject GetPrefabForPowerUpType(PowerUpType _powerUpType, bool _isTemporary)
        {
            var typeUsesTickIcon = false;
            foreach (var sprite in Sprites) {
                if (sprite.PowerUpType == _powerUpType && sprite.IsTemporary == _isTemporary) {
                    typeUsesTickIcon = sprite.UseTickIcon;
                    break;
                }
            }

            foreach (var prefab in Prefabs) {
                if (prefab.IsTemporary == _isTemporary && prefab.HasTickIcon == typeUsesTickIcon) {
                    return prefab.Prefab;
                }
            }

            Debug.LogWarning($"Prefab for PowerUpType {_powerUpType} not found in Prefabs.");
            return null;
        }

        public Sprite GetSpriteForPowerUpType(PowerUpType _powerUpType, bool _isTemporary)
        {
            foreach (var sprite in Sprites) {
                if (sprite.PowerUpType == _powerUpType && sprite.IsTemporary == _isTemporary) {
                    return sprite.Sprite;
                }
            }
            Debug.LogWarning($"Sprite for PowerUpType {_powerUpType} as temp {_isTemporary} not found in Sprites.");
            return null;
        }
    }
}
