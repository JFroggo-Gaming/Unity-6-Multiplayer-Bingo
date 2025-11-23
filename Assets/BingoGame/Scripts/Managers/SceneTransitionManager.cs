using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;

namespace BingoGame.Network
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string lobbySceneName = "MainScene";
        [SerializeField] private string gameSceneName = "GameScene";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Server loads GameScene for all players
        /// </summary>
        public void LoadGameScene()
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[SceneTransitionManager] LoadGameScene called but not server");
                return;
            }

            Debug.Log($"[Server] Loading GameScene: {gameSceneName}");

            // Mirror's NetworkManager handles scene loading for all clients
            BingoNetworkManager networkManager = BingoNetworkManager.Instance;
            if (networkManager != null)
            {
                networkManager.ServerChangeScene(gameSceneName);
            }
            else
            {
                Debug.LogError("NetworkManager not found!");
            }
        }

        public void LoadLobbyScene()
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[SceneTransitionManager] LoadLobbyScene called but not server");
                return;
            }

            Debug.Log($"[Server] Loading Lobby Scene: {lobbySceneName}");

            BingoNetworkManager networkManager = BingoNetworkManager.Instance;
            if (networkManager != null)
            {
                networkManager.ServerChangeScene(lobbySceneName);
            }
            else
            {
                Debug.LogError("NetworkManager not found!");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[SceneTransitionManager] Scene loaded: {scene.name}, NetworkServer.active: {NetworkServer.active}, NetworkClient.isConnected: {NetworkClient.isConnected}");

            if (scene.name == gameSceneName)
            {
                Debug.Log($"[SceneTransitionManager] GameScene detected, starting delayed spawn");
                StartCoroutine(OnGameSceneLoadedDelayed());
            }
        }

        private IEnumerator OnGameSceneLoadedDelayed()
        {
            // Wait for scene to fully load
            yield return new WaitForSeconds(1f);
            Debug.Log($"After wait - NetworkServer.active: {NetworkServer.active}, NetworkClient.isConnected: {NetworkClient.isConnected}");

            // Only spawn players on server!
            if (!NetworkServer.active)
            {
                yield break;
            }

            Debug.Log("[SceneTransitionManager] IS SERVER - proceeding with player spawning");

            // Find SpawnPlayersHandler and setup player (SERVER ONLY)
            SpawnPlayersHandler spawnHandler = FindObjectOfType<SpawnPlayersHandler>();
            if (spawnHandler != null)
            {
                spawnHandler.OnSceneReady();
            }
            else
            {
                Debug.LogError("SpawnPlayersHandler not found in GameScene");
            }
        }
    }
}
