using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class CommanderPingController : NetworkBehaviour
    {
        [SerializeField]
        private int existenceTime;

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(PingWait());
        }

        [Server]
        private IEnumerator PingWait()
        {
            yield return new WaitForSeconds(existenceTime);

            Despawn(gameObject);
        }
    }
}
