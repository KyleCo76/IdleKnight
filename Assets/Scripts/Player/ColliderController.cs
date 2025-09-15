using UnityEngine;

namespace Player
{
    public partial class PlayerController : MonoBehaviour
    {

        //private void ColliderAwake()
        //{

        //}

        //private void OnCollisionEnter2D(Collision2D collision)
        //{

        //}

        //private void OnCollisionExit2D(Collision2D collision)
        //{

        //}

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Shop")) {
                if (isInteracting)
                    UIManager.Instance.ActivateShop(true);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Shop")) {
                if (isInteracting)
                    UIManager.Instance.ActivateShop(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Shop")) {
                UIManager.Instance.ActivateShop(false);
            }
        }
    }
}