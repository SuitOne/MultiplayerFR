using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class OfficerWeaponController : NetworkBehaviour
    {
        [SerializeField]
        private Camera playerCamera;
        [SerializeField]
        private float fireRate = 0.5f; // Time in seconds between shots
        [SerializeField]
        private int damage = 25;
        [SerializeField]
        private int range = 25;    // Maximum range of the raycast

        private bool canShoot = true;
        private PlayerManager playerManager;

        private void Start()
        {
            if(TryGetComponent(out PlayerManager pM))
            {
                playerManager = pM;
            }
            else
            {
                Debug.LogError("Could not get player manager!");
            }
        }

        void Update()
        {
            if(playerManager.GetPlayerState != PlayerManager.PlayerState.Play) { return; }

            // Check if it's time to fire and if the player clicked the left mouse button
            if (Input.GetButtonDown("Fire1") && canShoot)
            {
                if (IsServerInitialized)
                {
                    Shoot();
                }
                else
                {
                    StartCoroutine(WaitForShoot());
                    RpcShoot();
                }
            }
        }

        [ServerRpc]
        void RpcShoot()
        {
            Shoot();
        }

        private void Shoot()
        {
            if (canShoot)
            {
                StartCoroutine(WaitForShoot());
                // Create a ray from the center of the screen
                Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                // Perform the raycast
                if (Physics.Raycast(ray, out RaycastHit hit, range))
                {
                    if (hit.collider.TryGetComponent(out Health health))
                    {
                        health.ChangeHealth(-damage);
                    }
                }
            }
        }

        private IEnumerator WaitForShoot()
        {
            if (!canShoot) { yield return null; }

            canShoot = false;
            yield return new WaitForSeconds(fireRate);
            canShoot = true;
        }
    }
}
