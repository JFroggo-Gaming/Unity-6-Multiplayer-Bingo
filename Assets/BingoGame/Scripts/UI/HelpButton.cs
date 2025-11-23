using UnityEngine;
using UnityEngine.UI;

namespace BingoGame.Network
{
    // Zupełnie nie sus skrypt
    public class HelpButton : MonoBehaviour
    {
        [SerializeField] private Button helpButton;
        [SerializeField] private string url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=RDdQw4w9WgXcQ&start_radio=1";

        private void Start()
        {
            if (helpButton != null)
            {
                helpButton.onClick.AddListener(OpenURL);
            }
        }

        private void OpenURL()
        {
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }

        private void OnDestroy()
        {
            if (helpButton != null)
            {
                helpButton.onClick.RemoveListener(OpenURL);
            }
        }
    }
}
