using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class ShopSpawner : MonoBehaviour
    {
        [FoldoutGroup("Spawning"), SerializeField, Tooltip("Time interval between shop spawns")]
        private float spawnInterval = 500f;
        [FoldoutGroup("Spawning"), SerializeField, Tooltip("Length of time the shop will remain on the map after being spawned")]
        private float timeToLive = 300f;
        [FoldoutGroup("Spawning"), SerializeField, Tooltip("Layer to check for overlaps")]
        private LayerMask shopLayer;

        [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Minimum distance from player to spawn shops")]
        private float spawnRangeMin = 15f;
        [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Maximum distance from player to spawn shops")]
        private float spawnRangeMax = 25f;
        [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Minimum distance from other shops to spawn shops")]
        private float shopSpawnOffset = 10f;
        [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Maximum distance the shop can be from the player before being culled")]
        private float cullDistance = 20f;

        
        public static ShopSpawner Instance;
        
        private const int MaxPlacementAttempts = 10;
        private GameObject[] shopPrefabs;
        private readonly Dictionary<string, TrackedShop> activeShops = new();
        private Transform playerTransform;
        private bool shouldSpawn;
        
        private readonly List<ShopLocationData> shopLocations = new();
        private readonly Queue<string> shopsToCull = new();
        private bool jobRunning;


        void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            
            shopPrefabs = Resources.LoadAll<GameObject>("Shops");
        }

        private void OnEnable()
        {
            GameSceneManager.Instance.OnSceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            GameSceneManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
        }

        void Update()
        {
            if (!shouldSpawn || !GameManager.Instance || GameManager.Instance.IsPaused)
                return;

            while (shopsToCull.Count > 0) {
                var shop = shopsToCull.Dequeue();
                if (String.IsNullOrEmpty(shop))
                    continue;
                Destroy(activeShops[shop].Shop);
                activeShops.Remove(shop);
            }

            if (!jobRunning) {
                TryShopCull();
            }

            var snapshot = new Dictionary<string, TrackedShop>(activeShops);
            foreach (var shop in snapshot) {
                var trackedShop = shop.Value;
                trackedShop.TimeToLive -= Time.deltaTime;
                if (trackedShop.TimeToLive > 0 && activeShops.ContainsKey(shop.Key)) {
                    activeShops[shop.Key] = trackedShop;
                    continue;
                }
                Destroy(shop.Value.Shop);
                activeShops.Remove(shop.Key);
            }
        }


        private void HandleSceneLoaded(int _sceneIndex)
        {
            StopAllCoroutines();
            activeShops.Clear();
            if (_sceneIndex is SceneNames.MainMenu or SceneNames.PlayerHome) {
                shouldSpawn = false;
                return;
            }
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                playerTransform = playerObj.transform;
                shouldSpawn = true;
            } else {
                shouldSpawn = false;
                Debug.LogError("No GameObject tagged 'Player' found. Please assign the player tag.");
            }

            StartCoroutine(ShopSpawnerCoroutine(spawnInterval));
        }
        
        private bool IsShopTooClose(Vector2 _position)
        {
            // Overlap any shop colliders within buffer distance
            return Physics2D.OverlapCircle(_position, shopSpawnOffset, shopLayer);
        }

        public void LeaveShop(GameObject _shop)
        {
            foreach (var shop in activeShops) {
                if (shop.Value.Shop == _shop) {
                    activeShops.Remove(shop.Key);
                    Destroy(_shop);
                    break;
                }
            }
        }
        
        private Vector2 RandomPointInAnnulus(Vector2 _center, float _innerRadius, float _outerRadius)
        {
            if (_outerRadius < _innerRadius) (_innerRadius, _outerRadius) = (_outerRadius, _innerRadius);

            var seed = new Unity.Mathematics.Random(GameManager.Instance.GetEntropy());
            var rng = new Unity.Mathematics.Random(seed.NextUInt());
            float angle = rng.NextFloat(0f, Mathf.PI * 2f);
            float u = Random.value;
            float r = Mathf.Sqrt(Mathf.Lerp(_innerRadius * _innerRadius, _outerRadius * _outerRadius, u));
            return _center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
        }

        private void TryShopCull()
        {
            shopLocations.Clear();
            foreach (var shop in activeShops) {
                var locationData = new ShopLocationData
                {
                    PlayerPosition = playerTransform.position, ShopPosition = shop.Value.Shop.transform.position, Guid = shop.Key
                };
                shopLocations.Add(locationData);
            }
            jobRunning = true;
            float cullDistSqr = cullDistance * cullDistance;
                
            Task.Run(() =>
                {
                    var cullList = new List<string>();
                    foreach (var shopData in shopLocations) {
                        if ((shopData.ShopPosition - shopData.PlayerPosition).sqrMagnitude > cullDistSqr) {
                            cullList.Add(shopData.Guid);
                        }
                    }

                    return cullList;
                })
                .ContinueWith(_task =>
                {
                    if (_task.Status == TaskStatus.RanToCompletion) {
                        foreach (var guid in _task.Result)
                            shopsToCull.Enqueue(guid);
                    }
                    jobRunning = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool TryGetValidShopPosition(out Vector3 _position)
        {
            _position = Vector3.zero;
            if (!playerTransform)
                return false;

            var center = (Vector2)playerTransform.position;

            for (int i = 0; i < MaxPlacementAttempts; i++)
            {
                Vector2 candidate = RandomPointInAnnulus(center, spawnRangeMin, spawnRangeMax);

                // If there is a shop within shopSpawnOffset, reject and retry
                if (IsShopTooClose(candidate)) {
                    continue;
                }

                _position = new Vector3(candidate.x, candidate.y, 0f);
                return true;
            }
            return false;
        }

        private void SpawnShop()
        {
            if (!shopPrefabs[0] || !playerTransform) return;

            if (!TryGetValidShopPosition(out var spawnPos)) {
                Debug.LogWarning("No Valid Shop Position");
                return;
            }

            var shop = Instantiate(shopPrefabs[0], spawnPos, Quaternion.identity);
            var id = Guid.NewGuid().ToString();
            activeShops.Add(id, new TrackedShop{ Shop = shop, SpawnPosition = spawnPos, TimeToLive = timeToLive });
        }

        private IEnumerator ShopSpawnerCoroutine(float _interval)
        {
            var wait = new WaitForSeconds(_interval);
            while (enabled) {
                if (GameManager.Instance && !GameManager.Instance.IsPaused && shouldSpawn) {
                    SpawnShop();
                }
                yield return wait;
            }
        }
    }

    public struct TrackedShop : IEquatable<TrackedShop>
    {
        public GameObject Shop;
        public Vector2 SpawnPosition;
        public float TimeToLive;
        
        public bool Equals(TrackedShop _other)
        {
            return ReferenceEquals(Shop, _other.Shop);
        }
        
        public override bool Equals(object _obj)
        {
            return _obj is TrackedShop other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Shop.GetHashCode();
        }
    }

    public struct ShopLocationData
    {
        public Vector2 PlayerPosition;
        public Vector2 ShopPosition;
        public string Guid;
    }
}