using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public NPC LoadNPC(string fileName)
    {
        // 1. Load JSON from Resources
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile == null)
        {
            Debug.LogError("JSON not found: " + fileName);
            return null;
        }

        // 2. Parse into CharacterData
        CharacterData data =
            JsonUtility.FromJson<CharacterData>(jsonFile.text);

        // 3. Convert sprite path → Sprite
        data.avatar = Resources.Load<Sprite>(data.avatarPath);

        if (data.avatar == null)
        {
            Debug.LogWarning("Sprite not found at: " + data.avatarPath);
        }

        // 4. Create NPC runtime object
        NPC npc = new NPC(data);

        // 5. Ensure variables exist in runtime dictionary
        // (NPC constructor already copies them, but this is safe fallback)
        if (npc.variables == null || npc.variables.Count == 0)
        {
            npc.variables = new System.Collections.Generic.Dictionary<string, string>();

            foreach (var v in data.variables)
            {
                npc.variables[v.name] = v.value;
            }
        }

        //Debug.Log("Loaded NPC: " + data.name);

        return npc;
    }
}