using TMPro;
using UnityEngine;

public class UI_ItemHover : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;

    public void SetItemInfo(GameObject item)
    {
        string cleanName =
            item.name.Replace("(Clone)", "").Trim();

        itemNameText.text = cleanName;

        itemPriceText.text =
            $"{item.GetComponent<Item>().price} €";
    }
}
