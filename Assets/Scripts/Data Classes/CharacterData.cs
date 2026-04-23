using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string name;

    [HideInInspector]
    public string avatarPath;

    public Sprite avatarSprite;

    public List<ItemData> favouriteItems;

    public List<DetailData> details;

    public List<VariableData> variables;
}