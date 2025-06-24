using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Camera canvasCamera;
    private Vector3 offset;

    void Start() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) {
            Debug.LogError("DraggableItem: No Canvas found in parent hierarchy.");
        } else {
            canvasCamera = canvas.worldCamera;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Calculate offset between object's world position and mouse position
        Vector3 worldMousePos = GetWorldPosition(eventData);
        offset = rectTransform.position - worldMousePos;
    }

    public void OnDrag(PointerEventData eventData) {
        if (canvas == null) return;

        // Move the object to the mouse position + offset in world space
        Vector3 worldMousePos = GetWorldPosition(eventData);
        rectTransform.position = worldMousePos + offset;
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    private Vector3 GetWorldPosition(PointerEventData eventData) {
        Vector3 screenPoint = eventData.position;

        // Calculate distance from camera to object for correct depth
        float distance = Vector3.Distance(canvasCamera.transform.position, rectTransform.position);

        // Convert screen point to world point with correct Z distance
        Vector3 worldPoint = canvasCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distance));
        return worldPoint;
    }
}