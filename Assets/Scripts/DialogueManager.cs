using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public NPC currentNPC;

    public DialogueDayData currentDialogue;

    public NodeData currentNode;

    private int currentNPCIndex = 0;

    private DaySchedule currentSchedule;

    
    private void Awake()
    {
        Instance = this;
    }

    public void StartDay(int day)
    {
        DayManager.Instance.currentDay = day;

        currentSchedule = DayManager.Instance.GetToday();

        currentNPCIndex = 0;

        StartNextNPCDialogue();
    }

    public void StartNextNPCDialogue()
    {
        if (currentNPCIndex >= currentSchedule.npcOrder.Count)
        {
            Debug.Log("Day complete");
            return;
        }

        string npcName =
            currentSchedule.npcOrder[currentNPCIndex];

        currentNPC =
            NPCManager.Instance.GetNPC(npcName);

        currentDialogue =
            currentNPC.data.dialogues.Find(
                d => d.day == DayManager.Instance.currentDay
            );

        if (currentDialogue == null)
        {
            Debug.LogError("Dialogue missing");
            return;
        }

        currentNode = currentDialogue.nodes[0];

        ShowCurrentNode();
    }

    public void ShowCurrentNode()
    {
        UIManager.Instance.ShowNode(currentNode);
    }
    public void SelectOption(OptionData option)
    {
        ApplyEffects(option.effects);

        string nextNodeId = option.nextNodes[0];

        if (nextNodeId == "End_Dialogue")
        {
            currentNPCIndex++;

            StartNextNPCDialogue();

            return;
        }

        currentNode =
            currentDialogue.nodes.Find(
                n => n.id == nextNodeId
            );

        if (currentNode == null)
        {
            Debug.LogError("Node not found: " + nextNodeId);
            return;
        }

        ShowCurrentNode();
    }
    void ApplyEffects(List<EffectData> effects)
    {
        foreach (var effect in effects)
        {
            VariableData variable =
                currentNPC.data.variables.Find(
                    v => v.name == effect.variable
                );

            if (variable != null)
            {
                variable.value = effect.value;
            }

            switch (effect.variable)
            {
                case "open_id_tab" when effect.value == "true":
                    UIManager.Instance.ToggleIDentification_ID();
                    break;
                case "open_id_tab" when effect.value == "false":
                    UIManager.Instance.ToggleIDentification_ID();
                    break;
                case "open_ssi_tab":
                    UIManager.Instance.ToggleIdentification_SSI();

                    SSIManager.Instance.SetupOrders(effect.value);

                    break;
            }
        }
    }
    public bool MeetsRequirements(List<RequirementData> requirements)
    {
        if (requirements == null || requirements.Count == 0)
            return true;

        foreach (var requirement in requirements)
        {
            // Variable missing
            if (!currentNPC.variables.ContainsKey(requirement.variable))
                return false;

            // Wrong value
            if (currentNPC.variables[requirement.variable] != requirement.value)
                return false;
        }

        return true;
    }
}