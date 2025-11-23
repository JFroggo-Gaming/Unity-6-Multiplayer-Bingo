using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

namespace BingoGame.Network
{
    public class MainMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private PlayerCountSelection playerCountSelection;
        [SerializeField] private GameObject joinPanel;
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private Button confirmJoinButton;
        [SerializeField] private Button cancelJoinButton;
        [SerializeField] private GameObject errorHostLeftPanel;
        [SerializeField] private Button errorOkButton;
        [SerializeField] private GameObject errorConnectionFailedPanel;
        [SerializeField] private Button errorConnectionOkButton;
        [SerializeField] private GameObject errorNoHostPanel;
        [SerializeField] private Button errorNoHostOkButton;

        private BingoNetworkManager networkManager;
        private bool isAttemptingToConnect = false;
        private float connectionTimeout = 10f; // 10 sekund timeout (Mirror potrzebuje czasu na synchronizację)
        private float connectionAttemptStartTime;

        private void Start()
        {
            networkManager = BingoNetworkManager.Instance;
            if (networkManager == null)
            {
                networkManager = FindObjectOfType<BingoNetworkManager>();
            }

            // Setup button listeners
            hostButton.onClick.AddListener(OnHostButtonClicked);
            joinButton.onClick.AddListener(OnJoinButtonClicked);

            if (confirmJoinButton != null)
                confirmJoinButton.onClick.AddListener(OnConfirmJoin);
            if (cancelJoinButton != null)
                cancelJoinButton.onClick.AddListener(OnCancelJoin);
            if (errorOkButton != null)
                errorOkButton.onClick.AddListener(OnErrorHostLeftOkClicked);
            if (errorConnectionOkButton != null)
                errorConnectionOkButton.onClick.AddListener(OnErrorConnectionOkClicked);
            if (errorNoHostOkButton != null)
                errorNoHostOkButton.onClick.AddListener(OnErrorNoHostOkClicked);

            // Hide panels initially
            if (joinPanel != null)
                joinPanel.SetActive(false);
            if (errorHostLeftPanel != null)
                errorHostLeftPanel.SetActive(false);
            if (errorConnectionFailedPanel != null)
                errorConnectionFailedPanel.SetActive(false);
            if (errorNoHostPanel != null)
                errorNoHostPanel.SetActive(false);
        }

        private void OnEnable()
        {
            NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
            NetworkClient.OnConnectedEvent += OnClientConnected;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;
            NetworkClient.OnConnectedEvent -= OnClientConnected;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {

            // Hide main menu when GameScene loads
            if (scene.name == "GameScene")
            {
                if (mainMenuPanel != null)
                {
                    mainMenuPanel.SetActive(false);
                }
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Check for connection timeout
            if (isAttemptingToConnect)
            {
                // Check if we have a local player spawned - if yes, connection succeeded
                if (NetworkClient.localPlayer != null)
                {
                    Debug.Log("Local player spawned - connection successful!");
                    isAttemptingToConnect = false;

                    if (joinPanel != null)
                    {
                        joinPanel.SetActive(false);
                    }
                    return;
                }

                // Timeout check
                if (Time.time - connectionAttemptStartTime > connectionTimeout)
                {
                    Debug.Log($"Connection timeout after {connectionTimeout} seconds - showing NoHost error");
                    isAttemptingToConnect = false;

                    // Stop the client connection attempt
                    if (networkManager != null)
                    {
                        networkManager.StopClient();
                    }

                    ShowNoHostError();
                }
            }
        }

        private void OnClientConnected()
        {
            // Successfully connected to host
            isAttemptingToConnect = false;
            if (joinPanel != null)
            {
                joinPanel.SetActive(false);
            }
        }

        private void OnClientDisconnected()
        {

            // Only show error if we were a client (not host)
            if (!NetworkServer.active)
            {
                if (isAttemptingToConnect)
                {
                    ShowNoHostError();
                    isAttemptingToConnect = false;
                }
                else
                {
                    ShowHostLeftError();
                }
            }
            else
            {
                // niemozliwe tak bardzo,ze az mi sie nie chce pisac loga pod to
            }
        }

        public void ShowHostLeftError()
        {
            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.gameObject.SetActive(false);
            }

            ShowMainMenu();

            // Show error panel
            if (errorHostLeftPanel != null)
            {
                errorHostLeftPanel.SetActive(true);
            }
        }

        private void ShowConnectionFailedError()
        {
            // Show error panel
            if (errorConnectionFailedPanel != null)
            {
                errorConnectionFailedPanel.SetActive(true);
            }
        }

        private void ShowNoHostError()
        {
            if (errorNoHostPanel != null)
            {
                errorNoHostPanel.SetActive(true);
            }
        }

        private void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
        }

