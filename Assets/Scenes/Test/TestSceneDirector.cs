using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    public class TestSceneDirector : NetworkBehaviour
    {
        [SerializeField]
        private List<Transform> enemySpawns = new();
        [SerializeField]
        private GameObject enemyPrefab;

        public void BeginEnemySpawning()
        {
            if (!IsServerInitialized) { return; }

            InvokeRepeating(nameof(SpawnEnemy), 5, 3);
        }

        private void SpawnEnemy()
        {
            // Instantiates an enemy at a random spawn
            GameObject newEnemy = Instantiate(enemyPrefab);
            newEnemy.transform.position = enemySpawns[Random.Range(0, enemySpawns.Count - 1)].position;
            InstanceFinder.ServerManager.Spawn(newEnemy);
        }
    }
}
