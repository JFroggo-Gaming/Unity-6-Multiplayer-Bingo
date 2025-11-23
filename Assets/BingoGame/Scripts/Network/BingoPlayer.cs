using Mirror;
using UnityEngine;

namespace BingoGame.Network
{
    public class BingoPlayer : NetworkBehaviour
    {
        [Header("Visual Prefabs")]
        [SerializeField] private GameObject[] visualPrefabs = new GameObject[6];

        [SyncVar(hook = nameof(OnPrefabIndexChanged))]
        public int prefabIndex = -1;  // Which prefab variant this player is using (0-5)

        [SyncVar]
        public int playerIndex = -1;

        [SyncVar]
        public string playerName = "Player";

        [SyncVar]
        public bool isReady = false;

        private GameObject spawnedVisual;

        public override void OnStartClient()
        {
            base.OnStartClient();

            Debug.Log($"[OnStartClient] Player {playerIndex} started. IsLocalPlayer: {isLocalPlayer}, prefabIndex: {prefabIndex}, spawnedVisual: {(spawnedVisual != null ? spawnedVisual.name : "null")}");

            // Position this player in the correct lobby slot
            if (playerIndex >= 0)
            {
                LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
                if (lobbyUI != null)
                {
                    Transform slot = lobbyUI.GetPlayerSlot(playerIndex);
                    if (slot != null)
                    {
                        transform.SetParent(slot, false);
                        transform.localPosition = Vector3.zero;
                        transform.localRotation = Quaternion.identity;
                        transform.localScale = Vector3.one;
                        Debug.Log($"[OnStartClient] Player {playerIndex} positioned in lobby slot");
                    }
                }
            }

            // Always try to spawn visual if we have a valid prefabIndex and no visual yet
            if (spawnedVisual == null && prefabIndex >= 0)
            {
                Debug.Log($"[OnStartClient] Spawning visual for player {playerIndex}, prefabIndex: {prefabIndex}");
                SpawnVisualPrefab();
            }
        }

        private void OnPrefabIndexChanged(int oldIndex, int newIndex)
        {
            Debug.Log($"[OnPrefabIndexChanged] Player {playerIndex}: {oldIndex} → {newIndex}");

            // Destroy old visual if exists
            if (spawnedVisual != null)
            {
                Debug.Log($"[OnPrefabIndexChanged] Destroying old visual for player {playerIndex}");
                Destroy(spawnedVisual);
            }

            // Spawn new visual only if we don't already have one
            // (OnStartClient might have already spawned it)
            if (newIndex >= 0 && spawnedVisual == null)
            {
                Debug.Log($"[OnPrefabIndexChanged] Spawning new visual for player {playerIndex}");
                SpawnVisualPrefab();
            }
        }

        private void SpawnVisualPrefab()
        {
            Debug.Log($"SpawnVisualPrefab called. prefabIndex: {prefabIndex}, visualPrefabs.Length: {visualPrefabs.Length}");

            if (visualPrefabs == null || visualPrefabs.Length == 0)
            {
                Debug.LogError("Visual prefabs array is null or empty! Make sure to assign visual prefabs in the BingoPlayer prefab inspector.");
                return;
            }

            if (prefabIndex < 0 || prefabIndex >= visualPrefabs.Length)
            {
                Debug.LogError($"Invalid prefabIndex: {prefabIndex} (must be 0-{visualPrefabs.Length - 1})");
                return;
            }

            GameObject prefab = visualPrefabs[prefabIndex];
            if (prefab == null)
            {
                Debug.LogError($"Visual prefab at index {prefabIndex} is null! Check inspector assignments.");
                return;
            }

            // Spawn visual as child of this network object
            spawnedVisual = Instantiate(prefab, transform);
            spawnedVisual.transform.localPosition = Vector3.zero;
            spawnedVisual.transform.localRotation = Quaternion.identity;

            // Note: playerIndex might still be -1 here if called from SyncVar hook during deserialization
            // It will be updated shortly, so we log in OnStartClient instead
        }

        public override void OnStopClient()
        {
            // Cleanup visual prefab
            if (spawnedVisual != null)
            {
                Destroy(spawnedVisual);
            }

            // Notify lobby UI that a player has left
            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.OnPlayerLeft(this);
            }

            base.OnStopClient();
        }

        [Command]
        public void CmdSetReady(bool ready)
        {
            isReady = ready;
        }

        [Command]
        public void CmdSetPlayerName(string name)
        {
            playerName = name;
        }

        [ClientRpc]
        public void RpcShowLobby(int requiredPlayerCount)
        {
            Debug.Log($"RpcShowLobby called for player {playerIndex} with required count: {requiredPlayerCount}");
            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                Debug.Log($"LobbyUI found, showing lobby");
                lobbyUI.Show(requiredPlayerCount);
            }
            else
            {
                // LobbyUI might not exist if we're in GameScene - this is normal
                Debug.Log($"LobbyUI not found (probably in GameScene now)");
            }
        }

        [ClientRpc]
        public void RpcRefreshLobby()
        {
            Debug.Log($"[RpcRefreshLobby] Called on player {playerIndex}");
            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                // Find all players and refresh lobby
                BingoPlayer[] allPlayers = FindObjectsOfType<BingoPlayer>();
                Debug.Log($"[RpcRefreshLobby] Found {allPlayers.Length} players");

                // Just update the UI - avatars are already spawned by each BingoPlayer
                lobbyUI.UpdateUI();
            }
        }

        [TargetRpc]
        public void TargetRefreshAllPlayers(NetworkConnectionToClient target)
        {
            Debug.Log($"TargetRefreshAllPlayers called for connection");
            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                // Find all players and add them to lobby
                BingoPlayer[] allPlayers = FindObjectsOfType<BingoPlayer>();
                Debug.Log($"Found {allPlayers.Length} players to refresh");
                foreach (BingoPlayer player in allPlayers)
                {
                    if (player != null)
                    {
                        Debug.Log($"Adding player {player.playerIndex} to lobby");
                        lobbyUI.OnPlayerJoined(player);
                    }
                }
            }
        }

        [ClientRpc]
        public void RpcGameStarted()
        {
            Debug.Log("Gra rozpoczęta (Client)");
            // Close lobby UI for all players
            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.OnGameStarted();
            }

            // Hide BingoPlayer visual in GameScene
            if (spawnedVisual != null)
            {
                spawnedVisual.SetActive(false);
                Debug.Log($"[BingoPlayer] Hiding visual for player {playerIndex} during game");
            }
        }
    }
}
