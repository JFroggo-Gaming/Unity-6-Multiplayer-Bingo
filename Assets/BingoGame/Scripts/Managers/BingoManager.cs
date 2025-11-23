using Mirror;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BingoGame.Network
{
    /// <summary>
    /// Global Bingo game manager - handles number drawing and game flow
    /// Runs on server only
    /// </summary>
    public class BingoManager : NetworkBehaviour
    {
        public static BingoManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private float drawInterval = 2f; // Time between number draws
        [SerializeField] private int minNumber = 1;
        [SerializeField] private int maxNumber = 75; // Standard Bingo uses 75
        [SerializeField] private int maxDraws = 25; // Maximum numbers to draw

        [Header("Pattern Settings")]
        [SerializeField] private BingoPattern[] availablePatterns;

        // SyncVar - automatically synchronized to all clients
        [SyncVar(hook = nameof(OnCurrentPatternChanged))]
        private int currentPatternIndex = -1;

        // NOT a SyncVar - we sync this manually via RPC for better control
        private float nextDrawTime = 0f;

        // SyncList - list that automatically syncs to all clients
        private readonly SyncList<int> drawnNumbers = new SyncList<int>();

        private List<int> availableNumbers = new List<int>();
        private Coroutine drawCoroutine;
        private bool gameStarted = false;

        public List<int> DrawnNumbers => new List<int>(drawnNumbers);
        public BingoPattern CurrentPattern => currentPatternIndex >= 0 && currentPatternIndex < availablePatterns.Length
            ? availablePatterns[currentPatternIndex]
            : null;
        public float TimeUntilNextDraw => Mathf.Max(0, nextDrawTime - Time.time);
        public int MinNumber => minNumber;
        public int MaxNumber => maxNumber;

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
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Initialize available numbers pool
            InitializeNumberPool();

            // Select random pattern
            SelectRandomPattern();

            // Start drawing numbers after a short delay
            Invoke(nameof(StartGame), 3f);
        }

        private void InitializeNumberPool()
        {
            availableNumbers.Clear();
            for (int i = minNumber; i <= maxNumber; i++)
            {
                availableNumbers.Add(i);
            }
        }

        private void SelectRandomPattern()
        {
            if (availablePatterns == null || availablePatterns.Length == 0)
            {
                return;
            }

            currentPatternIndex = Random.Range(0, availablePatterns.Length);
        }

        [Server]
        private void StartGame()
        {
            if (gameStarted)
            {
                return;
            }

            gameStarted = true;

            // Start drawing numbers
            drawCoroutine = StartCoroutine(DrawNumbersRoutine());
        }

        private IEnumerator DrawNumbersRoutine()
        {
            while (drawnNumbers.Count < maxDraws && availableNumbers.Count > 0)
            {
                // Tell ALL clients (including server-as-client) when the next draw will happen
                // This RPC will set nextDrawTime on everyone at the same moment
                RpcSyncTimer(drawInterval);

                // Wait for the interval
                yield return new WaitForSeconds(drawInterval);

                DrawNumber();
            }

            RpcSyncTimer(0f); // Tell everyone timer is finished
            RpcAnnounceGameEnd();
        }

        [Server]
        private void DrawNumber()
        {
            if (availableNumbers.Count == 0)
            {
                Debug.LogWarning("[BingoManager] No more numbers to draw!");
                return;
            }

            int randomIndex = Random.Range(0, availableNumbers.Count);
            int drawnNumber = availableNumbers[randomIndex];
            availableNumbers.RemoveAt(randomIndex);

            drawnNumbers.Add(drawnNumber);

            // Notify all clients about the drawn number
            RpcNumberDrawn(drawnNumber);
        }

        // Called by player when they think they have Bingo
        [Command(requiresAuthority = false)]
        public void CmdCheckBingo(int playerIndex, bool[] markedCells, NetworkConnectionToClient sender = null)
        {
            Debug.Log($"Checking Bingo for player {playerIndex}");

            // Verify the pattern matches
            if (CurrentPattern == null)
            {
                Debug.LogError("No pattern selected!");
                return;
            }

            bool hasValidBingo = CurrentPattern.CheckPattern(markedCells);

            if (hasValidBingo)
            {
                RpcAnnounceWinner(playerIndex);

                if (drawCoroutine != null)
                {
                    StopCoroutine(drawCoroutine);
                }
            }
            else
            {
                TargetInvalidBingo(sender);
            }
        }

        [ClientRpc]
        private void RpcNumberDrawn(int number)
        {
            // Number drawn notification (if needed for audio/visual feedback)
        }

        [ClientRpc]
        private void RpcAnnounceGameEnd()
        {
            Debug.Log("Game ended - all numbers drawn");
        }

        [TargetRpc]
        private void TargetInvalidBingo(NetworkConnection target)
        {
            Debug.LogWarning("Invalid Bingo claim - pattern does not match!");
        }

        private void OnCurrentPatternChanged(int oldIndex, int newIndex)
        {
            // Called when pattern changes - update UI if needed
            Debug.Log($"Pattern changed from {oldIndex} to {newIndex}");
        }

        [ClientRpc]
        private void RpcSyncTimer(float timeUntilNext)
        {
            // IMPORTANT: Server AND clients both receive this RPC
            // Each sets their own timer based on their own Time.time
            nextDrawTime = Time.time + timeUntilNext;
        }

        [ClientRpc]
        private void RpcAnnounceWinner(int winnerPlayerIndex)
        {
            // Find all GamePlayer instances and show appropriate UI
            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();
            foreach (GamePlayer player in allPlayers)
            {
                if (player.isLocalPlayer)
                {
                    if (player.playerIndex == winnerPlayerIndex)
                    {
                        player.ShowWinPanel();
                    }
                    else
                    {
                        player.ShowLosePanel();
                    }
                }
            }
        }

        [Server]
        public void ResetGame()
        {

            // Stop current draw coroutine if running
            if (drawCoroutine != null)
            {
                StopCoroutine(drawCoroutine);
                drawCoroutine = null;
            }

            drawnNumbers.Clear();

            InitializeNumberPool();

            SelectRandomPattern();

            gameStarted = false;

            RpcSyncTimer(0f);

            RpcResetGameUI();

            Invoke(nameof(StartGame), 3f);
        }

        [ClientRpc]
        private void RpcResetGameUI()
        {
            Debug.Log("Client resetting UI");

            // Hide win/lose panels and regenerate cards for all local players
            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();

            if (allPlayers == null || allPlayers.Length == 0)
            {
                Debug.LogWarning("No GamePlayer instances found during reset!");
                return;
            }

            foreach (GamePlayer player in allPlayers)
            {
                if (player == null)
                {
                    Debug.LogWarning("[Null player in array");
                    continue;
                }

                if (player.isLocalPlayer)
                {
                    // Hide win/lose panels
                    player.HidePanels();

                    // Re-initialize bingo card with new numbers
                    BingoCard bingoCard = player.GetComponentInChildren<BingoCard>();
                    if (bingoCard != null)
                    {
                        bingoCard.Initialize(player);
                    }
                    else
                    {
                        Debug.LogWarning("BingoCard component not found during reset");
                    }

                    // Re-initialize pattern display
                    PatternDisplay patternDisplay = player.GetComponentInChildren<PatternDisplay>();
                    if (patternDisplay != null)
                    {
                        patternDisplay.UpdatePatternDisplay();
                    }
                    else
                    {
                        Debug.LogWarning("atternDisplay component not found during reset");
                    }
                }
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
