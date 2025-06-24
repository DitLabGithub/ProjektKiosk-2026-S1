using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemCategory {
    ChipsBag,
    Sproingles,
    BoxOfChocolates,
    FamilyChips,
    SodaCan,
    BeerBottle,
    Haynako_Beer,
    Akira_Beer,
    WineBottle,
    BluePortCigarettes,
    RamboloCigarettes,
    HotShotCigarettes,
    DirtyMagazine,
    NerdComics,
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
            case ItemCategory.ChipsBag: return 3.0f;
            case ItemCategory.Sproingles: return 2.5f;
            case ItemCategory.BoxOfChocolates: return 10.0f;
            case ItemCategory.FamilyChips: return 5.0f;
            case ItemCategory.SodaCan: return 2.5f;
            case ItemCategory.BeerBottle: return 3.5f;
            case ItemCategory.Haynako_Beer: return 15f;
            case ItemCategory.Akira_Beer: return 16f;
            case ItemCategory.WineBottle: return 7.5f;
            case ItemCategory.BluePortCigarettes: return 14f;
            case ItemCategory.RamboloCigarettes: return 16f;
            case ItemCategory.HotShotCigarettes: return 15f;
            case ItemCategory.DirtyMagazine: return 12f;
            case ItemCategory.NerdComics: return 5.5f;
            case ItemCategory.Package: return 0f;
            case ItemCategory.Chicken_Jerky: return 6f;
            case ItemCategory.Giddy_Beer: return 3f;
            default: return 0f;
        }
    }
}