using FishNet;
using FishNet.Broadcast;
using FishNet.Component.Scenes;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainR
{
    [RequireComponent(typeof(NetworkManager))]
    public class LocalNetworkManager : MonoBehaviour
    {
        /// <summary>
        /// Client data container, should be added once client is locked in and removed when client is disconnected
        /// </summary>
        public class LocalClient
        {
            public enum PlayerRole
            {
                Commander,
                Officer
            }

            // Client ID, should be reflected by NetworkManager
            public int id;
            // Is this client the host
            public bool isHost;
            // Username picked by client
            public string username;
            // The client role, defaults to officer
            public PlayerRole role;

            public LocalClient(int id, string username, PlayerRole role = PlayerRole.Officer, bool isHost = false)
            {
                this.id = id;
                this.username = username;
                this.role = role;
                this.isHost = isHost;
            }
        }

        /// <summary>
        /// Data container to be sent to the server by joining clients
        /// </summary>
        public struct JoinBroadcast : IBroadcast
        {
            public string Username;

            public JoinBroadcast(string username)
            {
                Username = username;
            }
        }

        // Network managers
        private NetworkManager m_NetworkManager;
        public static LocalNetworkManager Singleton;

        // Config
        [Header("Config"), SerializeField]
        private string defaultScene;
        [SerializeField]
        private int maxPlayers;
        public int GetMaxPlayers => maxPlayers;

        // Private config
        [HideInInspector]
        public bool acceptingConnections = true;

        private void Awake()
        {
            // Establish singleton
            if (Singleton == null)
            {
                Singleton = this;
            }
            else { Destroy(this); }

            m_NetworkManager = GetComponent<NetworkManager>();
        }

        private void Start()
        {
            // Event handling
            if(m_NetworkManager != null)
            {
                // TODO: Consider moving Scene and Client handlers to be changed under server state handler
                m_NetworkManager.ServerManager.OnRemoteConnectionState += HandleClientConnectionStateChanged;
                m_NetworkManager.ServerManager.OnServerConnectionState += HandleServerStateChanged;
                m_NetworkManager.ServerManager.RegisterBroadcast<JoinBroadcast>(HandleJoinBroadcast);
            }
        }

        /// <summary>
        /// Called to completely reset the manager to a new state
        /// </summary>
        public void ResetNetworkManager()
        {
            clientList.Clear();
            acceptingConnections = true;
        }

        #region Client List
        // Client list
        private List<LocalClient> clientList = new();
        public List<LocalClient> GetClientList => clientList;

        // Events
        public event Action ClientListChanged; // Invoked when clients are added, removed or edited

        /// <summary>
        /// Add a client to the client list with a client id and username
        /// </summary>
        public void AddClient(int id, string username)
        {
            // TEMPORARY: Sets host as commander, other players as officer
            clientList.Add(new LocalClient(
                id, username, id == 0 ? LocalClient.PlayerRole.Commander : LocalClient.PlayerRole.Officer, id == 0));

            ClientListChanged?.Invoke();
        }

        /// <summary>
        /// Remove a client from the client list
        /// </summary>
        public void RemoveClient(LocalClient client)
        {
            // If the commander is being removed, make the host the commander
            if(client.role == LocalClient.PlayerRole.Commander)
            {
                SetClientRole(clientList[0], LocalClient.PlayerRole.Commander);
            }
            clientList.Remove(client);
            ClientListChanged?.Invoke();
        }

        /// <summary>
        /// Returns all clients that hold the specified role
        /// </summary>
        public List<LocalClient> GetClientsOfRole(LocalClient.PlayerRole role)
        {
            List<LocalClient> returnList = new();

            foreach(var client in clientList)
            {
                if(client.role == role)
                {
                    returnList.Add(client);
                }
            }

            return returnList;
        }
        
        /// <summary>
        /// Change a client's role
        /// </summary>
        public void SetClientRole(LocalClient client, LocalClient.PlayerRole role)
        {
            if(client.role == role) { return; }

            // Make sure there is only one commander
            if(role == LocalClient.PlayerRole.Commander)
            {
                foreach(var value in clientList)
                {
                    if(value.role == role)
                    {
                        value.role  = LocalClient.PlayerRole.Officer;
                    }
                }
            }

            client.role = role;
            ClientListChanged?.Invoke();
        }

        /// <summary>
        /// Returns a client that matches the provided ID, or null if none could be found
        /// </summary>
        public LocalClient GetClient(int id) {
            foreach(var client in clientList)
            {
                if(client.id == id) return client;
            }

            return null;
        }

        /// <summary>
        /// Returns true if provided username is allowed
        /// </summary>
        private bool ValidUsername(string username)
        {
            // Max length
            if (username.Length > 15) { return false; }

            // Remove whitespace, then check min length
            username.Trim();
            if (username.Length <= 0) { return false; }

            return true;
        }
        #endregion

        #region Handlers
        private void HandleClientConnectionStateChanged(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            // Kick clients joining while server is not in lobby or otherwise not taking connections
            if (!acceptingConnections)
            {
                Debug.Log("Connection dropped due to server not accepting connections, ID: " + conn.ClientId);
                m_NetworkManager.ServerManager.Kick(conn, FishNet.Managing.Server.KickReason.Unset);
            }

            // Remove client from the client list if client connection has been stopped
            if(args.ConnectionState == RemoteConnectionState.Stopped)
            {
                LocalClient clientObj = GetClient(conn.ClientId);
                if (clientObj != null)
                {
                    RemoveClient(clientObj);
                }
            }
        }

        private void HandleServerStateChanged (ServerConnectionStateArgs args)
        {
            // Reset the network manager when the server shuts down
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                ResetNetworkManager();
            }
        }

        private void HandleJoinBroadcast(NetworkConnection conn, JoinBroadcast jb, Channel channel)
        {
            // Kick clients trying to send join broadcast when connections are closed or client has unsatisfactory username
            if (!acceptingConnections || !ValidUsername(jb.Username)) { conn.Kick(FishNet.Managing.Server.KickReason.Unset); return; }

            Debug.Log("Recieved join broadcast from username: " + jb.Username);

            SceneLoadData sceneLoadData = new(defaultScene) { ReplaceScenes = ReplaceOption.All };
            InstanceFinder.SceneManager.LoadConnectionScenes(conn, sceneLoadData);

            // Add new client to list
            AddClient(conn.ClientId, jb.Username);
        }
        #endregion
    }
}
