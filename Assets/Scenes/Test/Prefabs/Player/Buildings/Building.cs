using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class Building : NetworkBehaviour
    {
        protected Health health;

        [SerializeField] protected BuildingObj buildingObj;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            InitializeBuilding();
        }

        /// <summary>
        /// Called on spawn, contains code that needs to be run before the building is in play
        /// </summary>
        protected virtual void InitializeBuilding()
        {
            // Init health
            if(TryGetComponent(out Health h))
            {
                health = h;
                // Holding off on health init until I swap over to interfaces
            }
            else
            {
                Debug.LogError("Building failed to find Health during initialization!");
            }
        }

        [ServerRpc]
        public void RpcDestroyBuilding()
        {
            Despawn(gameObject);
        }
    }
}
