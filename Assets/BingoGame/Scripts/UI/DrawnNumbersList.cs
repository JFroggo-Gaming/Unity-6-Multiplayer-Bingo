using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

namespace BingoGame.Network
{
    // Displays list of drawn numbers
    public class DrawnNumbersList : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI numbersText;
        [SerializeField] private string prefix = "Drawn Numbers:\n";

        private List<int> displayedNumbers = new List<int>();

        private void Start()
        {
            InvokeRepeating(nameof(UpdateNumbersList), 0.5f, 0.5f);
        }

        private void UpdateNumbersList()
        {
            if (BingoManager.Instance == null)
            {
                return;
            }

            List<int> drawnNumbers = BingoManager.Instance.DrawnNumbers;

            // Check if list changed
            if (drawnNumbers.Count != displayedNumbers.Count)
            {
                displayedNumbers = new List<int>(drawnNumbers);
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (numbersText == null) return;

            StringBuilder sb = new StringBuilder();
            sb.Append(prefix);

            // Display numbers in a nice format
            for (int i = 0; i < displayedNumbers.Count; i++)
            {
                sb.Append(displayedNumbers[i]);

                // Add comma separator, but not after last number
                if (i < displayedNumbers.Count - 1)
                {
                    sb.Append(", ");
                }

                // Line break every 10 numbers for readability
                if ((i + 1) % 10 == 0 && i < displayedNumbers.Count - 1)
                {
                    sb.Append("\n");
                }
            }

            numbersText.text = sb.ToString();
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}
