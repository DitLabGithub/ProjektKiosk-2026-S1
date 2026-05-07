using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string name;
    public string avatarPath;

    public Sprite avatar;

    public int age;
    public bool above18;
    public string address;
    public string expiryDate;

    public string gender;
    public int height;
    public string hairColor;
    public string eyeColor;

    public List<ItemData> favouriteItems;
    public List<Detail> details;

    [System.NonSerialized]
    public List<VariableData> variables;
    public List<DialogueDayData> dialogues;
}