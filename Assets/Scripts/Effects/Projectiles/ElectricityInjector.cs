using System.Collections;
using System.Collections.Generic;
using DigitalRuby.LightningBolt;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Effects.Projectiles
{
    [RequireComponent(typeof(LightningBoltScript))]
    public class ElectricityInjector : MonoBehaviour
    {
        [SerializeField, Tooltip("The amount of damage to inflict on nearby enemies")]
        private int shockDamage = 2;
        [SerializeField, Tooltip("The radius of the area to inflict damage on")]
        private float shockRadius = 10f;
        [SerializeField, Tooltip("Should effect bounce to another nearby enemy from the effected enemy?")]
        private bool shockBounce = true;
        [SerializeField, Tooltip("Number of bounces to inflict on nearby enemies"), ShowIf("shockBounce")]
        private int bounceCount;
        [SerializeField, Tooltip("The duration of each bounced zap")]
        private float zapDuration = 1f;
        
        
        public void TryInjector(List<GameObject> _effects)
        {
            if (_effects.Count == 0)
                return;

            foreach (var effect in _effects) {
                if (!effect.TryGetComponent<LightningBoltScript>(out var lightningController)) {
                    Debug.LogError("No LightningBoltScript component found");
                    return;
                }

                if (!lightningController.EndObject) {
                    Debug.LogError("No EndObject found in LightningBoltScript component");
                    return;
                }

                if (!lightningController.EndObject.TryGetComponent(out IMethodInjectable injectable)) {
                    Debug.LogError("No IMethodInjectable component found on EndObject");
                    return;
                }
                injectable.InjectCoroutine(_args => ShockNearbyEnemies((int)_args[0], (float)_args[1], (int)_args[2],
                    (float)_args[3], (GameObject)_args[4], (Transform)_args[5]));
                injectable.InvokeCoroutine(shockDamage, shockRadius, bounceCount, zapDuration, effect, lightningController.EndObject.transform);
            }
        }
        
        private IEnumerator ShockNearbyEnemies(int _damage, float _radius, int _bounceCount, float _duration,
            GameObject _effectPrefab, Transform _enemyTransform)
        {
            Debug.Log("ShockNearbyEnemies");
            
            if (_damage <= 0 || _radius <= 0 || !_effectPrefab) {
                Debug.LogError("Invalid parameters for ShockNearbyEnemies");
                yield break;
            }

            ContactFilter2D filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = LayerMask.GetMask("Enemies")
            };
            
            var hits = new List<Collider2D>();
            Physics2D.OverlapCircle(_enemyTransform.position, _radius, filter, hits);

            if (hits.Count == 0)
                yield break;

            GameObject closest = null;
            var closestDistance = float.MaxValue;
            foreach (var hit in hits) {
                var distance = Vector2.Distance(hit.transform.position, transform.position);
                if (distance < closestDistance) {
                    closest = hit.gameObject;
                    closestDistance = distance;
                }
            }

            if (!closest)
                yield break;

            if (!closest.TryGetComponent(out Enemies.Controller enemyController)) {
                Debug.LogError("No Controller found on closest enemy");
                yield break;
            }
            
            // Shock the nearest enemy
            var effect = Instantiate(_effectPrefab, _enemyTransform.position, Quaternion.identity);
            Debug.Log("Instantiated effect");
            if (!effect) {
                Debug.LogError("Failed to instantiate effect");
                yield break;
            }
            if (!effect.TryGetComponent(out LightningBoltScript lightningController)) {
                Debug.LogError("No LightningBoltScript component found on effect");
                yield break;
            }

            effect.transform.parent = _enemyTransform;
            lightningController.StartObject = this.gameObject;
            lightningController.EndObject = closest;
            effect.SetActive(true);
            
            enemyController.ChangeHealth(-_damage);

            if (!closest.activeInHierarchy) {
                Destroy(effect);
                yield break;           
            }

            // Try to bounce to the closest enemy if possible
            if (_bounceCount > 0) {
                _bounceCount--;
                if (!closest.TryGetComponent(out IMethodInjectable bounceable)) {
                    Debug.LogError("Closest enemy is not injectable");
                    Destroy(effect);
                    yield break;
                }

                bounceable.InjectCoroutine(_args => ShockNearbyEnemies((int)_args[0], (float)_args[1], (int)_args[2],
                    (float)_args[3], (GameObject)_args[4], (Transform)_args[5]));
                bounceable.InvokeCoroutine(_damage, _radius, _bounceCount, _duration, _effectPrefab, closest.transform);
            }
            
            yield return new WaitForSeconds(_duration);
            Destroy(effect);
        }

        private void OnValidate()
        {
            if (!shockBounce)
                bounceCount = 0;
        }
    }
}
