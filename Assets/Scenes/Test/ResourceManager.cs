using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class ResourceManager : NetworkBehaviour
    {
        public readonly SyncVar<int> syncResources = new();
        public readonly SyncVar<int> syncManpower = new();

        public int GetResources => syncResources.Value;
        public int GetManpower => syncManpower.Value;

        [SerializeField]
        private int startingResources;
        [SerializeField]
        private int startingManpower;

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Initialize resources
            syncResources.Value = startingResources;
            syncManpower.Value = startingManpower;

            // Start incrementing resources
            StartCoroutine(TestEnumerator());
        }

        [Server]
        public void ChangeResources(int change)
        {
            syncResources.Value += change;
        }

        [Server]
        public void ChangeManpower(int change) 
        {
            syncManpower.Value += change;
        }

        private IEnumerator TestEnumerator()
        {
            while (true)
            {
                ChangeResources(1);
                yield return new WaitForSeconds(2);
            }
        }
    }
}
