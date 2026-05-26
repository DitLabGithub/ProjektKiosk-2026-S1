using UnityEngine;
using System.Linq;

public class DataLoader : MonoBehaviour
{
    private readonly string[] npcVariables =
    {
        "friendship",
        "UpsoldNumber"
    };

    public NPC LoadNPC(string fileName)
    {
        TextAsset jsonFile =
            Resources.Load<TextAsset>(
                fileName
            );

        if (jsonFile == null)
        {
            Debug.LogError(
                "JSON not found: "
                + fileName
            );

            return null;
        }

        CharacterData data =
            JsonUtility.FromJson<CharacterData>(
                jsonFile.text
            );

        data.avatar =
            Resources.Load<Sprite>(
                data.avatarPath
            );
      /*if (data.ssiProfiles != null)
        {
            foreach (
                CustomSSIEntry profile
                in data.ssiProfiles
            )
            {
                profile.data.avatar =
                    Resources.Load<Sprite>(
                        profile.data.avatarPath
                    );
            }
        } */

        NPC npc =
            new NPC(data);

        foreach (
            VariableData variable
            in data.variables
        )
        {
            bool belongsToNPC =
                npcVariables.Contains(
                    variable.name
                );

            if (belongsToNPC)
            {
                npc.SetVariable(
                    variable.name,
                    variable.value
                );
            }
            else
            {
                // only create once
                if (
                    string.IsNullOrEmpty(
                        DialogueManager.Instance
                            .GetVariable(
                                variable.name
                            )
                    )
                )
                {
                    DialogueManager.Instance
                        .SetVariable(
                            variable.name,
                            variable.value
                        );
                }
            }
        }

        return npc;
    }
}