using UnityEngine;

public class MinimapRotate : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private RectTransform mapRect;   // parent container (minimap root)
    [SerializeField] private RectTransform iconRect;  // player icon (child)

    [Header("Settings")]
    [SerializeField] private bool rotateTogether = true; // true: icon rotates with map, false: icon fixed
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private bool invert = false;

    private const float ICON_FIXED_Z = -90f;

    private void Awake()
    {
        ApplyIconMode();
    }

    private void Update()
    {
        if (!mapRect) return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        if (mouseX == 0f) return;

        float spin = mouseX * sensitivity;
        if (!invert) spin = -spin;

        // 1) Map always spins
        mapRect.Rotate(0f, 0f, spin);

        // 2) Icon behavior depends on mode
        ApplyIconMode();
    }

    private void ApplyIconMode()
    {
        if (!iconRect || !mapRect) return;

        if (rotateTogether)
        {
            // icon rotates with the map (as a child) -> just set the local look you want
            iconRect.localRotation = Quaternion.Euler(0f, 0f, ICON_FIXED_Z);
        }
        else
        {
            // icon stays fixed on screen at -90°
            // counter the parent rotation in WORLD space
            iconRect.rotation = Quaternion.Euler(0f, 0f, ICON_FIXED_Z);
        }
    }

    // Optional: for UI Toggle
    public void SetRotateTogether(bool value)
    {
        rotateTogether = value;
        ApplyIconMode();
    }
}
