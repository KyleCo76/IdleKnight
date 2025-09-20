using Game;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Controller))]
    public class PooledMinion : MonoBehaviour, IPooledResettable
    {
        [ShowInInspector]
        private MinionSpawner parentSpawner;
        [ShowInInspector]
        public GameObject SourcePrefab { get; private set; }
        
        public void OnReturnedToPool()
        {
            
        }
    
        public void OnTakenFromPool(GameObject _instance, MinionSpawner _spawner)
        {
            if (!TryGetComponent(out Controller enemyController)) {
                Debug.LogError("No Controller component found on " + gameObject.name + ".");
                return;
            }
            
            parentSpawner = _spawner;
            SourcePrefab = _instance;
            enemyController.SetPooledMember();
            enemyController.ResetHealth();
        }

        public void ReturnToPool()
        {
            parentSpawner.ReleaseMinion(gameObject);
        }
    }
}
