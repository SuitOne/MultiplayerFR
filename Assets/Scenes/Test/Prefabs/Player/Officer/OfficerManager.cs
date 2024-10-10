using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class OfficerManager : PlayerManager
    {
        public OfficerUIManager UIManager { get; private set; }
        public OfficerController OfficerController { get; private set; }
        public OfficerWeaponController WeaponController { get; private set; }

        public override void InitializeClientside()
        {
            // Enable/disable clientside objects
            foreach(var pair in clientsidePairs)
            {
                pair.obj.SetActive(pair.clientEnable);
            }

            // Enable movement and assign reference
            if (TryGetComponent(out OfficerController oc))
            {
                oc.enabled = true;
                OfficerController = oc;
            }
            else { Debug.LogError("Failed to get officer controller!"); }

            // Enable weapon controller and assign reference
            if (TryGetComponent(out OfficerWeaponController owc))
            {
                owc.enabled = true;
                WeaponController = owc;
            }
            else { Debug.LogError("Failed to get weapon controller!"); }

            // Enable ui manager and assign reference
            if (TryGetComponent(out OfficerUIManager oum))
            {
                oum.enabled = true;
                UIManager = oum;
            }
            else { Debug.LogError("Failed to get UI manager!"); }
        }

        public override void SetPlayerState(PlayerState newState)
        {
            base.SetPlayerState(newState);

            // Cursor handling
            Cursor.lockState = newState == PlayerState.Paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = newState == PlayerState.Paused;
        }

        [Server]
        public void OnHealthChanged(float oldHealth, float newHealth)
        {
            HealthChanged(Owner, oldHealth, newHealth);
        }

        [TargetRpc]
        private void HealthChanged(NetworkConnection conn, float oldHealth, float newHealth)
        {
            UIManager.SetHealthFillAmount(newHealth / 100f);
        }

        [Server]
        public void OnDeath()
        {
            Despawn();

            LocalNetworkManager.LocalClient client;

            // Get client
            client = LocalNetworkManager.Singleton.GetClient(OwnerId);

            TestSceneManager.Instance.SpawnPlayer(client);
        }
    }
}
