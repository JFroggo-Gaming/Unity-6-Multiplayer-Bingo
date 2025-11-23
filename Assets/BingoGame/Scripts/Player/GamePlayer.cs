using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BingoGame.Network
{

    // Handles player in actual game (not lobby)
    public class GamePlayer : NetworkBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CameraMovement cameraMovement;
        [SerializeField] private GameObject playerModel; // Visual representation of player

        [Header("UI")]
        [SerializeField] private Canvas playerCanvas;
        [SerializeField] private BingoCard bingoCard;
        [SerializeField] private PatternDisplay patternDisplay;
        [SerializeField] private DrawnNumbersList drawnNumbersList;
        [SerializeField] private CountdownTimer countdownTimer;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Button winOkButton;
        [SerializeField] private Button loseOkButton;

        [Header("Player Data")]
        [SyncVar]
        public int playerIndex = -1;

        [SyncVar]
        public string playerName = "Player";

        private void Start()
        {
            // Hide win/lose panels initially
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);

            if (winOkButton != null)
            {
                winOkButton.onClick.AddListener(OnOkButtonClicked);
            }
            if (loseOkButton != null)
            {
                loseOkButton.onClick.AddListener(OnOkButtonClicked);
            }
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            // Host can reset game with ESC key
            if (Input.GetKeyDown(KeyCode.Escape))
            {

                if (isServer || NetworkServer.active)
                {
                    CmdResetGame();
                }
                else
                {
                    Debug.LogWarning("ESC pressed but not host - cannot reset game");
                }
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Disable UI for non-local players
            if (!isLocalPlayer && playerCanvas != null)
            {
                playerCanvas.enabled = false;
            }

            // SHOW other players' models so we can see them
            if (!isLocalPlayer && playerModel != null)
            {
                playerModel.SetActive(true);
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // Enable camera only for local player
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                playerCamera.gameObject.SetActive(true);

                // Setup Canvas to use UI camera
                if (playerCanvas != null)
                {
                    playerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    playerCanvas.planeDistance = 100f; // po co ja to zrobilem? ale dziala
                    playerCanvas.enabled = true;

                    UnityEngine.UI.GraphicRaycaster raycaster = playerCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    if (raycaster == null)
                    {
                        raycaster = playerCanvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }
                    else
                    {
                        Debug.Log($"GraphicRaycaster already exists on Canvas for player {playerIndex}");
                    }

                    raycaster.enabled = true;
                }

                EnsureEventSystem();

                InitializeBingoUI();
            }
            else
            {
                Debug.LogError($"Camera reference is NULL for local player {playerIndex}");
            }

            // Enable camera movement only for local player
            if (cameraMovement != null)
            {
                cameraMovement.enabled = true;
            }
            else
            {
                Debug.LogWarning($"CameraMovement component not found for player {playerIndex}");
            }

            // Disable cameras on other GamePlayer instances
            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();
            foreach (GamePlayer gp in allPlayers)
            {
                if (gp != this && gp.playerCamera != null)
                {
                    gp.playerCamera.enabled = false;
                }
            }
        }

        private void EnsureEventSystem()
        {
            // Check if EventSystem exists
            EventSystem eventSystem = FindObjectOfType<EventSystem>();

            if (eventSystem == null)
            {
                Debug.LogWarning("No EventSystem found in scene! Creating one...");

                // Create EventSystem GameObject
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }
            else
            {
                Debug.Log($"EventSystem found: {eventSystem.name}");
            }

            StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
        }

        private void InitializeBingoUI()
        {
            // Initialize Bingo card with unique numbers
            if (bingoCard != null)
            {
                bingoCard.Initialize(this);
            }
            else
            {
                Debug.LogWarning($"BingoCard not assigned!");
            }
        }

        public void ShowWinPanel()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
            if (losePanel != null)
            {
                losePanel.SetActive(false);
            }
        }

        public void ShowLosePanel()
        {
            if (losePanel != null)
            {
                losePanel.SetActive(true);
            }
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }
        }

        public void HidePanels()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }
            if (losePanel != null)
            {
                losePanel.SetActive(false);
            }
        }

        private void OnOkButtonClicked()
        {
            CmdResetGame();
        }

        [Command(requiresAuthority = false)]
        private void CmdResetGame(NetworkConnectionToClient sender = null)
        {
            if (BingoManager.Instance != null)
            {
                BingoManager.Instance.ResetGame();
            }
        }
    }
}
