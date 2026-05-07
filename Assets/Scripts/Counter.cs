using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public static Counter Instance { get; private set; }
    public int Money;
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
        {
            itemsInCart.Add(item);
            foreach (var itemPrefab in ItemPrefabs)
            {
                if (itemPrefab.GetComponent<Item>().name == item.name)
                {
                    GameObject newItem = Instantiate(itemPrefab, itemContainer);
                    newItem.GetComponent<Button>().onClick.AddListener(() => RemoveItem(newItem));
                    break;
                }
            }
        }
    }
    public void RemoveItem(GameObject item)
    {
        itemsInCart.Remove(item.GetComponent<Item>());
            Destroy(item);
    }

    public void Clear()
    {
        itemsInCart.Clear();
    }

    public void Sell()
    {
        foreach (var item in itemsInCart)
        {
            if (requestedItems.Contains(item))
            {
                Money += (int)item.price;
                requestedItems.Remove(item);
            }
            else if (DialogueManager.Instance.currentNPC.data.favouriteItems.Contains(item))
            {
                Money += (int)item.price;
                requestedItems.Remove(item);
                int upsoldNumber = int.Parse(DialogueManager.Instance.currentNPC.GetVariable("UpsoldNumber"));
                upsoldNumber++;
                DialogueManager.Instance.currentNPC.SetVariable("UpsoldNumber", upsoldNumber.ToString());
            }
        }
    }

    }