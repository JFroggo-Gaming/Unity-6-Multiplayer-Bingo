using Mirror;
using UnityEngine;
using System.Collections.Generic;

namespace BingoGame.Network
{
    public class BingoNetworkManager : NetworkManager
    {
        [Header("Bingo Settings")]
        [SerializeField] private int maxPlayers = 6;
        [SerializeField] private int minPlayersToStart = 2;

        [Header("Game Prefabs")]
        [SerializeField] private GameObject gamePlayerPrefab;  // Assign GamePlayer prefab here

        public static BingoNetworkManager Instance { get; private set; }

        public int RequiredPlayerCount { get; set; } = 2;
        public List<BingoPlayer> ConnectedPlayers { get; private set; } = new List<BingoPlayer>();

        private List<int> availableVisualIndices = new List<int>();
        private List<int> availableSlotIndices = new List<int>();

        public override void Awake()
        {
            base.Awake();
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            // Initialize available visual indices (0-5)
            InitializeAvailableVisuals();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Register GamePlayer prefab for network spawning
            if (gamePlayerPrefab != null)
            {
                NetworkClient.RegisterPrefab(gamePlayerPrefab);
                Debug.Log("[BingoNetworkManager] GamePlayer prefab registered");
            }
            else
            {
                Debug.LogError("[BingoNetworkManager] GamePlayer prefab not assigned!");
            }
        }

        private void InitializeAvailableVisuals()
        {
            availableVisualIndices.Clear();
            availableSlotIndices.Clear();

            for (int i = 0; i < 6; i++)  // 6 visual variants and 6 slots
            {
                availableVisualIndices.Add(i);
                availableSlotIndices.Add(i);
            }
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // Check if we have room for more players
            if (numPlayers >= maxPlayers)
            {
                Debug.LogWarning($"Server full. Max players: {maxPlayers}");
                return;
            }

            // Check if we have available visual variants
            if (availableVisualIndices.Count == 0)
            {
                Debug.LogError("No available visual variants!");
                return;
            }

            // Get random visual variant from available ones
            int randomVisualIndex = Random.Range(0, availableVisualIndices.Count);
            int visualIndex = availableVisualIndices[randomVisualIndex];
            availableVisualIndices.RemoveAt(randomVisualIndex);

            // Get next available slot (sequential, not random)
            int slotIndex = availableSlotIndices[0];
            availableSlotIndices.RemoveAt(0);

            // Spawn BingoPlayer (always the same prefab)
            GameObject player = Instantiate(playerPrefab);

            BingoPlayer bingoPlayer = player.GetComponent<BingoPlayer>();
            if (bingoPlayer != null)
            {
                // Set player data BEFORE adding to network (so it syncs correctly)
                bingoPlayer.playerIndex = slotIndex;  // Use slot index instead of list position
                bingoPlayer.prefabIndex = visualIndex; // This will trigger visual spawning via SyncVar hook

                // Now add player to connection (this triggers sync to clients)
                NetworkServer.AddPlayerForConnection(conn, player);

                // Track player
                ConnectedPlayers.Add(bingoPlayer);
                Debug.Log($"Player connected: slot {slotIndex}, visual variant {visualIndex}. Total players: {ConnectedPlayers.Count}");

                // Position player in lobby slot (find lobby UI and get slot transform)
                LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
                if (lobbyUI != null)
                {
                    Transform slot = lobbyUI.GetPlayerSlot(slotIndex);
                    if (slot != null)
                    {
                        player.transform.SetParent(slot, false);
                        player.transform.localPosition = Vector3.zero;
                        player.transform.localRotation = Quaternion.identity;
                        player.transform.localScale = Vector3.one;
                        Debug.Log($"Player positioned in lobby slot {slotIndex}");
                    }
                }

                // Show lobby for all players
                Debug.Log($"Showing lobby for all players. RequiredPlayerCount: {RequiredPlayerCount}");
                foreach (BingoPlayer p in ConnectedPlayers)
                {
                    if (p != null)
                    {
                        Debug.Log($"Calling RpcShowLobby on player {p.playerIndex}");
                        p.RpcShowLobby(RequiredPlayerCount);

                        // Tell each player to refresh their lobby (to see all players including the new one)
                        p.RpcRefreshLobby();
                    }
                }
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            // Remove player from list and return their visual variant and slot to available pool
            if (conn.identity != null)
            {
                BingoPlayer bingoPlayer = conn.identity.GetComponent<BingoPlayer>();
                if (bingoPlayer != null)
                {
                    // Return visual variant and slot to available pool
                    availableVisualIndices.Add(bingoPlayer.prefabIndex);
                    availableSlotIndices.Add(bingoPlayer.playerIndex);
                    Debug.Log($"Player slot {bingoPlayer.playerIndex} with visual {bingoPlayer.prefabIndex} disconnected. Returned to pool.");

                    ConnectedPlayers.Remove(bingoPlayer);
                    Debug.Log($"Remaining players: {ConnectedPlayers.Count}");
                }
            }

            base.OnServerDisconnect(conn);
        }

        public bool CanStartGame()
        {
            return ConnectedPlayers.Count >= minPlayersToStart &&
                   ConnectedPlayers.Count >= RequiredPlayerCount;
        }

        public void StartBingoGame()
        {
            if (!NetworkServer.active) return;

            if (CanStartGame())
            {
                Debug.Log("Gra rozpoczęta!");

                // Notify all players to start the game
                foreach (BingoPlayer player in ConnectedPlayers)
                {
                    if (player != null)
                    {
                        player.RpcGameStarted();
                    }
                }

                // Load GameScene
                SceneTransitionManager sceneManager = SceneTransitionManager.Instance;
                if (sceneManager != null)
                {
                    sceneManager.LoadGameScene();
                }
                else
                {
                    Debug.LogError("SceneTransitionManager not found!");
                }
            }
            else
            {
                Debug.LogWarning($"Nie można rozpocząć gry. Graczy: {ConnectedPlayers.Count}/{RequiredPlayerCount}");
            }
        }

        public override void OnStopServer()
        {
            Debug.Log("[BingoNetworkManager] Server stopping - cleaning up");
            ConnectedPlayers.Clear();
            // Reset available visual variants for next game
            InitializeAvailableVisuals();
            base.OnStopServer();
        }

        public override void OnStopClient()
        {
            Debug.Log("[BingoNetworkManager] Client stopping - cleaning up");
            base.OnStopClient();
        }

        private void OnApplicationQuit()
        {
            // Force cleanup on application quit
            if (NetworkServer.active)
            {
                NetworkServer.Shutdown();
            }
            if (NetworkClient.isConnected)
            {
                NetworkClient.Shutdown();
            }
        }
    }
}
