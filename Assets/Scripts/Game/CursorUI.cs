using UnityEngine;

namespace Game
{
    public class CursorUI : MonoBehaviour
    {
        [SerializeField, Tooltip("The image that will be used as the cursor")]
        private RectTransform cursorImg;

        private CursorUI instance;
        private readonly Vector2 hotspot = new(4f, 3f);

        private void Awake()
        {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            // DontDestroyOnLoad is handled by GameManager
            Cursor.visible = false;
        }
        private void Update()
        {
            if (!cursorImg)
                return;
        
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cursorImg.parent as RectTransform,
                Input.mousePosition,
                null,
                out var pos
            );
            cursorImg.localPosition = pos - hotspot;
        }
    }
}
