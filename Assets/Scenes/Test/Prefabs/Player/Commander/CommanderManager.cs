using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class CommanderManager : PlayerManager
    {
        public CommanderUIManager UIManager { get; private set; }
        public CommanderController CommanderController { get; private set; }
        public CommanderPlacementController PlacementController { get; private set; }

        public override void InitializeClientside()
        {
            // Enable/disable clientside objects
            foreach (var pair in clientsidePairs)
            {
                pair.obj.SetActive(pair.clientEnable);
            }

            // Enable movement and assign reference
            if (TryGetComponent(out CommanderController cc))
            {
                cc.enabled = true;
                CommanderController = cc;
            }
            else { Debug.LogError("Failed to get commander controller!"); }

            // Enable ui manager and assign reference
            if (TryGetComponent(out CommanderUIManager cum))
            {
                cum.enabled = true;
                UIManager = cum;
            }
            else { Debug.LogError("Failed to get UI manager!"); }

            // Enable placement controller and assign reference
            if(TryGetComponent(out CommanderPlacementController cpc))
            {
                cpc.enabled = true;
                PlacementController = cpc;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsOwner) { return; }

            // Event callbacks
            TestSceneManager.Instance.ResourceManager.syncResources.OnChange += OnResourcesChanged;
            TestSceneManager.Instance.ResourceManager.syncManpower.OnChange += OnManpowerChanged;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (!IsOwner) { return; }

            // Event callbacks
            TestSceneManager.Instance.ResourceManager.syncResources.OnChange -= OnResourcesChanged;
            TestSceneManager.Instance.ResourceManager.syncManpower.OnChange -= OnManpowerChanged;
        }

        public override void SetPlayerState(PlayerState newState)
        {
            base.SetPlayerState(newState);

            // Cursor handling
            Cursor.lockState = newState == PlayerState.Paused ? CursorLockMode.None : CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void OnResourcesChanged(int prev, int next, bool asServer)
        {
            UIManager.SetResourcesText(next);
        }

        private void OnManpowerChanged(int prev, int next, bool asServer)
        {
            UIManager.SetManpowerText(next);
        }
    }
}
