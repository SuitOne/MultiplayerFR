using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MainR
{
    public class BuildingGhost : NetworkBehaviour
    {
        [SerializeField] protected BuildingObj buildingObj;

        [SerializeField] private LayerMask terrainLayer;

        [SerializeField] private float yOffset;

        protected Renderer ghostRenderer;
        protected BoxCollider boxCollider;
        private List<Collider> collisionObjects = new();

        public override void OnStartClient()
        {
            base.OnStartClient();
            InitializeGhost();
        }

        [Client]
        protected virtual void InitializeGhost()
        {
            // Component init
            if(TryGetComponent(out Renderer gR))
            {
                ghostRenderer = gR;
            }
            else { Debug.LogError("Failed to get Renderer on ghost initialization!"); }

            if (TryGetComponent(out BoxCollider bC))
            {
                boxCollider = bC;
            }
            else { Debug.LogError("Failed to get BoxCollider on ghost initialization!"); }

            PlayerManager.Instance.PlayerStateChanged += HandlePlayerStateChanged;

            // Initialize position and color before enabling
            FollowTerrain();
            CanPlace();
            ghostRenderer.enabled = true;
        }

        private void Update()
        {
            if (IsOwner)
            {
                FollowTerrain();
                HandleControls();
            }
        }

        private void HandleControls()
        {
            // Try place
            if (CanPlace() && Input.GetMouseButtonDown(0))
            {
                RpcTryPlaceGhost();
            }

            // Cancel placement
            if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                RpcDespawnGhost();
            }
        }

        [ServerRpc]
        private void RpcDespawnGhost()
        {
            Despawn(gameObject);
        }

        [ServerRpc]
        private void RpcTryPlaceGhost()
        {

            if (CanPlace())
            {
                TestSceneManager.Instance.ResourceManager.ChangeResources(-buildingObj.resourceCost);
                TestSceneManager.Instance.ResourceManager.ChangeManpower(-buildingObj.manpowerCost);

                GameObject go = Instantiate(buildingObj.prefab, transform.position, transform.rotation);

                Spawn(go, Owner);

                Despawn(gameObject);
            }
        }

        [Client]
        private void FollowTerrain()
        {
            // Move ghost over terrain
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                transform.position = hit.point + new Vector3(0, yOffset, 0);
            }
        }

        private void HandlePlayerStateChanged(PlayerManager.PlayerState state)
        {
            if(state != PlayerManager.PlayerState.Placement)
            {
                RpcDespawnGhost();
            }
        }

        protected bool CanPlace()
        {
            bool result = true;

            // Price check
            ResourceManager rManager = TestSceneManager.Instance.ResourceManager;

            if(rManager.GetManpower < buildingObj.manpowerCost || rManager.GetResources < buildingObj.resourceCost)
            {
                SetGhostColor(Color.red);
                result =  false;
            }

            // Collision
            if (result == true)
            {
                if (collisionObjects.Count > 0)
                {
                    result = false;
                }
            }

            SetGhostColor(result ? Color.green : Color.red);
            return result;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!collisionObjects.Contains(other))
            {
                collisionObjects.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (collisionObjects.Contains(other))
            {
                collisionObjects.Remove(other);
            }
        }

        [Client]
        private void SetGhostColor(Color color)
        {
            ghostRenderer.material.color = color;
        }
    }
}
