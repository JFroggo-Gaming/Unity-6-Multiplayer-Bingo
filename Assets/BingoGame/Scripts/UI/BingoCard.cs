using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BingoGame.Network
{
    // Individual player's Bingo card with unique numbers
    public class BingoCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform buttonsParent;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color markedColor = Color.red;

        private Button[] cardButtons = new Button[24];
        private int[] cardNumbers = new int[24];
        private bool[] markedCells = new bool[24];
        private GamePlayer ownerPlayer;

        private void Awake()
        {
            if (buttonsParent == null)
            {
                Debug.LogError("Buttons parent not assigned");
                return;
            }

            // Get existing buttons from parent
            GetButtonsFromParent();
        }

        public void Initialize(GamePlayer player)
        {
            ownerPlayer = player;

            // Reset marked cells
            for (int i = 0; i < 24; i++)
            {
                markedCells[i] = false;
                UpdateCellVisual(i);
            }

            GenerateCardNumbers();

            // Subscribe to BingoManager events (cancel any previous)
            CancelInvoke(nameof(CheckForDrawnNumbers));
            if (BingoManager.Instance != null)
            {
                // Listen for drawn numbers
                InvokeRepeating(nameof(CheckForDrawnNumbers), 0.5f, 0.5f);
            }
        }

        private void GetButtonsFromParent()
        {
            Button[] allButtons = buttonsParent.GetComponentsInChildren<Button>();
            List<Button> childButtons = new List<Button>();

            foreach (Button btn in allButtons)
            {
                if (btn.transform != buttonsParent)
                {
                    childButtons.Add(btn);
                }
            }

            Button[] buttons = childButtons.ToArray();

            if (buttons.Length != 24)
            {
                Debug.LogError($"Expected 24 buttons found {buttons.Length}");
                return;
            }

            for (int i = 0; i < 24; i++)
            {
                cardButtons[i] = buttons[i];

                int index = i;
                cardButtons[i].onClick.AddListener(() => OnCellClicked(index));

                cardButtons[i].interactable = true;

                UnityEngine.UI.Image buttonImage = cardButtons[i].GetComponent<UnityEngine.UI.Image>();
                if (buttonImage == null)
                {
                    Debug.LogWarning($"[BingoCard] Button {i} has no Image component");
                    buttonImage = cardButtons[i].gameObject.AddComponent<UnityEngine.UI.Image>();
                }

                // CRITICAL: Image must have raycastTarget = true to receive clicks!
                buttonImage.raycastTarget = true;
            }
        
        }

        private void GenerateCardNumbers()
        {
            if (cardButtons == null || cardButtons.Length != 24)
            {
                Debug.LogError("Cannot generate card numbers - buttons not initialized");
                return;
            }

            if (cardNumbers == null || cardNumbers.Length != 24)
            {
                Debug.LogError("Cannot generate card numbers - cardNumbers array invalid");
                return;
            }

            // Get number range from BingoManager
            int minNum = 1;
            int maxNum = 75;

            if (BingoManager.Instance != null)
            {
                minNum = BingoManager.Instance.MinNumber;
                maxNum = BingoManager.Instance.MaxNumber;
            }
            else
            {
                Debug.LogWarning("[BingoManager not found");
            }

            List<int> availableNumbers = new List<int>();
            for (int i = minNum; i <= maxNum; i++)
            {
                availableNumbers.Add(i);
            }

            Debug.Log($"[BingoCard] Generating numbers from {minNum} to {maxNum}, need 24 unique numbers");

            // Pick 24 random numbers
            for (int i = 0; i < 24; i++)
            {
                if (cardButtons[i] == null)
                {
                    Debug.LogWarning($"[BingoCard] Button at index {i} is null, skipping...");
                    continue;
                }

                int randomIndex = Random.Range(0, availableNumbers.Count);
                int number = availableNumbers[randomIndex];
                availableNumbers.RemoveAt(randomIndex);

                cardNumbers[i] = number;

                // Update button text
                TextMeshProUGUI text = cardButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = number.ToString();
                }
                else
                {
                    Debug.LogWarning($"Button has no TextMeshProUGUI component");
                }
            }
        }

        private void OnCellClicked(int index)
        {


            // Validate index
            if (index < 0 || index >= 24)
            {
                Debug.LogError($"Invalid cell index: {index}");
                return;
            }

            // Validate arrays
            if (markedCells == null || cardNumbers == null || cardButtons == null)
            {
                Debug.LogError("Card arrays not initialized");
                return;
            }

            if (markedCells[index])
            {
                return;
            }

            int number = cardNumbers[index];
            // Check if this number has been drawn
            if (BingoManager.Instance == null)
            {
                Debug.LogError("BingoManager not found");
                return;
            }

            List<int> drawnNumbers = BingoManager.Instance.DrawnNumbers;
            if (drawnNumbers == null)
            {
                return;
            }


            if (!drawnNumbers.Contains(number))
            {
                return;
            }

            markedCells[index] = true;
            UpdateCellVisual(index);
            // Check for Bingo
            CheckForBingo();
        }

        private void UpdateCellVisual(int index)
        {
            if (index < 0 || index >= 24)
            {
                Debug.LogError($"Invalid index in UpdateCellVisual:{index}");
                return;
            }

            if (cardButtons == null || cardButtons[index] == null)
            {
                Debug.LogWarning($"Button at index {index} is null");
                return;
            }

            Image buttonImage = cardButtons[index].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = markedCells[index] ? markedColor : normalColor;
            }
            else
            {
                Debug.LogWarning($"[BingoCard] Button at index {index} has no Image component!");
            }
        }

        private void CheckForDrawnNumbers()
        {
            
        }

        private void CheckForBingo()
        {
            if (BingoManager.Instance == null)
            {
                return;
            }

            if (ownerPlayer == null)
            {
                return;
            }

            if (markedCells == null || markedCells.Length != 24)
            {
                return;
            }

            // Send to server to verify
            BingoManager.Instance.CmdCheckBingo(ownerPlayer.playerIndex, markedCells);
        }

        public int[] GetCardNumbers()
        {
            return cardNumbers;
        }

        public bool[] GetMarkedCells()
        {
            return markedCells;
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
