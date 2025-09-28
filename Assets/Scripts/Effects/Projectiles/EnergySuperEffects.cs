using DigitalRuby.LightningBolt;
using UnityEngine;

namespace Effects.Projectiles
{
    public class EnergySuperEffects : MonoBehaviour
    {
        private GameObject energyEffect;
        private LightningBoltScript lightningController;


        private void Awake()
        {
            energyEffect = Resources.Load<GameObject>("Projectiles/Effects/Supers/EnergyEffect");
            if (!energyEffect) {
                Debug.LogError("Unable to find energyEffect prefab");
                return;
            }

            if (!energyEffect.TryGetComponent(out lightningController)) {
                Debug.LogError("No Lightning Bolt component found");
            }
        }
    }
}
