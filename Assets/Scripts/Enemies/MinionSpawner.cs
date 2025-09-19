using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    [FoldoutGroup("Settings"), SerializeField, Tooltip("Time interval between minion spawns")]
    private float spawnInterval = 2f;
    [FoldoutGroup("Settings"), SerializeField, Tooltip("Maximum number of minions allowed in the scene at once. 0 will spawn infinitely")]
    private int maxMinions = 5;
    [FoldoutGroup("Settings"), SerializeField, Tooltip("Length of time the minion will remain on the map after being spawned. 0 will last indefinitely")]
    private float timeToLive = 0f;
    [FoldoutGroup("Settings"), SerializeField, Tooltip("The layer to assign to spawned minions")]
    private LayerMask minionLayer;
    [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Minimum distance from spawner to spawn minions")]
    private float spawnRangeMin = 0.2f;
    [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Maximum distance from spawner to spawn minions")]
    private float spawnRangeMax = 1.4f;
    [FoldoutGroup("Animation Settings"), SerializeField, Tooltip("Does the spawner have an animation that plays when spawning a minion?")]
    private bool hasSpawnAnimation = false;

    private float spawnTimer = 0f;
    private readonly List<TrackedMinion> activeMinions = new();

    // Cached Components
    private Animator animator;
    private GameObject[] minionPrefabs;


    void Awake()
    {
        if (!TryGetComponent<Animator>(out animator) && hasSpawnAnimation) {
            Debug.LogWarning("MinionSpawner set to use spawn animation but no Animator component found.");
            hasSpawnAnimation = false;
        }

        minionPrefabs = Resources.LoadAll<GameObject>("Enemies/SpawnableMinions/Skeleton");
        if (minionPrefabs.Length == 0) {
            Debug.LogWarning("No minion prefabs found in Resources/Enemies/SpawnableMinions/Skeleton.");
        }
    }


    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsPaused)
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && activeMinions.Count < maxMinions) {
            SpawnMinion();
            spawnTimer = 0f;
        }

        // Update TTL for active minions
        for (int i = activeMinions.Count - 1; i >= 0; i--) {
            TrackedMinion trackedMinion = activeMinions[i];
            trackedMinion.TimeToLive -= Time.deltaTime;
            if (trackedMinion.TimeToLive <= 0f) {
                Destroy(trackedMinion.Minion);
                activeMinions.RemoveAt(i);
            } else {
                activeMinions[i] = trackedMinion;
            }
        }
    }


    private void SpawnMinion()
    {
        if (minionPrefabs.Length == 0) {
            Debug.LogWarning("No minion prefabs assigned to MinionSpawner.");
            return;
        }
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        float spawnDistance = Random.Range(spawnRangeMin, spawnRangeMax);
        Vector3 spawnPosition = transform.position + (Vector3)(spawnDirection * spawnDistance);
        GameObject minionPrefab = minionPrefabs[Random.Range(0, minionPrefabs.Length - 1)];
        GameObject newMinion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        newMinion.layer = Mathf.RoundToInt(Mathf.Log(minionLayer.value, 2)); // Set the layer based on the LayerMask
        if (!Mathf.Approximately(timeToLive, 0f)) {
            activeMinions.Add(new TrackedMinion { Minion = newMinion, TimeToLive = timeToLive });
        }
        if (hasSpawnAnimation) {
            if (animator != null) {
                animator.SetTrigger("Spawn");
            }
        }
    }
}

public struct TrackedMinion
{
    public GameObject Minion;
    public float TimeToLive;
}
