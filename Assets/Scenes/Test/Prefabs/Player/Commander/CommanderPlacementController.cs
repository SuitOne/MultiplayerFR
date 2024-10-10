using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class CommanderPlacementController : NetworkBehaviour
    {
        [SerializeField] private GameObject pingPrefab;
        [SerializeField] private GameObject buildingPrefab;

        private CommanderManager commanderManager;

        private GameObject curGhost = null;

        private void Awake()
        {
            commanderManager = GetComponent<CommanderManager>();
        }

        public void PlaceGhost(Vector3 spot)
        {
            if (curGhost == null)
            {
                RpcPlaceBuildingGhost(spot);
            }
        }

        [ServerRpc]
        public void RpcPing(Vector3 pingSpot)
        {
            if (pingSpot == null)
            {
                Debug.LogWarning("RpcPing called with null argument!");
                return;
            }

            GameObject pingObj = Instantiate(pingPrefab, pingSpot, Quaternion.identity);

            Spawn(pingObj);
        }

        [ServerRpc]
        private void RpcPlaceBuildingGhost(Vector3 buildingSpot)
        {
            commanderManager.SetPlayerState(PlayerManager.PlayerState.Placement);

            GameObject buildingObj = Instantiate(buildingPrefab, buildingSpot, Quaternion.identity);

            Spawn(buildingObj, Owner);
            RpcGhostPlaced(Owner, buildingObj);
        }

        [TargetRpc]
        public void RpcGhostPlaced(NetworkConnection conn, GameObject ghost)
        {
            curGhost = ghost;
        }
    }
}
