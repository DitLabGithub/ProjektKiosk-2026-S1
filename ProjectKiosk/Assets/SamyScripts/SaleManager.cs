using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaleManager : MonoBehaviour {
    public Button makeSaleButton;
    public NPCRequest currentNPC;
    public ItemPickupManager itemPickupManager;

    [Header("Audio Clips")]
    public AudioClip saleSuccessClip;
    public AudioClip saleRejectedClip;

    private void Start() {
        makeSaleButton.onClick.AddListener(AttemptSale);
    }

    public void AttemptSale() {
        // Play button click sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonSellSound();
        }

        if (currentNPC == null || itemPickupManager == null) {
            Debug.LogWarning("Missing references in SaleManager.");
            return;
        }

        List<ItemCategory> requested = currentNPC.requestedItems;
        List<ItemCategory> present = GetItemCategoriesInCheckout();

        if (CheckRequestedItemsMatch(requested, present)) {
            Debug.Log("Sale successful!");
            SoundManager.Instance.PlaySound(saleSuccessClip);
            itemPickupManager.SellItems(); // Only called on success
            OnSaleSuccess();
        } else {
            Debug.Log("Sale rejected!");
            SoundManager.Instance.PlaySound(saleRejectedClip); // Play rejection sound
            OnSaleRejected();
        }
    }

    private List<ItemCategory> GetItemCategoriesInCheckout() {
        List<ItemCategory> categories = new List<ItemCategory>();

        foreach (Transform item in itemPickupManager.checkoutDropZone) {
            ItemSlotData data = item.GetComponent<ItemSlotData>();
            if (data != null) {
                categories.Add(data.category);
            }
        }

        return categories;
    }

    private bool CheckRequestedItemsMatch(List<ItemCategory> requested, List<ItemCategory> present) {
        List<ItemCategory> presentCopy = new List<ItemCategory>(present);

        foreach (ItemCategory required in requested) {
            if (!presentCopy.Contains(required))
                return false;

            presentCopy.Remove(required);
        }

        return true;
    }

    private void OnSaleSuccess() {
        Debug.Log("NPC: Thanks, this is exactly what I wanted!");
        // Add visual feedback here if needed
    }

    private void OnSaleRejected() {
        Debug.Log("NPC: This isn't what I asked for.");
        // Add visual feedback here if needed
    }
}
