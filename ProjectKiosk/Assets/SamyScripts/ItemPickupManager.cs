using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using TMPro;
//using static UnityEditor.Progress;
using System.Collections;


public class ItemPickupManager : MonoBehaviour {
    public TextMeshProUGUI checkoutTotalText;
    public Transform leftHandSlot;
    public Transform rightHandSlot;
    public Transform checkoutDropZone;
    public float totalFunds = 0f;
    public TextMeshProUGUI fundsText; // UI for total funds
    public ReturnItemConfirmationUI returnConfirmationUI; // UI for when returning items that are already in-hand to the shelf

    private List<GameObject> carriedItems = new List<GameObject>();
    private List<int> availableSlots = new List<int>(); // Reusable slots
    private List<int> usedSlots = new List<int>(); // Currently occupied slots

    private float slotSpacing = 1f; // Distance between items in the checkout zone

    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // First check for pickup items only
            int pickupLayerMask = LayerMask.GetMask("PickupItem");
            Collider2D hitItem = Physics2D.OverlapPoint(mousePos, pickupLayerMask);

            if (hitItem) {
                Debug.Log("Clicked on: " + hitItem.gameObject.name);
                TryPickup(hitItem.gameObject);
                return;
            }

            // Then check for checkout area
            int checkoutLayerMask = LayerMask.GetMask("CheckoutZone");
            Collider2D hitCheckout = Physics2D.OverlapPoint(mousePos, checkoutLayerMask);

            if (hitCheckout && hitCheckout.CompareTag("CheckoutArea")) {
                DropItemAtCheckout();
            }
            int shelfLayerMask = LayerMask.GetMask("Shelf");
            Collider2D hitShelf = Physics2D.OverlapPoint(mousePos, shelfLayerMask);

            if (hitShelf && carriedItems.Count > 0) {
                returnConfirmationUI.Show(() => {
                    ReturnItemsToShelf();
                });
                return;
            }

        }
    }


    void TryPickup(GameObject item) {
        // Check if the item is already being held
        if (carriedItems.Contains(item)) return;

        // Determine which hand is free
        bool leftHandFree = !carriedItems.Exists(obj => obj.transform.parent == leftHandSlot);
        bool rightHandFree = !carriedItems.Exists(obj => obj.transform.parent == rightHandSlot);
        Transform targetSlot = null;
        if (leftHandFree) {
            targetSlot = leftHandSlot;
        } else if (rightHandFree) {
            targetSlot = rightHandSlot;
        } else {
            Debug.Log("Hands are full!");
            FindObjectOfType<SoundManager>().PlayPickupFailSound();
            return;
        }


        // If item is in checkout, free its slot
        if (item.transform.parent == checkoutDropZone) {
            RemoveItemFromCheckout(item);
        }
        
        bool cameFromCheckout = item.transform.parent == checkoutDropZone;

        if (cameFromCheckout) {
            RemoveItemFromCheckout(item);
        }

        // Attach item to the free hand
        item.transform.SetParent(targetSlot);
        item.transform.localPosition = Vector3.zero;
        carriedItems.Add(item);
        SoundManager.Instance.PlayPickupSound();

        // Apply scaling and rotation based on which hand
        ItemSlotData data = item.GetComponent<ItemSlotData>();
        if (data != null) {
            item.transform.localScale = data.originalScale * 1.5f;

            // Give opposite rotation for each hand
            if (targetSlot == leftHandSlot) {
                item.transform.localRotation = Quaternion.Euler(0, 0, 10f); // Slight tilt to the right
            } else if (targetSlot == rightHandSlot) {
                item.transform.localRotation = Quaternion.Euler(0, 0, -10f); // Slight tilt to the left
            }
        }



        // Update the total AFTER reparenting
        if (cameFromCheckout) {
            UpdateCheckoutTotal();
        }

    }

    void UpdateCheckoutTotal() {
        float total = 0f;

        foreach (Transform item in checkoutDropZone) {
            ItemSlotData itemScript = item.GetComponent<ItemSlotData>();
            if (itemScript != null) {
                total += itemScript.Value;
            }
        }

        checkoutTotalText.text = $"Total: €{total:0.00}";
    }

    void DropItemAtCheckout() {
        if (carriedItems.Count == 0) return;

        GameObject itemToDrop = carriedItems[0];
        carriedItems.RemoveAt(0);
        SoundManager.Instance.PlayPlaceSound();
        // Determine slot index (reserve immediately)
        int slotIndex;
        if (availableSlots.Count > 0) {
            slotIndex = availableSlots[0];
            availableSlots.RemoveAt(0);
        } else {
            slotIndex = usedSlots.Count > 0 ? usedSlots.Max() + 1 : 0;
        }
        usedSlots.Add(slotIndex);

        // Position the item
        Vector3 dropPosition = new Vector3(
            checkoutDropZone.position.x - (slotSpacing * slotIndex),
            checkoutDropZone.position.y,
            checkoutDropZone.position.z
        );

        itemToDrop.transform.SetParent(checkoutDropZone);
        itemToDrop.transform.position = dropPosition;

        ItemSlotData data = itemToDrop.GetComponent<ItemSlotData>();
        itemToDrop.transform.localScale = data.originalScale;
        itemToDrop.transform.rotation = data.originalRotation;


        // Tag slot info
        itemToDrop.name = $"CheckoutItem_{slotIndex}";
        itemToDrop.GetComponent<ItemSlotData>().slotIndex = slotIndex;

        // Update total price
        UpdateCheckoutTotal();
    }

    void ReturnItemsToShelf() {
        foreach (GameObject item in carriedItems) {
            item.transform.SetParent(null);

            // Optional: return to original shelf position
            var itemData = item.GetComponent<ItemSlotData>();
            if (itemData != null) {
                item.transform.position = itemData.originalShelfPosition;
                item.transform.localScale = itemData.originalScale;
            }
            item.transform.localRotation = Quaternion.identity;
        }

        carriedItems.Clear();
    }


    void RemoveItemFromCheckout(GameObject item) {
        ItemSlotData slotData = item.GetComponent<ItemSlotData>();
        if (slotData != null) {
            int slotIndex = slotData.slotIndex;
            if (!availableSlots.Contains(slotIndex)) {
                availableSlots.Add(slotIndex);
            }
            usedSlots.Remove(slotIndex);
        }
    }

    public void SellItems() {
        float saleTotal = 0f;

        List<Transform> itemsToRemove = new List<Transform>();

        // Collect all item values in checkout zone
        foreach (Transform item in checkoutDropZone) {
            ItemSlotData itemData = item.GetComponent<ItemSlotData>();
            if (itemData != null) {
                saleTotal += itemData.Value;
                itemsToRemove.Add(item); // Store to delete after
            }
        }

        // Remove items from checkout and destroy them
        foreach (Transform item in itemsToRemove) {
            RemoveItemFromCheckout(item.gameObject);
            Destroy(item.gameObject);
        }

        // Add value to player's funds
        totalFunds += saleTotal;

        // Update UI
        UpdateFundsUI();
        StartCoroutine(DelayedCheckoutTotalUpdate()); //This is a coroutine so that the checkout value doesn't bug out when selling stuff
    }
    void UpdateFundsUI() {
        fundsText.text = $"Funds: €{totalFunds:0.00}";
    }

    private IEnumerator DelayedCheckoutTotalUpdate() {
        yield return null; // Wait one frame
        UpdateCheckoutTotal();
    }


}
