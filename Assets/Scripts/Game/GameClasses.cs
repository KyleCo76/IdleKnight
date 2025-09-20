using System.Collections.Generic;
using Enemies;
using UnityEngine;

namespace Game
{
    public class PowerUpData
    {
        public PowerUpType Type { get; private set; }
        public float Duration { get; private set; }
        public float Multiplier { get; private set; }
        public float Amount { get; private set; }
        public PowerUpData(PowerUpType _type, float _duration = 0f, float _multiplier = 1f, float _amount = 0f)
        {
            Type = _type;
            Duration = _duration;
            Multiplier = _multiplier;
            Amount = _amount;
        }
    }

    public enum AttackType
    {
        None,
        PlayerAttack,
        Environment,
        Other
    }

    public enum PowerUpType
    {
        None,
        Invincibility,
        DoublePoints,
        CoinMagnet,
        AttackSpeedBoost,
        RangedDamageBoost,
        MeleeDamageBoost,
        HealthRegenTickRate,
        HealthRegenAmount,
        MaxHealthBoost,
        HealAmount,
        ManaRegenTickRate,
        ManaRegenAmount,
        ManaBoost,
        MaxManaBoost,
        SpeedBoost,
        AuraTickSpeedBoost,
        AuraRangeBoost,
        AuraDamageBoost,
        SuperCooldownReduction,
        SuperDamageBoost
    }

    public enum SuperType
    {
        None,
        BlobLarge,
        BlobSmall,
        ElectricLarge,
        ElectricSmall,
        EnergyLarge,
        EnergySmall,
        FireBallLarge,
        FireBallSmall,
        LaserLarge,
        LaserSmall,
        MagicMissileLarge,
        MagicMissileSmall,
        RockLarge,
        RockSmall,
        SlashLarge,
        SlashSmall,
    }

    // Optional hook for components that need resetting between uses
    public interface IPooledResettable
    {
        void OnTakenFromPool(GameObject _instance, MinionSpawner _spawner);
        void OnReturnedToPool();
    }
    
    public struct TrackedMinion : System.IEquatable<TrackedMinion>
    {
        public GameObject Minion;
        public float TimeToLive;

        public bool Equals(TrackedMinion _other)
        {
            return ReferenceEquals(Minion, _other.Minion);
        }

        public override bool Equals(object _obj)
        {
            return _obj is TrackedMinion other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked {
                int hash = 17;
                hash = hash * 31 + (Minion ? Minion.GetHashCode() : 0);
                hash = hash * 31 + TimeToLive.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(TrackedMinion _left, TrackedMinion _right) => _left.Equals(_right);
        public static bool operator !=(TrackedMinion _left, TrackedMinion _right) => !_left.Equals(_right);
    }

    public sealed class PooledMinionManager
    {
        private readonly Transform poolRoot;
        private readonly int initialSize;
        private readonly int globalMaxPrefabAmount;
        private readonly Dictionary<GameObject, int> maxSizePerPrefab;
        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
        private readonly Dictionary<GameObject, int> liveCounts = new();
        
        public PooledMinionManager(int _initialPoolSize, int _maxSizePerPrefab)
        {
            this.initialSize = Mathf.Max(0, _initialPoolSize);
            this.globalMaxPrefabAmount = Mathf.Max(0, _maxSizePerPrefab);

            var rootGo = GameObject.Find("Enemies");
            if (!rootGo) {
                rootGo = new GameObject("Enemies");
            }
            poolRoot = rootGo.transform;
        }

        public PooledMinionManager(int _initialPoolSize)
        {
            this.initialSize = Mathf.Max(0, _initialPoolSize);

            var rootGo = GameObject.Find("Enemies");
            if (!rootGo) {
                rootGo = new GameObject("Enemies");
            }
            poolRoot = rootGo.transform;
        }

        private void EnsurePool(GameObject _prefab)
        {
            if (!pools.ContainsKey(_prefab)) {
                pools[_prefab] = new Queue<GameObject>(initialSize);
                liveCounts[_prefab] = 0;
            }
        }

        public GameObject GetFromPool(GameObject _prefab, Vector3 _position, Quaternion _rotation)
        {
            EnsurePool(_prefab);
            var queue = pools[_prefab];

            GameObject go;
            if (queue.Count > 0) {
                go = queue.Dequeue();
            } else {
                if (!HasSpawnsAvailable(_prefab)) {
                    return null;
                }
                // if (globalMaxPrefabAmount > 0 && GetLiveCount(_prefab) >= globalMaxPrefabAmount) {
                //     return null; // Ignore spawn if we reached the max number of prefabs
                // }
                go = Object.Instantiate(_prefab, poolRoot);
            }

            go.transform.SetPositionAndRotation(_position, _rotation);
            go.SetActive(true);
            
            liveCounts[_prefab]++;
            return go;
        }

        private int GetLiveCount(GameObject _prefab)
        {
            return liveCounts.GetValueOrDefault(_prefab);
        }

        private bool HasSpawnsAvailable(GameObject _prefab)
        {
            if (maxSizePerPrefab.TryGetValue(_prefab, out var maxSize)) {
                return maxSize == 0 || GetLiveCount(_prefab) < maxSize;
            } else {
                return globalMaxPrefabAmount == 0 || GetLiveCount(_prefab) < globalMaxPrefabAmount;
            }
        }
        
        public void PreWarm(GameObject _prefab)
        {
            if (_prefab == null)
                return;

            EnsurePool(_prefab);
            var queue = pools[_prefab];
            while (queue.Count < initialSize) {
                var go = Object.Instantiate(_prefab, poolRoot);
                go.SetActive(false);
                queue.Enqueue(go);
            }
        }
        
        public void PreWarm(GameObject _prefab, int _initialAmount, int _maxPrefabAmount)
        {
            if (_prefab == null)
                return;

            EnsurePool(_prefab);
            var queue = pools[_prefab];
            maxSizePerPrefab[_prefab] = _maxPrefabAmount;
            while (queue.Count < _initialAmount) {
                var go = Object.Instantiate(_prefab, poolRoot);
                go.SetActive(false);
                queue.Enqueue(go);
            }
        }
        

        public void Release(GameObject _instance)
        {
            if (!_instance)
                return;
            
            var marker = _instance.GetComponent<PooledMinion>();
            if (!marker || !marker.SourcePrefab) {
                _instance.SetActive(false);
                _instance.transform.SetParent(poolRoot, false);
                return;
            }
            
            var prefab = marker.SourcePrefab;
            EnsurePool(prefab);
            
            // Call Reset Hook
            var hooks = _instance.GetComponents<IPooledResettable>();
            foreach (var hook in hooks) {
                hook.OnReturnedToPool();
            }
            
            _instance.SetActive(false);
            _instance.transform.SetParent(poolRoot, false);
            liveCounts[prefab]--;
            pools[prefab].Enqueue(_instance);
            
            liveCounts[prefab] = Mathf.Max(0, GetLiveCount(prefab) - 1);
        }
    }
}
