using Edgegap.Editor.Api.Models.Results;
using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public abstract class PlayerManager : NetworkBehaviour
    {
        public enum PlayerState
        {
            Play,
            Paused,
            Placement
        }

        public static PlayerManager Instance;
        protected PlayerState state = PlayerState.Play;
        public PlayerState GetPlayerState => state;
        public event Action<PlayerState> PlayerStateChanged;

        /// <summary>
        /// Used to tell a client during initialization which objects to enable or disable
        /// </summary>
        [Serializable]
        protected struct ClientsidePair
        {
            public GameObject obj;
            public bool clientEnable;
        }

        [SerializeField]
        protected List<ClientsidePair> clientsidePairs;

        public void Awake()
        {
            if(Instance == null)
            {
                Debug.Log("Playermanager singleton established");
                Instance = this;
            }
            else
            {
                Debug.LogError("Multiple player managers are active!");
                enabled = false;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (Owner.IsLocalClient)
            {
                InitializeClientside();
            }

            // Subscribe to events
            ClientManager.OnClientConnectionState += HandleClientConnectionStateChanged;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            // Unsubscribe from events
            ClientManager.OnClientConnectionState += HandleClientConnectionStateChanged;
        }

        protected void HandleClientConnectionStateChanged(ClientConnectionStateArgs args)
        {
            // TODO: This needs to be moved off of the player and onto something more persistent
            if(args.ConnectionState == LocalConnectionState.Stopped)
            {
                QuitGame();
            }
        }

        /// <summary>
        /// Quits the game and shuts down the client/server managers, loads main menu
        /// </summary>
        public void QuitGame()
        {
            if (IsServerInitialized) { InstanceFinder.ServerManager.StopConnection(true); }
            InstanceFinder.ClientManager.StopConnection();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Called when object is started on the clientside
        /// </summary>
        public abstract void InitializeClientside();

        /// <summary>
        /// Sets the player state and invokes the PlayerStateChanged event
        /// </summary>
        public virtual void SetPlayerState(PlayerState newPlayerState)
        {
            state = newPlayerState;
            PlayerStateChanged.Invoke(newPlayerState);
        }
    }
}
