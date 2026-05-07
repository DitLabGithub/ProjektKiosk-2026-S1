using UnityEngine;
using System.Collections.Generic;

public class Item : MonoBehaviour
{
    public string name;
    public int price;

    public void AddToCart()
    {
        Counter.Instance.AddItem(this.gameObject);
    }

    public void RemoveFromCart()
    {
        Counter.Instance.RemoveItem(gameObject);
    }
}
