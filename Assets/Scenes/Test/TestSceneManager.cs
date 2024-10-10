using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class TestSceneManager : NetworkBehaviour
    {
        [SerializeField]
        private GameObject officerPrefab;
        [SerializeField]
        private GameObject commanderPrefab;
        [SerializeField]
        private GameObject playerSpawn;

        public ResourceManager ResourceManager { get; private set; }

        public static TestSceneManager Instance {  get; private set; }

        private void Awake()
        {
            // Singleton
            if (Instance == null) 
                { Instance = this; Debug.Log("Scene Manager instance established."); }
                else { Debug.LogWarning("Two scene managers detected!"); Destroy(this); }

            // Other references
            ResourceManager = GetComponent<ResourceManager>();
        }

        #region Callbacks
        public override void OnStartServer()
        {
            base.OnStartServer();

            // Callbacks
            SceneManager.OnClientPresenceChangeEnd += OnClientConnected;

            // Initialize scene director
            if(TryGetComponent(out TestSceneDirector dir))
            {
                dir.BeginEnemySpawning();
            }
            else { Debug.LogError("Could not contact scene director!"); }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            // Callbacks
            SceneManager.OnClientPresenceChangeEnd -= OnClientConnected;

        }

        private void OnClientConnected(ClientPresenceChangeEventArgs args)
        {
            NetworkConnection conn = args.Connection;

            // Spawn a player for a client who has loaded the scene
            if (args.Scene == SceneManager.GetScene("Test") && args.Added)
            {
                SpawnPlayer(LocalNetworkManager.Singleton.GetClient(conn.ClientId), playerSpawn.transform);
            }
        }
        #endregion

        /// <summary>
        /// Spawn a client at a spawn location, uses default location when not overloaded
        /// </summary>
        public void SpawnPlayer(LocalNetworkManager.LocalClient client, Transform spawnLocation = null)
        {
            // Default spawn location
            if (spawnLocation == null) { spawnLocation = playerSpawn.transform; }

            // Spawn prefab based off role
            GameObject go =
                Instantiate(client.role == LocalNetworkManager.LocalClient.PlayerRole.Officer ? officerPrefab : commanderPrefab, spawnLocation.position, spawnLocation.rotation);

            if (client.id == -1)
            {
                // Host
                InstanceFinder.ServerManager.Spawn(go);
            }
            else
            {
                // Client
                InstanceFinder.ServerManager.Spawn(go, NetworkManager.ServerManager.Clients[client.id]);
            }
        }
    }
}
