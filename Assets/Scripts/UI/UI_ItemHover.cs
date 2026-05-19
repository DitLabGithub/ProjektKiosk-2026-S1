using TMPro;
using UnityEngine;

public class UI_ItemHover : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;

    public void SetItemInfo(GameObject item)
    {
        itemNameText.text = item.name;
        itemPriceText.text = $"{item.GetComponent<Item>().price} €";
    }
}
