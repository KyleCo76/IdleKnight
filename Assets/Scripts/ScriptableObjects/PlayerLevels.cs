using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerLevels", menuName = "Scriptable Objects/PlayerLevels")]
    public class PlayerLevels : ScriptableObject
    {
        [System.Serializable]
        public struct LevelEntry
        {
            public int Level;
            public float ExperienceMultiplierRequired;
        }
        public LevelEntry[] Levels;
        public int BaseExperienceToLevelUp = 100;

        public float GetLevelMultiplier(int _level)
        {
            foreach (var entry in Levels) {
                if (entry.Level == _level) {
                    return entry.ExperienceMultiplierRequired;
                }
            }
            Debug.LogWarning($"Level {_level} not found in Levels.");
            return 1f;
        }
    }
}
