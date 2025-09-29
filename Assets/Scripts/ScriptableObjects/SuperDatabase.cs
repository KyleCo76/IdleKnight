using Game;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "SuperDatabase", menuName = "Scriptable Objects/SuperDatabase")]
    public class SuperDatabase : ScriptableObject
    {

        [System.Serializable]
        public struct SuperEntry
        {
            public SuperType Type;
            public int PowerLevel;
            public float SpeedMultiplier;
            public GameObject Prefab;
            public int BaseDamage;
            public SecondarySuperData SecondaryData;
        }

        [System.Serializable]
        public struct SecondarySuperData
        {
            public float Frequency;
            public int MaxEffects;
            public int SecondaryDamageAmount;
        }

        public SuperEntry[] SuperPowerLevels;

        public RuntimeAnimatorController GetAnimatorForSuper(SuperType _superType)
        {
            foreach (var entry in SuperPowerLevels) {
                if (entry.Type == _superType) {
                    if (entry.Prefab != null && entry.Prefab.TryGetComponent<Animator>(out var animator)) {
                        return animator.runtimeAnimatorController;
                    }
                }
            }
            Debug.LogWarning($"Animator for SuperType {_superType} not found.");
            return null;
        }

        public int GetBaseDamageForSuper(SuperType _superType)
        {
            foreach (var entry in SuperPowerLevels) {
                if (entry.Type == _superType) {
                    return entry.BaseDamage;
                }
            }
            Debug.LogWarning($"BaseDamage for SuperType {_superType} not found.");
            return -1;
        }
        
        public int GetPowerLevelForSuper(SuperType _superType)
        {
            foreach (var entry in SuperPowerLevels) {
                if (entry.Type == _superType) {
                    return entry.PowerLevel;
                }
            }
            Debug.LogWarning($"PowerLevel for SuperType {_superType} not found.");
            return -1;
        }

        public GameObject GetPrefabForSuper(SuperType _superType)
        {
            foreach (var entry in SuperPowerLevels) {
                if (entry.Type == _superType) {
                    return entry.Prefab;
                }
            }
            Debug.LogWarning($"Prefab for SuperType {_superType} not found.");
            return null;
        }

        public bool GetSecondaryDataForSuper(SuperType _superType, out float _frequency, out int _damageAmount, out int _maxEffects)
        {
            foreach (var entry in SuperPowerLevels) {
                if (entry.Type == _superType) {
                    _frequency = entry.SecondaryData.Frequency;
                    _damageAmount = entry.SecondaryData.SecondaryDamageAmount;
                    _maxEffects = entry.SecondaryData.MaxEffects;
                    return true;
                }
            }
            Debug.LogWarning($"SecondaryData for SuperType {_superType} not found.");
            _frequency = -1f;
            _damageAmount = -1;
            _maxEffects = -1;
            return false;
        }

        public float GetSpeedMultiplierForSuper(SuperType _superType)
        {
            foreach (var entry in SuperPowerLevels) {
                if (entry.Type == _superType) {
                    return entry.SpeedMultiplier;
                }
            }
            Debug.LogWarning($"SpeedMultiplier for SuperType {_superType} not found.");
            return 1f;
        }

        public Sprite GetSpriteForSuper(SuperType _superType)
        {
            foreach (var entry in SuperPowerLevels) {
                if (entry.Type == _superType) {
                    if (entry.Prefab != null && entry.Prefab.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) {
                        return spriteRenderer.sprite;
                    }
                }
            }
            Debug.LogWarning($"Sprite for SuperType {_superType} not found.");
            return null;
        }
    }
}
