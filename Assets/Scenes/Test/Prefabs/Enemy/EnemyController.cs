using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MainR
{
    public class EnemyController : NetworkBehaviour
    {
        public float updateTargetInterval = 0.5f; // How often to update the target position in seconds
        public float damage = 5f;
        private NavMeshAgent agent;
        private Transform curTarget;

        public override void OnStartServer()
        {
            base.OnStartServer();

            agent = GetComponent<NavMeshAgent>();
            InvokeRepeating(nameof(UpdateTargetPlayer), 0, updateTargetInterval);
        }

        void UpdateTargetPlayer()
        {
            // Find the nearest target
            List<GameObject> targets = new();
            targets.AddRange(GameObject.FindGameObjectsWithTag("Player"));
            targets.AddRange(GameObject.FindGameObjectsWithTag("Building"));

            float shortestDistance = Mathf.Infinity;
            GameObject nearestTarget = null;

            foreach (GameObject target in targets)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);
                if (distanceToPlayer < shortestDistance)
                {
                    shortestDistance = distanceToPlayer;
                    nearestTarget = target;
                }
            }

            if (nearestTarget != null)
            {
                if(shortestDistance < 5 && nearestTarget.TryGetComponent(out Health health))
                {
                    health.ChangeHealth(-damage);
                }

                curTarget = nearestTarget.transform;
                agent.SetDestination(curTarget.position);
            }
        }

        void Update()
        {
            // Continue moving toward the target player
            if (curTarget != null)
            {
                agent.SetDestination(curTarget.position);
            }
        }

        public void OnDeath()
        {
            // Consider switching to networked pooling
            Despawn();
        }
    }
}