        private void OnErrorHostLeftOkClicked()
        {
            if (errorHostLeftPanel != null)
            {
                errorHostLeftPanel.SetActive(false);
            }
        }

        private void OnErrorConnectionOkClicked()
        {
            if (errorConnectionFailedPanel != null)
            {
                errorConnectionFailedPanel.SetActive(false);
            }
        }

        private void OnErrorNoHostOkClicked()
        {
            if (errorNoHostPanel != null)
            {
                errorNoHostPanel.SetActive(false);
            }
        }

        private void OnHostButtonClicked()
        {
            Debug.Log("Host button clicked");
            // Show player count selection
            mainMenuPanel.SetActive(false);
            playerCountSelection.Show();
        }

        private void OnJoinButtonClicked()
        {
            Debug.Log("Join button clicked");
            // Hide main menu, show join panel
            mainMenuPanel.SetActive(false);

            if (joinPanel != null)
            {
                joinPanel.SetActive(true);
                // Set default localhost IP
                if (ipInputField != null)
                {
                    ipInputField.text = "localhost";
                }
            }
        }

        private void OnConfirmJoin()
        {
            string ip = ipInputField.text;
            if (string.IsNullOrEmpty(ip))
            {
                ip = "localhost";
            }


            // Validate IP (basic check)
            if (!IsValidIP(ip))
            {
                ShowConnectionFailedError();
                return;
            }

            networkManager.networkAddress = ip;

            try
            {

                isAttemptingToConnect = true;
                connectionAttemptStartTime = Time.time;

                networkManager.StartClient();
                // DON'T hide join panel yet - wait for connection success
                // It will be hidden in OnClientConnected() if successful
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception during StartClient: {e.Message}");
                isAttemptingToConnect = false;
                ShowConnectionFailedError();
            }
        }

        private bool IsValidIP(string ip)
        {
            // Basic validation - allow localhost or IP format
            if (ip == "localhost" || ip == "127.0.0.1")
                return true;

            // Check if it's a valid IP address format
            string[] parts = ip.Split('.');
            if (parts.Length == 4)
            {
                foreach (string part in parts)
                {
                    if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                        return false;
                }
                return true;
            }

            return false;
        }

        private void OnCancelJoin()
        {
            if (joinPanel != null)
                joinPanel.SetActive(false);

            // Return to main menu
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (hostButton != null)
                hostButton.onClick.RemoveListener(OnHostButtonClicked);
            if (joinButton != null)
                joinButton.onClick.RemoveListener(OnJoinButtonClicked);
            if (confirmJoinButton != null)
                confirmJoinButton.onClick.RemoveListener(OnConfirmJoin);
            if (cancelJoinButton != null)
                cancelJoinButton.onClick.RemoveListener(OnCancelJoin);
            if (errorOkButton != null)
                errorOkButton.onClick.RemoveListener(OnErrorHostLeftOkClicked);
            if (errorConnectionOkButton != null)
                errorConnectionOkButton.onClick.RemoveListener(OnErrorConnectionOkClicked);
            if (errorNoHostOkButton != null)
                errorNoHostOkButton.onClick.RemoveListener(OnErrorNoHostOkClicked);
        }
    }
}
