using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

namespace BingoGame.Network
{
    public class PlayerCountSelection : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Button exitButton;  // Changed from closeButton
        [SerializeField] private Button hostButton;  // Changed from nextButton
        [SerializeField] private List<Button> playerCountButtons = new List<Button>();
        [SerializeField] private LobbyUI lobbyUI;
        [SerializeField] private GameObject mainMenuPanel;

        private int selectedPlayerCount = -1;
        private bool isHelpMode = false;
        private BingoNetworkManager networkManager;

        private void Start()
        {
            networkManager = BingoNetworkManager.Instance;
            if (networkManager == null)
            {
                networkManager = FindObjectOfType<BingoNetworkManager>();
            }

            exitButton.onClick.AddListener(OnExitClicked);
            hostButton.onClick.AddListener(OnHostClicked);
            for (int i = 0; i < playerCountButtons.Count; i++)
            {
                int playerCount = i + 1; // 1-6
                Button button = playerCountButtons[i];
                button.onClick.AddListener(() => OnPlayerCountSelected(playerCount, button));

                Text buttonText = button.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = playerCount.ToString();
                }
            }

            // Disable host button initially
            hostButton.interactable = false;

            // Hide panel initially
            selectionPanel.SetActive(false);
        }

        public void Show()
        {
            isHelpMode = false;
            selectionPanel.SetActive(true);
            selectedPlayerCount = -1;
            hostButton.interactable = false;
        }

        public void ShowAsHelp()
        {
            isHelpMode = true;
            selectionPanel.SetActive(true);
            selectedPlayerCount = -1;

            // In help mode, hide host button
            hostButton.gameObject.SetActive(false);
        }

        private void OnPlayerCountSelected(int count, Button selectedButton)
        {
            selectedPlayerCount = count;
            // Enable host button
            if (!isHelpMode)
            {
                hostButton.interactable = true;
            }
        }

        private void OnHostClicked()
        {
            if (selectedPlayerCount <= 0) return;
            networkManager.RequiredPlayerCount = selectedPlayerCount;

            // Start host
            networkManager.StartHost();

            // Hide this panel and show lobby
            selectionPanel.SetActive(false);
            lobbyUI.Show(selectedPlayerCount);

            // Show main menu panel (it will be hidden behind lobby, but ready for return)
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }

        private void OnExitClicked()
        {
            selectionPanel.SetActive(false);

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            if (isHelpMode)
            {
                hostButton.gameObject.SetActive(true);
                isHelpMode = false;
            }
        }
    }
}
