using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Utilities
{
    public class DragWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform dragRectTransform;
        [SerializeField] private Canvas canvas;

        private void Awake()
        {
            if (dragRectTransform == null)
                dragRectTransform = transform.parent.GetComponent<RectTransform>();

            if (canvas != null) return;
            var testCanvasTransform = transform.parent;

            while (testCanvasTransform != null)
            {
                canvas = testCanvasTransform.GetComponent<Canvas>();
                if (canvas != null) break;
                testCanvasTransform = testCanvasTransform.parent;
            }
        }

        public void OnDrag(PointerEventData eventData) => dragRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        public void OnPointerDown(PointerEventData eventData) => dragRectTransform.SetAsLastSibling();
    }
}