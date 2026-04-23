using System.Collections.Generic;

[System.Serializable]
public class NPC
{
    public CharacterData data;

    public Dictionary<string, string> variables;

    public NPC(CharacterData data)
    {
        this.data = data;

        variables = new Dictionary<string, string>();

        foreach (var v in data.variables)
        {
            variables[v.name] = v.value;
        }
    }
}