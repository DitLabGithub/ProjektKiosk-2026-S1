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

        // 2. Parse JSON → CharacterData
        CharacterData data =
            JsonUtility.FromJson<CharacterData>(jsonFile.text);

        // 3. Load runtime sprite
        data.avatar = Resources.Load<Sprite>(data.avatarPath);

        if (data.avatar == null)
        {
            Debug.LogWarning("Sprite not found at: " + data.avatarPath);
        }

        // 4. Create runtime NPC (constructor handles variables)
        NPC npc = new NPC(data);

        return npc;
    }
}