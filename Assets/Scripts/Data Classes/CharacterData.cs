using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{

    public string name;
    public string avatarPath;

    public Sprite avatar;

    public List<ItemData> favouriteItems;
    public List<Detail> details;

    public List<VariableData> variables;

    public List<DialogueDayData> dialogues;
}