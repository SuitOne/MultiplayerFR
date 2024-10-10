using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Managing;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FishNet;
using FishNet.Transporting;

namespace MainR
{
    // TODO:
    // Create more modular user list
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField]
        private LobbyInterface lobbyInt;

        public override void OnStartClient()
        {
            base.OnStartClient();

            lobbyInt.InitializeLobby(IsHostInitialized);

            RpcRequestClientListUpdate();

            ClientManager.OnClientConnectionState += HandleClientConnectionStateChanged;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            LocalNetworkManager.Singleton.ClientListChanged += OnClientListChanged;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (LocalNetworkManager.Singleton != null)
            {
                LocalNetworkManager.Singleton.ClientListChanged -= OnClientListChanged;
            }
        }

        public void StartGame()
        {
            CmdStartGame();
        }

        /// <summary>
        /// Leaves the game
        /// </summary>
        public void QuitGame()
        {
            if (IsHostInitialized)
            {
                ServerManager.StopConnection(true);
            }
            else
            {
                ClientManager.StopConnection();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdStartGame(NetworkConnection conn = null)
        {
            if (!conn.IsHost) { return; }

            // Stop accepting new connections
            LocalNetworkManager.Singleton.acceptingConnections = false;

            // Kick uninitialized clients
            foreach (var client in InstanceFinder.ServerManager.Clients)
            {
                if(LocalNetworkManager.Singleton.GetClient(client.Key) == null)
                {
                    InstanceFinder.ServerManager.Kick(client.Value, FishNet.Managing.Server.KickReason.Unset);
                }
            }

            // Load new scene
            SceneLoadData sceneLoadData = new("Test")
            {
                ReplaceScenes = ReplaceOption.All
            };
            NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        }

        public void SetCommander(int index)
        {
            CmdSetCommander(index);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdSetCommander(int index, NetworkConnection conn = null)
        {
            if (!conn.IsHost) { return; }

            if(LocalNetworkManager.Singleton.GetClientList.Count <= index) { Debug.LogWarning("Attempted to set commander to invalid index"); return; }

            // Set new commander
            LocalNetworkManager.Singleton.SetClientRole(LocalNetworkManager.Singleton.GetClientList[index], LocalNetworkManager.LocalClient.PlayerRole.Commander);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RpcRequestClientListUpdate()
        {
            OnClientListChanged();
        }

        private void OnClientListChanged()
        {
            // Checks client list for changes and passes the changes to user list to clients
            List<LocalNetworkManager.LocalClient> clientList = LocalNetworkManager.Singleton.GetClientList;

            List<string> usernameList = new();
            int commanderIndex = -1;
            for(int i = 0; i < LocalNetworkManager.Singleton.GetMaxPlayers; i++)
            {
                if(clientList.Count <= i)
                {
                    // No users for this text spot
                    RpcUpdateUsername(i, "", Color.white);
                }
                else
                {
                    string username = clientList[i].username;
                    usernameList.Add(username);

                    Color color = clientList[i].role ==
                        LocalNetworkManager.LocalClient.PlayerRole.Commander ? Color.red : Color.white;

                    if (color == Color.red) { commanderIndex = i; }

                    RpcUpdateUsername(i, username, color);
                }
            }
            // Update host commander selection dropdown
            RpcUpdateCommanderDropdown(usernameList, commanderIndex);
        }

        private void HandleClientConnectionStateChanged(ClientConnectionStateArgs args)
        {
            // Return to main menu if the connection is lost to server
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }

        [ObserversRpc]
        private void RpcUpdateCommanderDropdown(List<string> usernameList, int commanderIndex)
        {
            if (!IsHostInitialized) { return; }
            lobbyInt.SetCommanderDropdown(usernameList, commanderIndex);
        }

        [ObserversRpc]
        private void RpcUpdateUsername(int index, string username, Color color)
        {
            lobbyInt.SetUsernameText(index, username, color);
        }
    }
}
