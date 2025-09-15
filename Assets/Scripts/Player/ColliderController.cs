using UnityEngine;

namespace Player
{
    public partial class PlayerController : MonoBehaviour
    {
        GameObject shopCanvasObject;
        GameObject uiCanvas;

        private void ColliderAwake()
        {
            shopCanvasObject = GameObject.Find("Shop");
            if (shopCanvasObject == null) {
                Debug.LogError("No Shop GameObject found in the scene.");
                enabled = false;
                return;
            }
            shopCanvasObject.SetActive(false);

            uiCanvas = GameObject.Find("Canvas");
            if (uiCanvas == null) {
                Debug.LogError("No UI Canvas found in the scene.");
                enabled = false;
                return;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {

        }

        private void OnCollisionExit2D(Collision2D collision)
        {

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Shop")) {
                if (isInteracting)
                    ActivateShop(true);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Shop")) {
                if (isInteracting)
                    ActivateShop(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Shop")) {
                ActivateShop(false);
            }
        }


        private void ActivateShop(bool _activate)
        {
            shopCanvasObject.SetActive(_activate);
            uiCanvas.SetActive(!_activate);
        }
    }
}