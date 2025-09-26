using System.Collections.Generic;
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

        
        public static ShopSpawner Instance;
        
        private const int MaxPlacementAttempts = 10;
        private GameObject[] shopPrefabs;
        private float spawnTimer;
        private readonly List<TrackedShop> activeShops = new();
        private Transform playerTransform;
        private bool shouldSpawn;


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
            if (!GameManager.Instance || GameManager.Instance.IsPaused)
                return;

            // Update TTL for active shops
            for (int i = activeShops.Count - 1; i >= 0; i--)
            {
                TrackedShop trackedShop = activeShops[i];
                trackedShop.TimeToLive -= Time.deltaTime;
                if (trackedShop.TimeToLive <= 0f)
                {
                    Destroy(trackedShop.Shop);
                    activeShops.RemoveAt(i);
                }
                else
                {
                    activeShops[i] = trackedShop;
                }
            }

            if (!shouldSpawn)
                return;
            
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnShop();
                spawnTimer = 0f;
            }

        }


        private void HandleSceneLoaded(int _sceneIndex)
        {
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
        }

        public void LeaveShop(GameObject _shop)
        {
            foreach (var shop in activeShops) {
                if (shop.Shop == _shop) {
                    activeShops.Remove(shop);
                    Destroy(_shop);
                    break;
                }
            }
        }
        
        private Vector2 RandomPointInAnnulus(Vector2 _center, float _innerRadius, float _outerRadius)
        {
            if (_outerRadius < _innerRadius) (_innerRadius, _outerRadius) = (_outerRadius, _innerRadius);
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float u = Random.value;
            float r = Mathf.Sqrt(Mathf.Lerp(_innerRadius * _innerRadius, _outerRadius * _outerRadius, u));
            return _center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
        }

        private bool IsShopTooClose(Vector2 _position)
        {
            // Overlap any shop colliders within buffer distance
            return Physics2D.OverlapCircle(_position, shopSpawnOffset, shopLayer) != null;
        }

        private bool TryGetValidShopPosition(out Vector3 _position)
        {
            _position = Vector3.zero;
            if (!playerTransform) return false;

            var center = (Vector2)playerTransform.position;

            for (int i = 0; i < MaxPlacementAttempts; i++)
            {
                Vector2 candidate = RandomPointInAnnulus(center, spawnRangeMin, spawnRangeMax);

                // If there is a shop within shopSpawnOffset, reject and retry
                if (IsShopTooClose(candidate)) continue;

                _position = new Vector3(candidate.x, candidate.y, 0f);
                return true;
            }
            return false;
        }

        private void SpawnShop()
        {
            if (shopPrefabs[0] == null || playerTransform == null) return;

            if (!TryGetValidShopPosition(out var spawnPos))
            {
                // Couldn't find a valid spot this frame; skip spawn to avoid overlaps
                return;
            }

            var shop = Instantiate(shopPrefabs[0], spawnPos, Quaternion.identity);
            activeShops.Add(new TrackedShop{ Shop = shop, SpawnPosition = spawnPos, TimeToLive = timeToLive});
        }
    }

    public struct TrackedShop : System.IEquatable<TrackedShop>
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
}