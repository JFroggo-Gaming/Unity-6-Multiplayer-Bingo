using UnityEngine;
using TMPro;

namespace BingoGame.Network
{
    // Displays countdown until next number draw
    public class CountdownTimer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private string format = "Next draw in: {0:F1}s";

        private void Update()
        {
            if (BingoManager.Instance == null || timerText == null)
            {
                return;
            }

            float timeRemaining = BingoManager.Instance.TimeUntilNextDraw;

            if (timeRemaining > 0)
            {
                timerText.text = string.Format(format, timeRemaining);
            }
            else
            {
                timerText.text = "Drawing...";
            }

        }
    }
}
