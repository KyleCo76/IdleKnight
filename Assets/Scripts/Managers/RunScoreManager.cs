using System.Collections;
using Game;
using ScriptableObjects;
using UnityEngine;

namespace Managers
{
    public class RunScoreManager : MonoBehaviour
    {
        public static RunScoreManager Instance { get; private set; }

        [SerializeField, Tooltip("The multiplier applied to the score when the player defeats an enemy")]
        private float playerAttackMultiplier = 2f;

        public delegate void PlayerLeveledUpEventHandler(int _newLevel);
        public event PlayerLeveledUpEventHandler OnPlayerLeveledUp;

        [Tooltip("The current score of the player")]
        public int RunScore { get; private set; }
        public int PowerUpScore { get; private set; }
        public int GemsCount { get; private set; }

        private int playerLevel = 1;
        private int lastScoreLevelThreshold = 50;

        // Cached Components
        private PlayerLevels playerLevels;


        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad is handled by parent GameManager

            playerLevels = Resources.Load<PlayerLevels>("ScriptableObjects/PlayerLevels");
            if (playerLevels == null) {
                Debug.LogError("PlayerLevels not found in Resources/ScriptableObjects.");
                enabled = false;
            }
        }
        private void OnEnable()
        {
            Enemies.Controller.OnEnemyDeath += HandleEnemyDeath;
        }

        private void OnDisable()
        {
            Enemies.Controller.OnEnemyDeath -= HandleEnemyDeath;
        }

        private void Update()
        {
            if (RunScore >= lastScoreLevelThreshold + (lastScoreLevelThreshold * playerLevels.GetLevelMultiplier(playerLevel)))
            {
                lastScoreLevelThreshold = RunScore;
                playerLevel++;
                OnPlayerLeveledUp?.Invoke(playerLevel);
                Debug.Log($"Player leveled up to {playerLevel}!");
            }
        }

        public void AddScore(int _points)
        {
            RunScore += _points;
            RunScore = Mathf.Max(0, RunScore);
            UIManager.Instance.UpdateScoreText();
        }

        public void AddPowerUpScore(int _points)
        {
            PowerUpScore += _points;
            PowerUpScore = Mathf.Max(0, PowerUpScore);
            UIManager.Instance.UpdatePowerUpScoreText();
        }

        public int GetPlayerLevel()
        {
            return playerLevel;
        }

        private void HandleEnemyDeath(AttackType _attackType, int _points, float _itemChance, Vector2 _position, GameObject _enemy)
        {
            int pointValue = Mathf.FloorToInt(_points * (_attackType == AttackType.PlayerAttack ? playerAttackMultiplier : 1f));
            AddScore(pointValue);
        }

        public void ModifyPointMultiplier(float _duration)
        {
            float originalPoints = playerAttackMultiplier;
            playerAttackMultiplier *= 2f;
            StartCoroutine(ResetPointMultiplier(originalPoints, _duration));
        }

        private IEnumerator ResetPointMultiplier(float _originalPoints, float _duration)
        {
            yield return new WaitForSeconds(_duration);
            playerAttackMultiplier = _originalPoints;
        }
    }
}
