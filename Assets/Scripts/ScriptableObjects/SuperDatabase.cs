using UnityEngine;
using Game;

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
