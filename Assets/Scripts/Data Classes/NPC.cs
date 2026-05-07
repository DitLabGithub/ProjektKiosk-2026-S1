using System.Collections.Generic;

[System.Serializable]
public class NPC
{
    public CharacterData data;

    // Runtime variables
    public List<VariableData> variables =
        new List<VariableData>();

    public NPC(CharacterData data)
    {
        this.data = data;

        CopyVariablesFromData();

        InjectRuntimeDefaults();
    }

    private void CopyVariablesFromData()
    {
        if (data.variables == null)
            return;

        foreach (var v in data.variables)
        {
            variables.Add(new VariableData
            {
                name = v.name,
                value = v.value
            });
        }
    }

    private void InjectRuntimeDefaults()
    {
        AddVariableIfMissing("ID_Result", "none");
        AddVariableIfMissing("ID_Checked", "false");

        AddVariableIfMissing("SSI_Result", "none");

        AddVariableIfMissing("open_id_tab", "false");
        AddVariableIfMissing("open_ssi_tab", "false");
    }

    public void AddVariableIfMissing(string name, string value)
    {
        VariableData existing =
            variables.Find(v => v.name == name);

        if (existing == null)
        {
            variables.Add(new VariableData
            {
                name = name,
                value = value
            });
        }
    }

    public string GetVariable(string variableName)
    {
        VariableData variable =
            variables.Find(v => v.name == variableName);

        if (variable == null)
            return "";

        return variable.value;
    }

    public void SetVariable(string variableName, string value)
    {
        VariableData variable =
            variables.Find(v => v.name == variableName);

        if (variable != null)
        {
            variable.value = value;
        }
        else
        {
            variables.Add(new VariableData
            {
                name = variableName,
                value = value
            });
        }
    }
}