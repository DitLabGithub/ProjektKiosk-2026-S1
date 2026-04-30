using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    public List<NPC> npcs = new List<NPC>();

    private DataLoader dataLoader;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dataLoader = GetComponent<DataLoader>();

        NPC amon = dataLoader.LoadNPC("Amon");

        if (amon != null)
        {
            npcs.Add(amon);

           // Debug.Log("Loaded NPC in the manager: " + amon.data.name);
        }
        NPC mira = dataLoader.LoadNPC("Mira");

        if (mira != null)
        {
            npcs.Add(mira);

           // Debug.Log("Loaded NPC in the manager: " + amon.data.name);
        }
        NPC gregor = dataLoader.LoadNPC("Gregor");

        if (gregor != null)
        {
            npcs.Add(gregor);

           // Debug.Log("Loaded NPC in the manager: " + amon.data.name);
        }
    }
    public NPC GetNPC(string npcName)
    {
        return npcs.Find(npc => npc.data.name == npcName);
    }
}