using UnityEngine;

namespace Enemies
{
    public class EnemyAnimatorOverrides : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            
        }

        public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            
        }

        public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            var enemy = _animator.gameObject;
            if (enemy.TryGetComponent(out Controller controller)) {
                controller.Die(enemy.transform.position);
            }
        }
    }
}
