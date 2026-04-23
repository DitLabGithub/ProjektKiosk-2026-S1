using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    public string name;

    public string avatarPath;

    public List<ItemData> favouriteItems;

    public List<DetailData> details;

    public List<VariableData> variables;
}