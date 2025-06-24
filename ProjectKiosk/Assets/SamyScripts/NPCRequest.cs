using System.Collections.Generic;
using UnityEngine;

public class NPCRequest : MonoBehaviour {
    public List<ItemCategory> requestedItems = new List<ItemCategory>();

    public void SetRequest(List<ItemCategory> items) {
        requestedItems = items;
    }
}
