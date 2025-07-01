using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemCategory {
    Chips_Bag,
    Sproingles,
    Box_Of_Chocolates,
    Family_Chips,
    Soda_Can,
    Beer_Bottle,
    Haynako_Beer,
    Akira_Beer,
    Wine_Bottle,
    Blueport_Cigarettes,
    Rambolo_Cigarettes,
    HotShot_Cigarettes,
    Dirty_Magazine,
    Nerd_Comics,
    Package,
    Chicken_Jerky,
    Giddy_Beer,
}

public class ItemSlotData : MonoBehaviour {
    public ItemCategory category;
    public int slotIndex = -1; // Default slot for checkout zone, leave as is in inspector

    [HideInInspector] public Vector3 originalScale;
    [HideInInspector] public Quaternion originalRotation;

    private void Awake() {
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    public float Value => GetCategoryValue(category);
    public Vector3 originalShelfPosition; //leave as is

    void Start() {
        originalShelfPosition = transform.position;
    }

    private float GetCategoryValue(ItemCategory category) {
        switch (category) {
            case ItemCategory.Chips_Bag: return 3.0f;
            case ItemCategory.Sproingles: return 2.5f;
            case ItemCategory.Box_Of_Chocolates: return 10.0f;
            case ItemCategory.Family_Chips: return 5.0f;
            case ItemCategory.Soda_Can: return 2.5f;
            case ItemCategory.Beer_Bottle: return 3.5f;
            case ItemCategory.Haynako_Beer: return 15f;
            case ItemCategory.Akira_Beer: return 16f;
            case ItemCategory.Wine_Bottle: return 7.5f;
            case ItemCategory.Blueport_Cigarettes: return 14f;
            case ItemCategory.Rambolo_Cigarettes: return 16f;
            case ItemCategory.HotShot_Cigarettes: return 15f;
            case ItemCategory.Dirty_Magazine: return 12f;
            case ItemCategory.Nerd_Comics: return 5.5f;
            case ItemCategory.Package: return 0f;
            case ItemCategory.Chicken_Jerky: return 6f;
            case ItemCategory.Giddy_Beer: return 3f;
            default: return 0f;
        }
    }
}