using UnityEngine;
using UnityEngine.EventSystems;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 이 컴포넌트가 붙은 오브젝트(보통 타이틀바)를 드래그하면 windowRoot 전체가 움직인다.
    /// windowRoot는 부모 캔버스(또는 지정한 영역) 밖으로 나가지 않도록 클램프된다.
    /// </summary>
    public class DraggableWindow : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
    {
        [SerializeField] RectTransform windowRoot;

        RectTransform selfRect;
        Canvas canvas;

        void Awake()
        {
            selfRect = GetComponent<RectTransform>();
            if (windowRoot == null) windowRoot = selfRect;
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            windowRoot.SetAsLastSibling();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvas == null) return;

            float scale = canvas.scaleFactor <= 0f ? 1f : canvas.scaleFactor;
            windowRoot.anchoredPosition += eventData.delta / scale;
            ClampToParent();
        }

        void ClampToParent()
        {
            RectTransform parent = windowRoot.parent as RectTransform;
            if (parent == null) return;

            Vector2 halfWindow = windowRoot.rect.size * 0.5f;
            Vector2 halfParent = parent.rect.size * 0.5f;

            Vector2 pos = windowRoot.anchoredPosition;
            pos.x = Mathf.Clamp(pos.x, -halfParent.x + halfWindow.x, halfParent.x - halfWindow.x);
            pos.y = Mathf.Clamp(pos.y, -halfParent.y + halfWindow.y, halfParent.y - halfWindow.y);
            windowRoot.anchoredPosition = pos;
        }
    }
}
