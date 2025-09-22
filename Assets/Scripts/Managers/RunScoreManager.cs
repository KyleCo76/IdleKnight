using System.Collections;
using Game;
using TMPro;
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

        [SerializeField, Tooltip("The current score of the player")]
        private int runScore;
        public int CurrentScore => runScore;
        private int playerLevel = 1;
        private int lastScoreLevelThreshold = 50;

        // Cached Components
        private TextMeshProUGUI scoreText;
        private PlayerLevels playerLevels;


        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad is handled by parent GameManager

            scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
            if (scoreText == null) {
                Debug.LogError("ScoreText UI element not found in the scene.");
                enabled = false;
                return;
            }
            scoreText.text = Mathf.FloorToInt(runScore).ToString();

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
            if (runScore >= lastScoreLevelThreshold + (lastScoreLevelThreshold * playerLevels.GetLevelMultiplier(playerLevel)))
            {
                lastScoreLevelThreshold = runScore;
                playerLevel++;
                OnPlayerLeveledUp?.Invoke(playerLevel);
                Debug.Log($"Player leveled up to {playerLevel}!");
            }
        }

        public void AddScore(int _points)
        {
            runScore += _points;
            runScore = Mathf.Max(0, runScore);
            scoreText.text = Mathf.FloorToInt(runScore).ToString();
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
