using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace BingoGame.Network
{
    public class SpawnPlayersHandler : NetworkBehaviour
    {
        public static SpawnPlayersHandler Instance { get; private set; }

        [Header("Game Player Prefab")]
        [SerializeField] private GameObject gamePlayerPrefab;

        [Header("Player Spawn Points")]
        [SerializeField] private Transform[] playerSpawnPoints = new Transform[6];

        private List<int> availableSpawnIndices = new List<int>();
        private Dictionary<int, GamePlayer> spawnedGamePlayers = new Dictionary<int, GamePlayer>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            InitializeSpawnPoints();
        }

        private void InitializeSpawnPoints()
        {
            availableSpawnIndices.Clear();
            for (int i = 0; i < 6; i++)
            {
                availableSpawnIndices.Add(i);
            }
        }

        public void OnSceneReady()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (gamePlayerPrefab == null)
            {
                Debug.LogError("GamePlayer prefab not assigned!");
                return;
            }

            int spawnedCount = 0;
            foreach (var kvp in NetworkServer.connections)
            {
                NetworkConnectionToClient conn = kvp.Value;
                if (conn != null && conn.identity != null)
                {
                    BingoPlayer lobbyPlayer = conn.identity.GetComponent<BingoPlayer>();
                    if (lobbyPlayer != null)
                    {
                        SpawnGamePlayer(lobbyPlayer);
                        spawnedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"Connection {conn.connectionId} has identity but no BingoPlayer component!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Connection {kvp.Key} has null connection or identity!");
                }
            }
        }

        [Server]
        private void SpawnGamePlayer(BingoPlayer lobbyPlayer)
        {
            Debug.Log($"Starting spawn for player {lobbyPlayer.playerIndex}");

            if (availableSpawnIndices.Count == 0)
            {
                Debug.LogError("[SpawnPlayersHandler] No available spawn points!");
                return;
            }

            // Get random spawn point
            int randomIndex = Random.Range(0, availableSpawnIndices.Count);
            int spawnIndex = availableSpawnIndices[randomIndex];
            availableSpawnIndices.RemoveAt(randomIndex);

            if (spawnIndex < 0 || spawnIndex >= playerSpawnPoints.Length)
            {
                Debug.LogError($"Invalid spawn index: {spawnIndex}");
                return;
            }

            Transform spawnPoint = playerSpawnPoints[spawnIndex];
            if (spawnPoint == null)
            {
                Debug.LogError($"pawn point at index {spawnIndex}null!");
                return;
            }

            // Spawn GamePlayer
            GameObject gamePlayerObj = Instantiate(gamePlayerPrefab, spawnPoint.position, spawnPoint.rotation);
            GamePlayer gamePlayer = gamePlayerObj.GetComponent<GamePlayer>();

            if (gamePlayer != null)
            {
                // Copy data from lobby player
                gamePlayer.playerIndex = lobbyPlayer.playerIndex;
                gamePlayer.playerName = lobbyPlayer.playerName;

                NetworkConnectionToClient conn = lobbyPlayer.connectionToClient;

                // IMPORTANT: Replace the player object for this connection
                // This makes GamePlayer the new "local player" for this connection
                if (conn != null)
                {
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerObj, true);
                }
                else
                {
                    Debug.LogError($"Connection is null!");
                    NetworkServer.Spawn(gamePlayerObj);
                }

                // Track spawned player
                spawnedGamePlayers[lobbyPlayer.playerIndex] = gamePlayer;
            }
            else
            {
                Debug.LogError("GamePlayer component not found on spawned prefab!");
                Destroy(gamePlayerObj);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
