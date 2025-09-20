using Managers;
using UnityEngine;

namespace Player
{
    public partial class PlayerController
    {
        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (_other.CompareTag("Shop")) {
                if (isInteracting)
                    UIManager.Instance.ActivateShop(true);
            }
        }

        private void OnTriggerStay2D(Collider2D _other)
        {
            if (_other.CompareTag("Shop")) {
                if (isInteracting)
                    UIManager.Instance.ActivateShop(true);
            }
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            if (_other.CompareTag("Shop")) {
                UIManager.Instance.ActivateShop(false);
            }
        }
    }
}