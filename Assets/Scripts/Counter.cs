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

    public bool hasActiveRequest = false;

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
        if (itemsInCart.Count == 0)
        {
            Debug.Log("No items in cart to sell.");
            return;
        }

        int upsoldThisTransaction = 0;

        // VALIDATION PASS

        Dictionary<string, int> requestedCounts =
            new Dictionary<string, int>();

        Dictionary<string, int> cartCounts =
            new Dictionary<string, int>();

        // Count requested items
        foreach (var requestedItem in requestedItems)
        {
            if (!requestedCounts.ContainsKey(requestedItem.name))
            {
                requestedCounts[requestedItem.name] = 0;
            }

            requestedCounts[requestedItem.name]++;
        }

        // Count cart items
        foreach (var item in itemsInCart)
        {
            if (!cartCounts.ContainsKey(item.name))
            {
                cartCounts[item.name] = 0;
            }

            cartCounts[item.name]++;
        }

        // 1. Validate exact requested item quantities
        foreach (var pair in requestedCounts)
        {
            string itemName = pair.Key;
            int requiredAmount = pair.Value;

            int cartAmount =
                cartCounts.ContainsKey(itemName)
                ? cartCounts[itemName]
                : 0;

            if (cartAmount != requiredAmount)
            {
                Debug.Log(
                    "Transaction failed. Wrong quantity for requested item: "
                    + itemName
                );

                DialogueManager.Instance.ShowCurrentNode();
                return;
            }
        }

        // 2. Validate all cart items
        foreach (var pair in cartCounts)
        {
            string itemName = pair.Key;
            int amount = pair.Value;

            bool requested =
                requestedCounts.ContainsKey(itemName);

            bool favourite =
                DialogueManager.Instance.currentNPC
                .data.favouriteItems
                .Contains(itemName);

            // Item is neither requested nor favourite
            if (!requested && !favourite)
            {
                Debug.Log(
                    "Transaction failed. Invalid item: "
                    + itemName
                );

                DialogueManager.Instance.ShowCurrentNode();
                return;
            }

            // Requested items must EXACTLY match quantity
            if (requested)
            {
                int requiredAmount =
                    requestedCounts[itemName];

                if (amount != requiredAmount)
                {
                    Debug.Log(
                        "Transaction failed. Wrong amount of requested item: "
                        + itemName
                    );

                    DialogueManager.Instance.ShowCurrentNode();
                    return;
                }
            }

            // Favourite items can only appear once
            // BUT ONLY if they are NOT also requested
            if (favourite && !requested)
            {
                if (amount > 1)
                {
                    Debug.Log(
                        "Transaction failed. Too many favourite items: "
                        + itemName
                    );

                    DialogueManager.Instance.ShowCurrentNode();
                    return;
                }
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
        requestedItems.Clear();
        hasActiveRequest = false;
        CheckForSaleButton();
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
        if (requestedItems.Count > 0)
        {
            hasActiveRequest = true;
        }
        CheckForSaleButton();
    }
    public void CheckForSaleButton()
    {
        bool shouldShow = hasActiveRequest && requestedItems.Count > 0;

        UIManager.Instance.Ref_SellButton.SetActive(shouldShow);
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