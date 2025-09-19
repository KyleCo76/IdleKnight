using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ShopSpawner : MonoBehaviour
{
    [FoldoutGroup("Spawning"), SerializeField, Tooltip("Time interval between shop spawns")]
    private float spawnInterval = 500f;
    [FoldoutGroup("Spawning"), SerializeField, Tooltip("Maximum number of shops allowed in the scene at once")]
    private int maxShops = 1;
    [FoldoutGroup("Spawning"), SerializeField, Tooltip("Length of time the shop will remain on the map after being spawned")]
    private float timeToLive = 300f;

    [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Minimum distance from player to spawn shops")]
    private float spawnRangeMin = 15f;
    [FoldoutGroup("Spawn Distance"), SerializeField, Tooltip("Maximum distance from player to spawn shops")]
    private float spawnRangeMax = 25f;

    private GameObject[] shopPrefabs;
    private float spawnTimer = 0f;
    private readonly List<TrackedShop> activeShops = new();
    private Transform playerTransform;


    void Start()
    {
        shopPrefabs = Resources.LoadAll<GameObject>("Shops");
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null || !playerObject.TryGetComponent<Transform>(out playerTransform)) {
            Debug.LogError("Player GameObject not found or missing Transform component.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsPaused)
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

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnShop();
            spawnTimer = 0f;
        }

    }

    private void SpawnShop()
    {
        if (shopPrefabs.Length == 0 || activeShops.Count >= maxShops)
            return;

        int randomIndex = Random.Range(0, shopPrefabs.Length);
        GameObject shop = Instantiate(shopPrefabs[randomIndex]);
        shop.transform.position = (Vector2)playerTransform.position + GetRandomSpawnPosition();
        activeShops.Add(new TrackedShop { Shop = shop, TimeToLive = timeToLive });
    }

    private Vector2 GetRandomSpawnPosition()
    {
        // Spawn at a random position outside the player's area
        Vector2 spawningRange = new(Random.Range(spawnRangeMin, spawnRangeMax), Random.Range(spawnRangeMin, spawnRangeMax));
        if (Random.value < 0.5f) {
            spawningRange.x *= -1;
        }
        if (Random.value < 0.5f) {
            spawningRange.y *= -1;
        }
        return spawningRange;
    }
}

public struct TrackedShop
{
    public GameObject Shop;
    public float TimeToLive;
}
