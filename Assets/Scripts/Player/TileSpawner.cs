
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("The Tilemap to paint tiles on")]
    private Tilemap groundTilemap;
    [SerializeField, Tooltip("The RuleTile to use for painting")]
    private RuleTile groundRuleTile;
    [SerializeField, Tooltip("Radius around the player to paint (in world units)")]
    private float paintRadius = 50.0f;
    [SerializeField, Tooltip("How often to check and paint (in seconds)")]
    private float checkInterval = 1.0f;
    [SerializeField, Tooltip("Limit how many tiles are set per frame for performance")]
    private int maxTilesPerFrame = 100;
    [SerializeField, Tooltip("Tiles to use for obstacles")]
    private RuleTile obstacleTile;
    [SerializeField, Tooltip("The Tilemap to paint obstacles on")]
    private Tilemap obstacleTilemap;
    [SerializeField, Tooltip("Chance to place an obstacle tile (0 to 1)"), Range(0f, 1f)]
    private float obstacleChance = 0.05f;
    [SerializeField, Tooltip("Minimum distance from player to place obstacles (in world units)")]
    private float obstacleMinDistance = 10.0f;
    [SerializeField, Tooltip("Obstacle grouping chance (0 to 1)"), Range(0f, 1f)]
    private float obstacleGroupChance = 0.3f;

    private Transform playerTransform;
    private float nextCheckTime = 0f;
    private HashSet<Vector3Int> paintedTiles = new();
    private Vector3Int lastPlayerCellPos;
    private readonly Queue<System.Action> tileOpsQueue = new();

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            playerTransform = playerObj.transform;
        } else {
            Debug.LogError("No GameObject tagged 'Player' found. Please assign the player tag.");
            enabled = false;
        }

        if (groundTilemap == null || groundRuleTile == null) {
            Debug.LogError("Ground Tilemap or Ground Rule Tile not assigned in the Inspector.");
            enabled = false;
        }

        lastPlayerCellPos = Vector3Int.one * int.MinValue; // Ensure first update triggers painting
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Process a limited number of tile operations per frame
        int ops = 0;
        bool updatedAStar = false;
        while (tileOpsQueue.Count > 0 && ops < maxTilesPerFrame) {
            tileOpsQueue.Dequeue().Invoke();
            ops++;
            updatedAStar = true;
        }

        if (updatedAStar) {
            UpdateAStarGrid();
        }

        if (Time.time >= nextCheckTime) {
            Vector3Int playerCellPos = groundTilemap.WorldToCell(playerTransform.position);
            if (playerCellPos != lastPlayerCellPos) {
                EnqueueTileOps(playerCellPos);
                lastPlayerCellPos = playerCellPos;
            }
            nextCheckTime = Time.time + checkInterval;
        }
    }

    void EnqueueTileOps(Vector3Int _playerCellPos)
    {
        Vector3 cellSize = groundTilemap.cellSize;
        int cellRadiusX = Mathf.CeilToInt(paintRadius / cellSize.x);
        int cellRadiusY = Mathf.CeilToInt(paintRadius / cellSize.y);

        HashSet<Vector3Int> newTiles = new();
        for (int x = -cellRadiusX; x <= cellRadiusX; x++) {
            for (int y = -cellRadiusY; y <= cellRadiusY; y++) {
                Vector3Int pos = new(_playerCellPos.x + x, _playerCellPos.y + y, 0);
                newTiles.Add(pos);
            }
        }

        // Remove tiles that are no longer in the area
        foreach (var pos in paintedTiles) {
            if (!newTiles.Contains(pos)) {
                Vector3Int removePos = pos;
                tileOpsQueue.Enqueue(() => groundTilemap.SetTile(removePos, null));
            }
        }

        // Add new tiles
        foreach (var pos in newTiles) {
            if (!paintedTiles.Contains(pos)) {
                Vector3Int addPos = pos;
                // Randomly place obstacles
                if (obstacleTile != null && obstacleTilemap != null) {
                    float distanceToPlayer = Vector3.Distance(groundTilemap.CellToWorld(addPos) + groundTilemap.cellSize / 2, playerTransform.position);
                    if (distanceToPlayer >= obstacleMinDistance && Random.value < obstacleChance) {
                        tileOpsQueue.Enqueue(() => obstacleTilemap.SetTile(addPos, obstacleTile));
                        // Chance to place additional grouped obstacles
                        if (Random.value < obstacleGroupChance) {
                            List<Vector3Int> neighbors = new() {
                                new Vector3Int(addPos.x + 1, addPos.y, 0),
                                new Vector3Int(addPos.x - 1, addPos.y, 0),
                                new Vector3Int(addPos.x, addPos.y + 1, 0),
                                new Vector3Int(addPos.x, addPos.y - 1, 0)
                            };
                            foreach (var neighbor in neighbors) {
                                if (Random.value < 0.5f) { // 50% chance to place in each neighbor
                                    tileOpsQueue.Enqueue(() => obstacleTilemap.SetTile(neighbor, obstacleTile));
                                }
                            }
                        }
                    }
                }
                tileOpsQueue.Enqueue(() => groundTilemap.SetTile(addPos, groundRuleTile));
            }
        }
        paintedTiles = newTiles;
    }

    private void UpdateAStarGrid()
    {
        var updateBounds = new Bounds(transform.position, new(paintRadius, paintRadius, 1));
        AstarPath.active.UpdateGraphs(updateBounds);
    }
}