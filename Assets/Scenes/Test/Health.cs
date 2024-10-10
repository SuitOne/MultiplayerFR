using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MainR
{
    public class Health : NetworkBehaviour
    {
        // TODO:
        // Implement syncvars when required
        [SerializeField]
        private int maxHealth = 100;
        [SerializeField]
        private int startHealth = 100;
        private float health;

        [SerializeField]
        private UnityEvent onDeath;
        [SerializeField]
        private UnityEvent<float, float> onHealthChanged;

        public override void OnStartServer()
        {
            base.OnStartServer();

            health = startHealth;
        }

        [Server]
        public void ChangeHealth(float change)
        {
            health = Mathf.Clamp(health + change, 0, maxHealth);

            // On health change is invoked with old health, new health args
            onHealthChanged.Invoke(health - change, health);

            if(health <= 0)
            {
                onDeath.Invoke();
            }
        }
    }
}
