using System.Collections;
using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using Game;

namespace Enemies
{
    public class MinionSpawner : MonoBehaviour
    {
        [FoldoutGroup("Settings"), SerializeField, Tooltip("Time interval between minion spawns")]
        private float spawnInterval = 2f;
        [FoldoutGroup("Settings"), SerializeField, Tooltip("Maximum number of minions allowed in the scene at once. 0 will spawn infinitely")]
        private int maxMinions = 5;
        [FoldoutGroup("Settings"), SerializeField, Tooltip("Length of time the minion will remain on the map after being spawned. 0 will last indefinitely")]
        private float timeToLive;
        [FoldoutGroup("Settings"), SerializeField, Tooltip("The layer to assign to spawned minions")]
        private LayerMask minionLayer;
        [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Minimum distance from spawner to spawn minions")]
        private float spawnRangeMin = 0.2f;
        [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Maximum distance from spawner to spawn minions")]
        private float spawnRangeMax = 1.4f;
        [FoldoutGroup("Animation Settings"), SerializeField, Tooltip("Does the spawner have an animation that plays when spawning a minion?")]
        private bool hasSpawnAnimation;
        
        [FoldoutGroup("Pooling"), SerializeField, Tooltip("Initial pool size per prefab")]
        private int initialPoolSize = 10;
        [FoldoutGroup("Pooling"), SerializeField, Tooltip("Maximum pool size per prefab (0 = unlimited)")]
        private int maxPoolSizePerPrefab;

        private readonly List<TrackedMinion> activeMinions = new();

        // Cached Components
        private Animator animator;
        private GameObject[] minionPrefabs;
        private int spawnAnimationHash;
        
        // Minion Pool
        private PooledMinionManager minionPool;
        private int minionLayerIndex;


        private void Awake()
        {
            if (!TryGetComponent(out animator) && hasSpawnAnimation) {
                Debug.LogWarning("MinionSpawner set to use spawn animation but no Animator component found.");
                hasSpawnAnimation = false;
            }

            minionPrefabs = Resources.LoadAll<GameObject>("Enemies/SpawnableMinions/Skeleton");
            if (minionPrefabs.Length == 0) {
                Debug.LogWarning("No minion prefabs found in Resources/Enemies/SpawnableMinions/Skeleton.");
            }
            if (hasSpawnAnimation)
                spawnAnimationHash = Animator.StringToHash("Spawn");
            
            minionLayerIndex = minionLayer > 0 ? Mathf.RoundToInt(Mathf.Log(minionLayer, 2)) : 0;
            
            // Initialize minion pool
            minionPool = new PooledMinionManager(initialPoolSize, maxPoolSizePerPrefab);
            foreach (var prefab in minionPrefabs) {
                minionPool.PreWarm(prefab);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(SpawnLoop());
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.IsPaused)
                return;

            if (Mathf.Approximately(timeToLive, 0f))
                return;
            
            // Update TTL for active minions
            for (int i = activeMinions.Count - 1; i >= 0; i--) {
                TrackedMinion trackedMinion = activeMinions[i];
                trackedMinion.TimeToLive -= Time.deltaTime;
                if (trackedMinion.TimeToLive <= 0f) {
                    minionPool.Release(trackedMinion.Minion);
                    activeMinions.RemoveAt(i);
                } else {
                    activeMinions[i] = trackedMinion;
                }
            }
        }


        internal void ReleaseMinion(GameObject _minion)
        {
            minionPool.Release(_minion);
            foreach (var tracked in activeMinions) {
                if (tracked.Minion == _minion) {
                    activeMinions.Remove(tracked);
                    break;
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
            GameObject newMinion = minionPool.GetFromPool(minionPrefab, spawnPosition, Quaternion.identity);

            if (!newMinion) {
                Debug.LogWarning("Failed to spawn minion.");
                return;
            }
            
            newMinion.layer = minionLayerIndex;

            if (!newMinion.TryGetComponent(out IPooledResettable minionMarker)) {
                Debug.LogError("Minion prefab does not have a IPooledResettable component.");
                return;
            }
            
            minionMarker.OnTakenFromPool(minionPrefab, this);
            
            activeMinions.Add(new TrackedMinion { Minion = newMinion, TimeToLive = timeToLive });
            
            if (hasSpawnAnimation && animator) {
                animator.SetTrigger(spawnAnimationHash);
            }
        }

        private IEnumerator SpawnLoop()
        {
            var wait = new WaitForSeconds(spawnInterval);
            while (enabled) {
                if (GameManager.Instance || !GameManager.Instance.IsPaused) {
                    if (maxMinions == 0 || activeMinions.Count < maxMinions) {
                        SpawnMinion();
                    }
                }
                yield return wait;
            }
        }
    }
}