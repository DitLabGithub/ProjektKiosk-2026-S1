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

        UIManager.Instance.SetMoneyGoal(Counter.Instance.MoneyGoal);
        UIManager.Instance.SetDay(day);

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
            // =========================
            // KEY MOMENTS
            // =========================

            if (!string.IsNullOrEmpty(effect.keyMomentText))
            {
                KeyMomentData moment =
                    new KeyMomentData();

                moment.text =
                    effect.keyMomentText;

                moment.isPositive =
                    effect.keyMomentPositive;

                currentNPC.data.keyMoments.Add(moment);

                Debug.Log(
                    "Added key moment: "
                    + moment.text
                );
            }

            // =========================
            // MESSAGES
            // =========================

            if (!string.IsNullOrEmpty(effect.messageTitle))
            {
                MessageData message =
                    new MessageData();

                message.title =
                    effect.messageTitle;

                message.sender =
                    effect.messageSender;

                message.content =
                    effect.messageContent;

                message.read = false;

                MessagesManager.Instance.messages
                    .Add(message);

                Debug.Log(
                    "Added message: "
                    + message.title
                );
            }

            // =========================
            // SPECIAL EFFECTS
            // =========================

            switch (effect.variable)
            {
                case "open_id_tab":

                    UIManager.Instance
                        .ToggleIDentification_ID();

                    break;

                case "open_ssi_tab":

                    UIManager.Instance
                        .ToggleIdentification_SSI();

                    SSIManager.Instance
                        .SetupOrders(effect.value);

                    break;

                case "unlock_detail":

                    Detail detail =
                        currentNPC.data.details.Find(
                            d => d.key == effect.value
                        );

                    if (detail != null)
                    {
                        detail.unlocked = true;

                        Debug.Log(
                            "Unlocked detail: "
                            + detail.key
                        );
                    }

                    break;

                case "open_wares_tab":

                    UIManager.Instance
                        .ToggleWares();

                    Counter.Instance
                        .SetupRequestedItems(
                            effect.value
                        );

                    break;

                default:

                    // Normal runtime variable
                    if (!string.IsNullOrEmpty(effect.variable))
                    {
                        currentNPC.SetVariable(
                            effect.variable,
                            effect.value
                        );
                    }

                    break;
            }
        }
    }
    public bool MeetsRequirements(
    List<RequirementData> requirements)
    {
        if (requirements == null
            || requirements.Count == 0)
        {
            return true;
        }

        foreach (var requirement in requirements)
        {
            string value =
                currentNPC.GetVariable(
                    requirement.variable
                );

            if (value != requirement.value)
            {
                return false;
            }
        }

        return true;
    }
}