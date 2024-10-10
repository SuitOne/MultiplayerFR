using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MainR
{
    [RequireComponent(typeof( LobbyManager))]
    public class LobbyInterface : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private List<TMP_Text> usernameTexts;
        [SerializeField]
        private GameObject hostPanel;
        [SerializeField]
        private TMP_Dropdown commanderDropdown;

        private LobbyManager lobbyManager;

        private void Awake()
        {
            lobbyManager = GetComponent<LobbyManager>();
        }

        public void InitializeLobby(bool isHost = false)
        {
            // Activates host panel if host
            hostPanel.SetActive(isHost);
        }

        public void SetUsernameText(int index, string text, Color color)
        {
            if(index >= usernameTexts.Count) { Debug.LogError("Attempted to set username to invald index: " + index); return; }

            usernameTexts[index].text = text;
            usernameTexts[index].color = color;
        }

        public void SetCommanderDropdown(List<string> options, int selectedOption)
        {
            if(selectedOption >= options.Count) { Debug.LogError("Attempted to set commander dropdown to invalid index: " + selectedOption); return; }

            commanderDropdown.Hide();
            commanderDropdown.ClearOptions();
            commanderDropdown.AddOptions(options);
            commanderDropdown.value = selectedOption;
        }
    }
}
