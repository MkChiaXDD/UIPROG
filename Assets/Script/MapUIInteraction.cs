using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapUIInteraction : MonoBehaviour,
    IScrollHandler,
    IBeginDragHandler,
    IDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform map;      // the image you move & scale
    [SerializeField] private RectTransform viewport; // masked area

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2.5f;

    private float currentZoom = 1f;
    private Vector2 lastDragPos;

    // =======================
    // SCROLL TO ZOOM
    // =======================
    public void OnScroll(PointerEventData eventData)
    {
        currentZoom += eventData.scrollDelta.y * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        map.localScale = Vector3.one * currentZoom;
        ClampPosition();
    }

    // =======================
    // CLICK + DRAG TO PAN
    // =======================
    public void OnBeginDrag(PointerEventData eventData)
    {
        lastDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastDragPos;
        map.anchoredPosition += delta;
        lastDragPos = eventData.position;

        ClampPosition();
    }

    // =======================
    // CLAMP INSIDE VIEWPORT
    // =======================
    private void ClampPosition()
    {
        if (!viewport) return;

        Vector2 mapSize = map.rect.size * map.localScale;
        Vector2 viewSize = viewport.rect.size;

        float limitX = Mathf.Max(0, (mapSize.x - viewSize.x) * 0.5f);
        float limitY = Mathf.Max(0, (mapSize.y - viewSize.y) * 0.5f);

        Vector2 pos = map.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        pos.y = Mathf.Clamp(pos.y, -limitY, limitY);

        map.anchoredPosition = pos;
    }
}
