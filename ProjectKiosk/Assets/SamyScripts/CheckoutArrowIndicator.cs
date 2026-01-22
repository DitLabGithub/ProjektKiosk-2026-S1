using UnityEngine;

public class CheckoutArrowIndicator : MonoBehaviour {
    public Transform checkoutZone; // The checkout zone position
    public float bobSpeed = 2f; // How fast the arrow bobs
    public float bobAmount = 0.3f; // How far up/down it moves

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Position the arrow above the checkout zone
        if (checkoutZone != null) {
            transform.position = new Vector3(
                checkoutZone.position.x,
                checkoutZone.position.y + 1.5f,
                checkoutZone.position.z
            );
        }

        startPosition = transform.position;
        Hide(); // Start hidden
    }

    void Update() {
        // Bobbing animation
        if (spriteRenderer != null && spriteRenderer.enabled) {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    public void Show() {
        if (spriteRenderer != null) {
            spriteRenderer.enabled = true;
        }
    }

    public void Hide() {
        if (spriteRenderer != null) {
            spriteRenderer.enabled = false;
        }
    }

    public bool IsVisible() {
        return spriteRenderer != null && spriteRenderer.enabled;
    }
}
