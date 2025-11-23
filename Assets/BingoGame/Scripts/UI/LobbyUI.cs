using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace BingoGame.Network
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Transform[] playerSlotTransforms;
        [SerializeField] private Button playButton;  // Start game button
        [SerializeField] private Button exitButton;
        [SerializeField] private Text playerCountText;
        [SerializeField] private GameObject mainMenu;

        private BingoNetworkManager networkManager;
        private int requiredPlayerCount = 2;

        private void Start()
        {
            networkManager = BingoNetworkManager.Instance;
            if (networkManager == null)
            {
                networkManager = FindObjectOfType<BingoNetworkManager>();
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
                playButton.interactable = false;
            }
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitClicked);
            }

            lobbyPanel.SetActive(false);
        }

        private void OnExitClicked()
        {
            Debug.Log("Exit button clicked in lobby");

            // Stop client/host
            if (NetworkServer.active && NetworkClient.active)
            {
                networkManager.StopHost();
            }
            else if (NetworkClient.active)
            {
                networkManager.StopClient();
            }

            // Hide lobby
            lobbyPanel.SetActive(false);

            // Show main menu (no fade, just activate)
            if (mainMenu != null)
            {
                mainMenu.SetActive(true);
            }
        }

        public Transform GetPlayerSlot(int index)
        {
            if (index >= 0 && index < playerSlotTransforms.Length)
            {
                return playerSlotTransforms[index];
            }
            return null;
        }

        public void Show(int playerCount)
        {
            requiredPlayerCount = playerCount;

            if (lobbyPanel == null)
            {
                Debug.LogError("LobbyPanel is null!");
                return;
            }

            lobbyPanel.SetActive(true);
            UpdateUI();
        }

        public void OnPlayerJoined(BingoPlayer player)
        {
            // Skip if player index is not set yet
            if (player.playerIndex < 0)
            {
                Debug.LogWarning($"Player index not set yet, skipping");
                return;
            }

            UpdateUI();
        }

        public void OnPlayerLeft(BingoPlayer player)
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (playerCountText != null)
            {
                int currentPlayers = networkManager.ConnectedPlayers.Count;
                playerCountText.text = $"Gracze: {currentPlayers}/{requiredPlayerCount}";
            }

            if (playButton != null)
            {
                // Only host can start the game
                bool isHost = NetworkServer.active && NetworkClient.active;
                bool canStart = networkManager.CanStartGame();

                playButton.interactable = isHost && canStart;
            }
        }

        private void OnPlayClicked()
        {
            if (NetworkServer.active)
            {
                networkManager.StartBingoGame();
            }
        }

        public void OnGameStarted()
        {
            // Close lobby UI
            lobbyPanel.SetActive(false);
        }

        private void Update()
        {
            if (lobbyPanel.activeSelf)
            {
                UpdateUI();
            }
        }
    }
}
