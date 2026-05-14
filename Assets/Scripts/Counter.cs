using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public static Counter Instance { get; private set; }
    public int Money;
    public int MoneyGoal;
    public RectTransform itemContainer;
    public List<GameObject> ItemPrefabs;
    public List<Item> requestedItems = new List<Item>();
    public List<Item> itemsInCart = new List<Item>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(GameObject GO)
    {
        Item item = GO.GetComponent<Item>();

        foreach (var itemPrefab in ItemPrefabs)
        {
            if (itemPrefab.GetComponent<Item>().name == item.name)
            {
                GameObject newItem =
                    Instantiate(itemPrefab, itemContainer);

                itemsInCart.Add(
                    newItem.GetComponent<Item>()
                );

                newItem.GetComponent<Button>()
                    .onClick.AddListener(() => RemoveItem(newItem));

                break;
            }
        }

        CheckForSaleButton();
    }
    public void RemoveItem(GameObject item)
    {
        itemsInCart.Remove(item.GetComponent<Item>());
            Destroy(item);
            CheckForSaleButton();
    }

    public void Clear()
    {
        itemsInCart.Clear();
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        CheckForSaleButton();
    }

    public void Sell()
    {
        int upsoldThisTransaction = 0;

        // VALIDATION PASS
        foreach (var item in itemsInCart)
        {
            bool requested =
                requestedItems.Exists(r => r.name == item.name);

            bool favourite =
                DialogueManager.Instance.currentNPC
                .data.favouriteItems
                .Contains(item.name);

            if (!requested && !favourite)
            {
                DialogueManager.Instance.ShowCurrentNode();
                Debug.Log("Transaction failed. Invalid item: " + item.name);
                return;
            }
        }

        // SELL PASS
        foreach (var item in itemsInCart)
        {
            bool requested =
                requestedItems.Exists(r => r.name == item.name);

            bool favourite =
                DialogueManager.Instance.currentNPC
                .data.favouriteItems
                .Contains(item.name);

            Money += item.price;

            string baseName = item.name.Replace(" ", "");

            // NORMAL SALE
            if (requested)
            {
                Debug.Log("Sold requested item: " + item.name);

                RegisterSale("Sold", baseName);
            }
            // UPSELL SALE
            else if (favourite)
            {
                upsoldThisTransaction++;

                Debug.Log("Upsold item: " + item.name);

                RegisterSale("Upsold", baseName);
            }
        }

        // SAVE GLOBAL UPSOLD COUNT
        if (upsoldThisTransaction > 0)
        {
            int currentUpsold =
                int.Parse(
                    DialogueManager.Instance.currentNPC
                    .GetVariable("UpsoldNumber")
                );

            currentUpsold += upsoldThisTransaction;

            DialogueManager.Instance.currentNPC
                .SetVariable("UpsoldNumber", currentUpsold.ToString());
        }

        Clear();
        DialogueManager.Instance.ShowCurrentNode();
        UIManager.Instance.UpdateMoney(Money);
        Debug.Log("Transaction successful");
    }
    public void SetupRequestedItems(string items)
    {
        requestedItems.Clear();

        string[] splitItems = items.Split(',');

        foreach (string itemName in splitItems)
        {
            string trimmedName = itemName.Trim();

            foreach (GameObject prefab in ItemPrefabs)
            {
                Item item = prefab.GetComponent<Item>();

                if (item.name == trimmedName)
                {
                    requestedItems.Add(item);

                    Debug.Log(
                        "Requested item added: "
                        + item.name
                    );

                    break;
                }
            }
        }
    }
    public void CheckForSaleButton()
    {
               if (itemsInCart.Count > 0)
        {
            UIManager.Instance.Ref_SellButton.SetActive(true);
        }
        else
        {
            UIManager.Instance.Ref_SellButton.SetActive(false);
        }
    }
    private void RegisterSale(string prefix, string baseName)
    {
        int index = 1;

        string variableName = prefix + baseName + index;

        var npc = DialogueManager.Instance.currentNPC;

        while (npc.HasVariable(variableName))
        {
            index++;
            variableName = prefix + baseName + index;
        }

        npc.SetVariable(variableName, "true");
    }
}