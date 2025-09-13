
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("The Tilemap to paint tiles on")]
    private Tilemap targetTilemap;
    [SerializeField, Tooltip("The RuleTile to use for painting")]
    private RuleTile ruleTile;
    [SerializeField, Tooltip("Radius around the player to paint (in world units)")]
    private float paintRadius = 50.0f;
    [SerializeField, Tooltip("How often to check and paint (in seconds)")]
    private float checkInterval = 1.0f;
    [SerializeField, Tooltip("Limit how many tiles are set per frame for performance")]
    private int maxTilesPerFrame = 100;

    private Transform playerTransform;
    private float nextCheckTime = 0f;
    private HashSet<Vector3Int> paintedTiles = new HashSet<Vector3Int>();
    private Vector3Int lastPlayerCellPos;
    private Queue<System.Action> tileOpsQueue = new Queue<System.Action>();

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            playerTransform = playerObj.transform;
        } else {
            Debug.LogError("No GameObject tagged 'Player' found. Please assign the player tag.");
            enabled = false;
        }

        if (targetTilemap == null || ruleTile == null) {
            Debug.LogError("Target Tilemap or Rule Tile not assigned in the Inspector.");
            enabled = false;
        }

        lastPlayerCellPos = Vector3Int.one * int.MinValue; // Ensure first update triggers painting
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Process a limited number of tile operations per frame
        int ops = 0;
        while (tileOpsQueue.Count > 0 && ops < maxTilesPerFrame) {
            tileOpsQueue.Dequeue().Invoke();
            ops++;
        }

        if (Time.time >= nextCheckTime) {
            Vector3Int playerCellPos = targetTilemap.WorldToCell(playerTransform.position);
            if (playerCellPos != lastPlayerCellPos) {
                EnqueueTileOps(playerCellPos);
                lastPlayerCellPos = playerCellPos;
            }
            nextCheckTime = Time.time + checkInterval;
        }
    }

    void EnqueueTileOps(Vector3Int _playerCellPos)
    {
        Vector3 cellSize = targetTilemap.cellSize;
        int cellRadiusX = Mathf.CeilToInt(paintRadius / cellSize.x);
        int cellRadiusY = Mathf.CeilToInt(paintRadius / cellSize.y);

        HashSet<Vector3Int> newTiles = new HashSet<Vector3Int>();
        for (int x = -cellRadiusX; x <= cellRadiusX; x++) {
            for (int y = -cellRadiusY; y <= cellRadiusY; y++) {
                Vector3Int pos = new Vector3Int(_playerCellPos.x + x, _playerCellPos.y + y, 0);
                newTiles.Add(pos);
            }
        }

        // Remove tiles that are no longer in the area
        foreach (var pos in paintedTiles) {
            if (!newTiles.Contains(pos)) {
                Vector3Int removePos = pos;
                tileOpsQueue.Enqueue(() => targetTilemap.SetTile(removePos, null));
            }
        }

        // Add new tiles
        foreach (var pos in newTiles) {
            if (!paintedTiles.Contains(pos)) {
                Vector3Int addPos = pos;
                tileOpsQueue.Enqueue(() => targetTilemap.SetTile(addPos, ruleTile));
            }
        }

        paintedTiles = newTiles;
    }
}