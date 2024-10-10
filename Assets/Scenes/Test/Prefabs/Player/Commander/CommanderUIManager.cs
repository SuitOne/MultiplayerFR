using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static MainR.PlayerManager;

namespace MainR
{
    public class CommanderUIManager : UIManager
    {
        private CommanderManager commanderManager;

        [Header("UI Components")]
        [SerializeField]
        private TMP_Text resourcesText;
        [SerializeField]
        private TMP_Text manpowerText;

        private void Awake()
        {
            commanderManager = GetComponent<CommanderManager>();
        }

        private void Start()
        {
            commanderManager.PlayerStateChanged += OnPlayerStateChanged;
        }

        private void OnPlayerStateChanged(PlayerState state)
        {
            pauseObj.SetActive(state == PlayerState.Paused);
        }

        public override void PauseReturn()
        {
            commanderManager.SetPlayerState(PlayerState.Play);
        }

        public override void PauseQuit()
        {
            commanderManager.QuitGame();
        }

        public void SetResourcesText(int resources)
        {
            resourcesText.text = "Resources: " + resources;
        }

        public void SetManpowerText(int manpower)
        {
            manpowerText.text = "Manpower: " + manpower;
        }
    }
}
