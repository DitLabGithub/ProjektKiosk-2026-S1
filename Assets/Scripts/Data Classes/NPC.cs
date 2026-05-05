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

        LoadVariables();
        InjectRuntimeDefaults();
    }

    private void LoadVariables()
    {
        if (data.variables == null) return;

        foreach (var v in data.variables)
        {
            variables[v.name] = v.value;
        }
    }

    private void InjectRuntimeDefaults()
    {
        // ID system runtime state
        if (!variables.ContainsKey("ID_Result"))
            variables["ID_Result"] = "none";

        if (!variables.ContainsKey("ID_Checked"))
            variables["ID_Checked"] = "false";
    }
}