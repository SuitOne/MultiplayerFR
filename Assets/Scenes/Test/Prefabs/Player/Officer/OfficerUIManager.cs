using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MainR.PlayerManager;

namespace MainR
{
    public class OfficerUIManager : UIManager
    {
        private OfficerManager officerManager;

        [SerializeField]
        private GameObject crosshair;
        [SerializeField]
        private Image healthbar;

        private void Awake()
        {
            officerManager = GetComponent<OfficerManager>();
        }

        private void Start()
        {
            officerManager.PlayerStateChanged += OnPlayerStateChanged;
        }

        private void OnPlayerStateChanged(PlayerState state)
        {
            pauseObj.SetActive(state == PlayerState.Paused);
            crosshair.SetActive(state == PlayerState.Play);
        }

        public void SetHealthFillAmount(float amount)
        {
            amount = Mathf.Clamp(amount, 0f, 1f);

            healthbar.fillAmount = amount;
        }

        public override void PauseReturn()
        {
            officerManager.SetPlayerState(PlayerState.Play);
        }

        public override void PauseQuit()
        {
            officerManager.QuitGame();
        }
    }
}
