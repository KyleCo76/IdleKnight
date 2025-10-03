using System;
using System.Collections.Generic;
using Game;
using Player;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public partial class UIManager
    {
        private int overallMaxTier;

        private GameObject combatSkillTreeParent;
        private GameObject defenseSkillTreeParent;
        private GameObject gatheringSkillTreeParent;
        private GameObject skillItemPrefab;
        private TextMeshProUGUI skillNameText;
        private TextMeshProUGUI skillTierText;
        private TextMeshProUGUI skillEffectText;
        private TextMeshProUGUI skillEffectDifferenceText;
        private TextMeshProUGUI skillEffectDifferenceResultText;
        private TextMeshProUGUI skillFlairText;
        private TextMeshProUGUI skillCostText;
        private TextMeshProUGUI availableSkillPointsText;
        private Button unlockButton;
        
        private readonly Transform[][] categoryParents = new Transform[][]
        {
            new Transform[3],
            new Transform[2],
            new Transform[2]
        };
        
        /// <summary>
        /// Represents the maximum tier levels per category for each skill tree.
        /// The first dimension corresponds to the skill tree (e.g. combat, defense, gathering),
        /// and the second dimension corresponds to the categories within each tree.
        /// </summary>
        private readonly int[][] maxTiersByCategory = new int[][] { new int[3], new int[2], new int[2] };

        /// <summary>
        /// Represents the total points spent in each skill tree by the player.
        /// Each index corresponds to a specific skill tree, where the value at
        /// the index accumulates the cost of all purchased skills within that tree.
        /// </summary>
        private readonly int[] pointsSpentByTree = new int[3];

        /// <summary>
        /// Stores the references to skill item view GameObjects organized by tree and category.
        /// The outer array represents the tree index, the inner array represents the category,
        /// and the associated list contains the skill item GameObjects for that category.
        /// </summary>
        private readonly List<GameObject>[][] skillItemViews = new[]
        {
            new[] { new List<GameObject>(), new List<GameObject>(), new List<GameObject>() }, // Combat (3 categories)
            new[] { new List<GameObject>(), new List<GameObject>() },                         // Defense (2 categories)
            new[] { new List<GameObject>(), new List<GameObject>() }                          // Gathering (2 categories)
        };

        private readonly List<SkillItemView>[][] skillItemComponents = new[]
        {
            new[] { new List<SkillItemView>(), new List<SkillItemView>(), new List<SkillItemView>() },
            new[] { new List<SkillItemView>(), new List<SkillItemView>() },
            new[] { new List<SkillItemView>(), new List<SkillItemView>() }
        };

        private const float AbilityDamageMultiplier = 1.1f;
        private const float AbilityCooldownMultiplier = 1.05f;
        private const float AbilityRangeMultiplier = 1.15f;
        
        private const float SuperDamageMultiplier = 1.2f;
        private const float SuperCooldownMultiplier = 1.1f;
        
        private const float MagnetRangeMultiplier = 1.15f;
        private const float MagnetCooldownMultiplier = 1.1f;
        

        private void AwakeSkillsManager()
        {
            if (!skillItemPrefab) {
                skillItemPrefab = Resources.Load<GameObject>("Prefabs/SkillToggle");
                if (!skillItemPrefab)
                    throw new SystemException("Failed to load SkillToggle prefab.");
            }
            FindSkillSubComponents();
            FindSkillParents();
            
            InitializeSkillSubComponents();
            
            CalculateOverallMaxTier();
        }

        
        private void CalculateOverallMaxTier()
        {
            int max = 0;
            foreach (var tree in maxTiersByCategory) {
                foreach (var category in tree) {
                    if (category > max)
                        max = category;
                }
            }

            overallMaxTier = max;
        }
        
        private static string ConvertIntToRoman(int _number)
        {
            if (_number < 1 || _number > 3999)
                throw new ArgumentOutOfRangeException(nameof(_number), "Number must be between 1 and 3999");

            var map = new (int value, string roman)[]
            {
                (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"), (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
                (10, "X"),
                (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
            };
            
            var builder = new System.Text.StringBuilder();
            foreach (var (value, roman) in map)
            {
                while (_number >= value)
                {
                    builder.Append(roman);
                    _number -= value;
                }
            }
            return builder.ToString();
        }

        private void DisplaySkillInfo(SkillData _skillData)
        {
            skillNameText.text = _skillData.Name;
            skillTierText.text = _skillData.TierRoman;
            skillEffectText.text = _skillData.EffectText;
            skillEffectDifferenceText.text = _skillData.EffectText;
            switch (_skillData.Effect) {
                case SkillTypes.AbilityDamage:
                    skillEffectDifferenceResultText.text =
                        $"{(PlayerDataStorage.AbilityDamage * _skillData.EffectPower):0.00}";
                    break;
                case SkillTypes.AbilityCooldown:
                    skillEffectDifferenceResultText.text =
                        $"{(PlayerDataStorage.AbilityCooldown / _skillData.EffectPower):0.00}";
                    break;
                case SkillTypes.AbilityRange:
                    skillEffectDifferenceResultText.text =
                        $"{(PlayerDataStorage.AbilityRange * _skillData.EffectPower):0.00}";
                    break;
                case SkillTypes.SuperDamage:
                    skillEffectDifferenceResultText.text =
                        $"{(PlayerDataStorage.SuperDamage * _skillData.EffectPower):0.00}";
                    break;
                case SkillTypes.SuperCooldown:
                    skillEffectDifferenceResultText.text =
                        $"{(PlayerDataStorage.BaseSuperCooldown / _skillData.EffectPower):0.00}";
                    break;
                case SkillTypes.MagnetRange:
                    skillEffectDifferenceResultText.text =
                        $"{(PlayerDataStorage.MagnetRange * _skillData.EffectPower):0.00}";
                    break;
                case SkillTypes.MagnetCooldown:
                    skillEffectDifferenceResultText.text =
                        $"{(PlayerDataStorage.MagnetCooldown / _skillData.EffectPower):0.00}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(_skillData.Effect.ToString());
            }

            skillFlairText.text = _skillData.FlairText;

            skillCostText.text = $"Cost: {_skillData.Cost} Skill Points";
            unlockButton.interactable = (_skillData.Cost <= RunScoreManager.Instance.GemsCount) &&
                                        maxTiersByCategory[_skillData.TreePosition.x][_skillData.TreePosition.y] + 1 >=
                                        _skillData.TierNumerical &&
                                        pointsSpentByTree[_skillData.TreePosition.x] >= _skillData.TreeRequirement.y;
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(() => PurchaseSkill(_skillData));
        }

        private void FindSkillParents()
        {
            combatSkillTreeParent = GameObject.Find("CombatSkillTree");
            if (!combatSkillTreeParent)
                throw new SystemException("Failed to find CombatSkillTree GameObject.");
            defenseSkillTreeParent = GameObject.Find("DefenseSkillTree");
            if (!defenseSkillTreeParent)
                throw new SystemException("Failed to find DefenseSkillTree GameObject.");
            gatheringSkillTreeParent = GameObject.Find("GatheringSkillTree");
            if (!gatheringSkillTreeParent)
                throw new SystemException("Failed to find GatheringSkillTree GameObject.");

            for (int tree = 0; tree < categoryParents.Length; tree++) {
                var treeParent = tree == 0 ? combatSkillTreeParent :
                    tree == 1 ? defenseSkillTreeParent : gatheringSkillTreeParent;
                for (var category = 0; category < categoryParents[tree].Length; category++) {
                    var categoryParent = treeParent.transform.Find(category.ToString());
                    if (!categoryParent)
                        throw new SystemException("Failed to find category parent.");
                    categoryParents[tree][category] = categoryParent;
                }
            }
        }
        
        private void FindSkillSubComponents()
        {
            var skillNameObject = GameObject.Find("SkillName");
            if (!skillNameObject)
                throw new SystemException("Failed to find SkillName GameObject.");
            if (!skillNameObject.TryGetComponent(out skillNameText))
                throw new SystemException("Failed to find SkillName TextMeshProUGUI component.");

            var skillTierTextObject = GameObject.Find("SkillTierText");
            if (!skillTierTextObject)
                throw new SystemException("Failed to find SkillTierText GameObject.");
            if (!skillTierTextObject.TryGetComponent(out skillTierText))
                throw new SystemException("Failed to find SkillTierText TextMeshProUGUI component.");
            
            var skillEffectTextObject = GameObject.Find("EffectText");
            if (!skillEffectTextObject)
                throw new SystemException("Failed to find SkillEffectText GameObject.");
            if (!skillEffectTextObject.TryGetComponent(out skillEffectText))
                throw new SystemException("Failed to find SkillEffectText TextMeshProUGUI component.");
            
            var skillEffectDifferenceTextObject = GameObject.Find("DifferenceEffectText");
            if (!skillEffectDifferenceTextObject)
                throw new SystemException("Failed to find SkillEffectDifferenceText GameObject.");
            if (!skillEffectDifferenceTextObject.TryGetComponent(out skillEffectDifferenceText))
                throw new SystemException("Failed to find SkillEffectDifferenceText TextMeshProUGUI component.");
            
            var skillEffectDifferenceResultTextObject = GameObject.Find("DifferenceResultText");
            if (!skillEffectDifferenceResultTextObject)
                throw new SystemException("Failed to find SkillEffectDifferenceResultText GameObject.");
            if (!skillEffectDifferenceResultTextObject.TryGetComponent(out skillEffectDifferenceResultText))
                throw new SystemException("Failed to find SkillEffectDifferenceResultText TextMeshProUGUI component.");
            
            var skillFlairTextObject = GameObject.Find("FlairText");
            if (!skillFlairTextObject)
                throw new SystemException("Failed to find SkillFlairText GameObject.");
            if (!skillFlairTextObject.TryGetComponent(out skillFlairText))
                throw new SystemException("Failed to find SkillFlairText TextMeshProUGUI component.");
            
            var skillCostTextObject = GameObject.Find("SkillCostText");
            if (!skillCostTextObject)
                throw new SystemException("Failed to find SkillCostText GameObject.");
            if (!skillCostTextObject.TryGetComponent(out skillCostText))
                throw new SystemException("Failed to find SkillCostText TextMeshProUGUI component.");
            
            var availableSkillPointsTextObject = GameObject.Find("SkillPointsAmount");
            if (!availableSkillPointsTextObject)
                throw new SystemException("Failed to find AvailableSkillPointsText GameObject.");
            if (!availableSkillPointsTextObject.TryGetComponent(out availableSkillPointsText))
                throw new SystemException("Failed to find AvailableSkillPointsText TextMeshProUGUI component.");
            
            var unlockButtonObject = GameObject.Find("ButtonUnlock");
            if (!unlockButtonObject)
                throw new SystemException("Failed to find UnlockButton GameObject.");
            if (!unlockButtonObject.TryGetComponent(out unlockButton))
                throw new SystemException("Failed to find UnlockButton Button component.");
        }

        private static void GetSkillDataBasedOnLocation(int _tree, int _category, int _tier, out string _name,
            out SkillTypes _skillType, out string _flair, out float _multiplier, out string _effectText,
            out string _resultText, out int _cost)
        {
            _cost = Mathf.CeilToInt((_tier) * 1.5f);
            switch (_tree) {
                case 0:
                    switch (_category) {
                        case 0:
                            _name = "Ability Damage";
                            _skillType = SkillTypes.AbilityDamage;
                            _flair = "Rain down the damage of Zeus upon your foes";
                            _multiplier = AbilityDamageMultiplier;
                            _effectText = $"Damage * {_multiplier}";
                            _resultText = (PlayerDataStorage.AbilityDamage * _multiplier).ToString("0.00");
                            return;
                        case 1:
                            _name = "Ability Cooldown";
                            _skillType = SkillTypes.AbilityCooldown;
                            _flair = "Shoot with the speed of Hermes";
                            _multiplier = AbilityCooldownMultiplier;
                            _effectText = $"Cooldown * {_multiplier}";
                            _resultText = (PlayerDataStorage.AbilityCooldown * _multiplier).ToString("0.00");
                            return;
                        case 2:
                            _name = "Ability Range";
                            _skillType = SkillTypes.AbilityRange;
                            _flair = "Shoot to the top of Mount Olympus with the power of Artemis";
                            _multiplier = AbilityRangeMultiplier;
                            _effectText = $"Range * {_multiplier}";
                            _resultText = (PlayerDataStorage.AbilityRange * _multiplier).ToString("0.00");
                            return;
                        default:
                            throw new SystemException("Invalid ability category: " + _category);
                    }
                case 1:
                    switch (_category) {
                        case 0:
                            _name = "Super Damage";
                            _skillType = SkillTypes.SuperDamage;
                            _flair = "Burn your enemies with the power of Apollo";
                            _multiplier = SuperDamageMultiplier;
                            _effectText = $"Damage * {_multiplier}";
                            _resultText = (PlayerDataStorage.SuperDamage * _multiplier).ToString("0.00");
                            return;
                        case 1:
                            _name = "Super Cooldown";
                            _skillType = SkillTypes.SuperCooldown;
                            _flair = "Control the flow of time with the power of Kronos";
                            _multiplier = SuperCooldownMultiplier;
                            _effectText = $"Cooldown * {_multiplier}";
                            _resultText = (PlayerDataStorage.BaseSuperCooldown * _multiplier).ToString("0.00");
                            return;
                        default:
                            throw new SystemException("Invalid Super category: " + _category);
                    }
                case 2:
                    switch (_category) {
                        case 0:
                            _name = "Magnet Range";
                            _skillType = SkillTypes.MagnetRange;
                            _flair = "Collect all the treasures in the world with the power of Bacchus";
                            _multiplier = MagnetRangeMultiplier;
                            _effectText = $"Range * {_multiplier}";
                            _resultText = (PlayerDataStorage.MagnetRange * _multiplier).ToString("0.00");
                            return;
                        case 1:
                            _name = "Magnet Cooldown";
                            _skillType = SkillTypes.MagnetCooldown;
                            _flair = "Show your greed and leave no stone unturned with the power of Athena";
                            _multiplier = MagnetCooldownMultiplier;
                            _effectText = $"Cooldown * {_multiplier}";
                            _resultText = (PlayerDataStorage.MagnetCooldown * _multiplier).ToString("0.00");
                            return;
                        default:
                            throw new SystemException("Invalid magnet category: " + _category);
                    }
                default:
                    throw new SystemException("Invalid tier.");
            }
        }
        
        private void InitializeSkillSubComponents()
        {
            CalculateOverallMaxTier();
            var amountOfSkillsToPopulate = (overallMaxTier / 5 + 1) * 5;
            
            // Loop through all skill trees
            for (var tree = 0; tree < maxTiersByCategory.Length; tree++) {
                // Loop through all categories in each tree
                for (int category = 0; category < maxTiersByCategory[tree].Length; category++) {
                    var maxUnlockTier = maxTiersByCategory[tree][category] + 1; // Add 1 for next unlockable tier
                    var spentPoints = pointsSpentByTree[tree];
                    
                    for (var tier =skillItemViews[tree][category].Count; tier < amountOfSkillsToPopulate; tier++) {
                        var skill = Instantiate(skillItemPrefab, categoryParents[tree][category]);
                        if (!skill)
                            throw new SystemException("Failed to instantiate SkillItemPrefab.");
                        if (!skill.TryGetComponent(out SkillItemView skillItem)) {
                            Destroy(skill);
                            throw new SystemException("Failed to find SkillItemView component.");
                        }
                        // Check if we have reached the max tier for this category
                        var isLocked = (false, true);
                        if (tier > maxUnlockTier) {
                            isLocked = (true, false);
                        }
                        
                        GetSkillDataBasedOnLocation(tree, category, tier, out var skillName, out var skillEffect,
                            out var flair, out var multiplier, out var effectText, out var resultText, out var cost);
                        
                        var requirement = new int2(spentPoints, cost);
                        var romanTier = ConvertIntToRoman(tier + 1);

                        var skillData = new SkillData(skillName, tier + 1, romanTier, requirement, skillEffect, flair,
                            effectText,
                            resultText, multiplier, cost, false, isLocked.Item1, isLocked.Item2,
                            new int2(tree, category));
                        
                        skillItem.Bind(skillData, DisplaySkillInfo);
                        skillItemViews[tree][category].Add(skill);
                        skillItemComponents[tree][category].Add(skillItem);
                        
                        availableSkillPointsText.text = RunScoreManager.Instance.GemsCount.ToString();
                    }
                }
            }
        }

        private void PurchaseSkill(SkillData _skillData)
        {
            RunScoreManager.Instance.GemsCount -= _skillData.Cost;
            //GameManager.Instance.AddSkill(_skillData.Effect);
            availableSkillPointsText.text = RunScoreManager.Instance.GemsCount.ToString();
            var spentPoints = pointsSpentByTree[_skillData.TreePosition.x];
            var maxTier = maxTiersByCategory[_skillData.TreePosition.x][_skillData.TreePosition.y];
            
            var purchasedTier = _skillData.TierNumerical;
            maxTiersByCategory[_skillData.TreePosition.x][_skillData.TreePosition.y] = Mathf.Max(maxTier, purchasedTier);
            if (purchasedTier > overallMaxTier)
                CalculateOverallMaxTier();
            var maxTierChanged = overallMaxTier <= purchasedTier;
            pointsSpentByTree[_skillData.TreePosition.x] = spentPoints + _skillData.Cost;

            switch (_skillData.Effect) {
                case SkillTypes.AbilityDamage:
                    PlayerDataStorage.AbilityDamage *= _skillData.EffectPower;
                    break;
                case SkillTypes.AbilityCooldown:
                    PlayerDataStorage.AbilityCooldown /= _skillData.EffectPower;
                    break;
                case SkillTypes.AbilityRange:
                    PlayerDataStorage.AbilityRange *= _skillData.EffectPower;
                    break;
                case SkillTypes.SuperDamage:
                    PlayerDataStorage.SuperDamage *= _skillData.EffectPower;
                    break;
                case SkillTypes.SuperCooldown:
                    PlayerDataStorage.BaseSuperCooldown /= _skillData.EffectPower;
                    break;
                case SkillTypes.MagnetRange:
                    PlayerDataStorage.MagnetRange *= _skillData.EffectPower;
                    break;
                case SkillTypes.MagnetCooldown:
                    PlayerDataStorage.MagnetCooldown /= _skillData.EffectPower;
                    break;
                case SkillTypes.None:
                    break;
                default:
                    Debug.LogError("Unknown skill effect type");
                    break;
            }
            
            var skillItem = skillItemComponents[_skillData.TreePosition.x][_skillData.TreePosition.y][purchasedTier - 1];
            skillItem.Purchase();
            
            UnlockNextSkill(_skillData.TreePosition, purchasedTier);
            if (maxTierChanged) {
                InitializeSkillSubComponents();
            }
            UpdateSkillPoints(_skillData.TreePosition.x);
        }

        private void UnlockNextSkill(int2 _treePosition, int _skillTier)
        {
            if (_skillTier + 1 >= skillItemViews[_treePosition.x][_treePosition.y].Count) {
                return;
            }
            
            var unlockedSkill = skillItemViews[_treePosition.x][_treePosition.y][_skillTier + 1];
            if (!unlockedSkill || !unlockedSkill.TryGetComponent(out SkillItemView skillItem))
                throw new SystemException("Failed to find SkillItemView component.");
            
            skillItem.Unlock();
        }

        private void UpdateSkillPoints(int _tree)
        {
            for (var category = 0; category < maxTiersByCategory[_tree].Length; category++) {
                var startingIndex = maxTiersByCategory[_tree][category];
                for (var tier = startingIndex; tier < startingIndex + 2; tier++) {
                    if (tier >= skillItemViews[_tree][category].Count)
                        continue;
                    
                    var skillItem = skillItemComponents[_tree][category][tier];
                    
                    skillItem.UpdatePoints(pointsSpentByTree[_tree]);
                }
            }
        }
        
    }
}
