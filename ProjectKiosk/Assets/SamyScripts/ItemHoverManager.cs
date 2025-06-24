using UnityEngine;
using TMPro;

public class ItemHoverManager : MonoBehaviour {
    public LayerMask itemLayer; // Set to "PickupItem"
    public Camera mainCamera;

    private ItemHoverDisplay currentHover;

    void Update() {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, itemLayer);

        if (hit.collider != null) {
            ItemHoverDisplay hoverDisplay = hit.collider.GetComponent<ItemHoverDisplay>();
            if (hoverDisplay != null) {
                if (currentHover != hoverDisplay) {
                    if (currentHover != null) currentHover.OnHoverExit();
                    hoverDisplay.OnHoverEnter();
                    currentHover = hoverDisplay;
                }
            }
        } else if (currentHover != null) {
            currentHover.OnHoverExit();
            currentHover = null;
        }
    }
}
