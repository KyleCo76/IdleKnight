using System.Collections.Generic;
using DigitalRuby.LightningBolt;
using Game;
using Managers;
using UnityEngine;

namespace Effects.Projectiles
{
    public class EnergySuperEffects : MonoBehaviour
    {
        [SerializeField, Tooltip("The effect that will be used to zap enemies")]
        private GameObject energyEffect;
        
        private CircleCollider2D teslaRange;
        private CapsuleCollider2D energyCapsule;
        private float zapFrequency = 1f;
        private int maxPoolSize;
        private int zapDamage = 2;
        private SingleEffectPoolManager effectPoolManager;
        
        private readonly Dictionary<GameObject, float> toBeZapped = new();
        private readonly Dictionary<GameObject, (GameObject, float)> isZapped = new();
        
        private const float ZapTtl = 0.2f;
        private const int InitialPoolSize = 15;

        private void Awake()
        {
            if (!energyEffect)
                energyEffect = Resources.Load<GameObject>("Projectiles/Effects/Supers/EnergyEffect");
            
            teslaRange = GetComponentInChildren<CircleCollider2D>();
            if (!teslaRange) {
                Debug.LogError("No CircleCollider2D component found");
            }

            if (!TryGetComponent(out energyCapsule)) {
                Debug.LogError("No CapsuleCollider2D component found");
                return;
            }
            
            effectPoolManager = new SingleEffectPoolManager(transform, energyEffect, InitialPoolSize, maxPoolSize);
        }

        private void Update()
        {
            if (!GameManager.Instance || GameManager.Instance.IsPaused)
                return;
            
            var snapshot = new Dictionary<GameObject, float>(toBeZapped);
            foreach (var pair in snapshot) {
                float timer = pair.Value - Time.deltaTime;
                if (timer <= 0) {
                    ZapEnemy(pair.Key);
                } else {
                    if (toBeZapped.ContainsKey(pair.Key))
                        toBeZapped[pair.Key] = timer;
                }
            }

            if (isZapped.Count > 0)
                RemoveZaps();
        }

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if ((energyCapsule && energyCapsule.IsTouching(_other)) || !_other.CompareTag("Enemy"))
                return;
            if (!toBeZapped.TryGetValue(_other.gameObject, out _))
                toBeZapped.Add(_other.gameObject, 0f); // Zap Immediately upon entering
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            if ((energyCapsule && energyCapsule.IsTouching(_other)) || !_other.CompareTag("Enemy"))
                return;

            EjectFromLists(_other.gameObject);
        }

        private void OnDestroy()
        {
            toBeZapped.Clear();
            foreach (var pair in isZapped)
                effectPoolManager.Release(pair.Value.Item1);
            
            isZapped.Clear();
        }


        private void EjectFromLists(GameObject _enemy)
        {
            toBeZapped.Remove(_enemy);
            if (isZapped.ContainsKey(_enemy)) {
                effectPoolManager.Release(isZapped[_enemy].Item1);
                isZapped.Remove(_enemy);
            }
        }

        public void Initialize(float _frequency, int _damage, int _maxPoolSize)
        {
            zapFrequency = _frequency;
            zapDamage = _damage;
            maxPoolSize = _maxPoolSize;
        }

        private void RemoveZaps()
        {
            var snapshot = new Dictionary<GameObject, (GameObject, float)>(isZapped);
            foreach (var pair in snapshot) {
                float timer = pair.Value.Item2 - Time.deltaTime;
                if (timer <= 0) {
                    if (!pair.Key.TryGetComponent(out Enemies.Controller enemyController)) {
                        Debug.LogError("No Controller component found on enemy for zap");
                        return;
                    }
                    enemyController.ChangeHealth(-zapDamage);
                    effectPoolManager.Release(pair.Value.Item1);
                    isZapped.Remove(pair.Key);
                    if (!pair.Key.activeInHierarchy)
                        return;
                    
                    if (toBeZapped.TryGetValue(pair.Key, out _))
                        toBeZapped[pair.Key] = timer;
                    else
                        toBeZapped.Add(pair.Key, zapFrequency);
                } else {
                    if (isZapped.ContainsKey(pair.Key)) {
                        var zapperTuple = isZapped[pair.Key];
                        zapperTuple.Item2 = timer;
                        isZapped[pair.Key] = zapperTuple;
                    }
                }
            }
        }

        private void ZapEnemy(GameObject _enemy)
        {
            if (!_enemy || !_enemy.activeInHierarchy) {
                toBeZapped.Remove(_enemy);
                return;
            }
            var zap = effectPoolManager.GetFromPool();
            if (!zap)
                return;
            
            if (!zap.TryGetComponent(out LightningBoltScript lightningBolt)){
                Debug.LogError("No lightningBolt component found on zap");
                toBeZapped.Remove(_enemy);
                return;
            }

            //lightningBolt.ManualMode = true;
            lightningBolt.StartObject = this.gameObject;
            lightningBolt.EndObject = _enemy;
            zap.SetActive(true);
            //lightningBolt.Trigger();
            
            toBeZapped.Remove(_enemy);
            if (isZapped.TryGetValue(_enemy, out _))
                isZapped[_enemy] = (zap, ZapTtl);
            else
                isZapped.Add(_enemy, (zap, ZapTtl));
        }
    }
}
