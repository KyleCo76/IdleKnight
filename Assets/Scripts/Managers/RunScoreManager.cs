using Game;
using TMPro;
using UnityEngine;
using System.Collections;

public class RunScoreManager : MonoBehaviour
{
    public static RunScoreManager Instance { get; private set; }

    [SerializeField, Tooltip("The multiplier applied to the score when the player defeats an enemy")]
    private float playerAttackMultiplier = 2f; 

    private float runScore = 0f;

    // Cached Components
    public TextMeshProUGUI scoreText;


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
        scoreText.text = $"Score: {Mathf.FloorToInt(runScore)}";
    }
    private void OnEnable()
    {
        Enemies.Controller.OnEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        Enemies.Controller.OnEnemyDeath -= HandleEnemyDeath;
    }

    private void AddScore(float _points)
    {
        runScore += _points;
        scoreText.text = $"Score: {Mathf.FloorToInt(runScore)}";
    }

    private void HandleEnemyDeath(AttackType _attackType, float _points)
    {
        float pointValue = _points * (_attackType == AttackType.PlayerAttack ? playerAttackMultiplier : 1f);
        AddScore(pointValue);
    }

    public void ModifyPointMultiplier(float _multiplier, float _duration)
    {
        float originalPoints = playerAttackMultiplier;
        playerAttackMultiplier *= _multiplier;
        StartCoroutine(ResetPointMultiplier(originalPoints, _duration));
    }

    private IEnumerator ResetPointMultiplier(float _originalPoints, float _duration)
    {
        yield return new WaitForSeconds(_duration);
        playerAttackMultiplier = _originalPoints;
    }
}
