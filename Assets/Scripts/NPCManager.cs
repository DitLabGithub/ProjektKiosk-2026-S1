using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public List<CharacterData> characters = new List<CharacterData>();

    private void Start()
    {
        LoadCharacter("Amon");
    }

    private void LoadCharacter(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile == null)
        {
            Debug.LogError("Could not find JSON: " + fileName);
            return;
        }

        CharacterData character =
            JsonUtility.FromJson<CharacterData>(jsonFile.text);

        characters.Add(character);


        Debug.Log("Loaded: " + character.name);
    }
}