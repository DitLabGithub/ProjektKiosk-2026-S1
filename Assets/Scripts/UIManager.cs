using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public  Color ColorPositive;
    public  Color ColorNegative;
    public  Color ColorRead;

    [Header("References")]
    public Canvas mainCanvas;
    public MainMenu Ref_MainMenu;
    public LevelSelect Ref_LevelSelect;
    public UI_ID Ref_ID;
    public SSIManager Ref_SSI;
    public GameObject Ref_Wares;
    public GameObject Ref_CounterButton;
    public GameObject Ref_SellButton;
    public Image AvatarImage;
    public TMP_Text Ref_Money;
    public UI_FriendshipTab Ref_FriendshipTab;
    public TMP_Text Ref_MoneyGoal;
    public TMP_Text Ref_Day;
    public UI_MessagesBox MessagesBox;

    [Header("Prefabs")]
    public GameObject PF_MainMenu;
    public GameObject PF_LevelSelect;
    public GameObject PF_DialogueOption;
    public GameObject PF_ID;
    public GameObject PF_SSI;
    public GameObject PF_FriendshipTab;
    public GameObject PF_MessagesBox;

    [Header("Dialogue")]
    public TextMeshProUGUI npcText;
    public TextMeshProUGUI npcNameText;
    public Transform optionContainer;
    public GameObject NPCTextBox;

    private string lastNPCName = "";

    [SerializeField]
    private CanvasGroup avatarCanvasGroup;

    [SerializeField]
    private float avatarFadeSpeed = 0.7f;

    [SerializeField]
    private float nameTypingSpeed = 0.003f;

    private Coroutine nameCoroutine;





    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        if (PF_MainMenu != null)
        {
            Ref_MainMenu = Instantiate(PF_MainMenu, mainCanvas.transform).GetComponent<MainMenu>();
            Ref_MainMenu.LevelSelect.onClick.AddListener(ShowLevelSelect);
            Ref_MainMenu.Quit.onClick.AddListener(() => Debug.Log("Quit button clicked"));
            // Ref_MainMenu.gameObject.SetActive(false);
        }
        if (PF_LevelSelect != null)
        {
            Ref_LevelSelect = Instantiate(PF_LevelSelect, mainCanvas.transform).GetComponent<LevelSelect>();
            Ref_LevelSelect.Day1.onClick.AddListener(() => InitiateDay(1));
            Ref_LevelSelect.Day2.onClick.AddListener(() => InitiateDay(2));
            Ref_LevelSelect.Day3.onClick.AddListener(() => InitiateDay(3));
            Ref_LevelSelect.Day4.onClick.AddListener(() => InitiateDay(4));
            Ref_LevelSelect.Day5.onClick.AddListener(() => InitiateDay(5));
            Ref_LevelSelect.gameObject.SetActive(false);
        }
        if (PF_ID != null)
        {
            Ref_ID = Instantiate(PF_ID, mainCanvas.transform).GetComponent<UI_ID>();
            Ref_ID.gameObject.SetActive(false);
        }
        if (PF_FriendshipTab != null)
        {
            Ref_FriendshipTab = Instantiate(PF_FriendshipTab, mainCanvas.transform).GetComponent<UI_FriendshipTab>();
            Ref_FriendshipTab.gameObject.SetActive(false);
        }
            if (PF_MessagesBox != null)
            {
                MessagesBox = Instantiate(PF_MessagesBox, mainCanvas.transform).GetComponent<UI_MessagesBox>();
                MessagesBox.gameObject.SetActive(false);
        }
    }

    public void ToggleIDentification_ID()
    {
        if (Ref_ID.gameObject.activeSelf)
        {
            Ref_ID.gameObject.SetActive(false);
        }
        else
        {
            Ref_ID.gameObject.SetActive(true);
            Ref_ID.container.gameObject.SetActive(true);
            var currentNPC = DialogueManager.Instance.currentNPC;
            Ref_ID.Init(currentNPC.data.avatar, currentNPC.data.name, currentNPC.data.age, currentNPC.data.height, currentNPC.data.eyeColor, currentNPC.data.gender, currentNPC.data.address, currentNPC.data.expiryDate);
        }
    }

    public void ToggleIdentification_SSI()
    {
        if (Ref_SSI == null)
        { Ref_SSI = Instantiate(PF_SSI, mainCanvas.transform).GetComponent<SSIManager>(); }
        else
        {
            Destroy(Ref_SSI.gameObject);
        }
    }

    public void ToggleWares()
    {
        if (Ref_Wares.activeSelf)
        {
            Ref_Wares.SetActive(false);
            Ref_CounterButton.SetActive(false);
        }
        else
        {
            Ref_Wares.SetActive(true);
            Ref_CounterButton.SetActive(true);
        }
    }

    public void ToggleFriendshipTab()
    {
        if (Ref_FriendshipTab.gameObject.activeSelf)
        {
            Ref_FriendshipTab.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Toggling Friendship Tab ON");
            Ref_FriendshipTab.gameObject.SetActive(true);
            Ref_FriendshipTab.currentFriend = Ref_FriendshipTab.friends[0];
            Ref_FriendshipTab.PopulateFriend(Ref_FriendshipTab.currentFriend);
        }
    }

    public void ToggleMessagesBox()
    {
        if (MessagesBox.gameObject.activeSelf)
        {
            MessagesBox.gameObject.SetActive(false);
        }
        else
        {
            MessagesBox.gameObject.SetActive(true);
            MessagesBox.PopulateMessages();
        }
    }
    public void ShowLevelSelect()
    {
        Ref_MainMenu.gameObject.SetActive(false);
        
        Ref_LevelSelect.gameObject.SetActive(true);
        Debug.Log("Level Select menu shown.");
    }

    public void HideMenus()
    {
        Ref_MainMenu.gameObject.SetActive(false);
        Ref_LevelSelect.gameObject.SetActive(false);
    }

    public void InitiateDay(int day)
    {
        HideMenus();
        NPCTextBox.SetActive(true);
        DialogueManager.Instance.StartDay(day);
        Debug.Log($"Starting Day {day}...");
        // Additional logic to start the day can be added here
    }

    public void SetMoneyGoal(int goal)
    {
        Ref_MoneyGoal.text = goal.ToString();
    }

    public void SetDay(int day)
    {
        Ref_Day.text = day.ToString();
    }
    public void ShowNode(NodeData node)
    {
        npcText.text = node.text;

        string currentNPCName =
    DialogueManager.Instance.currentNPC.data.name;

        AvatarImage.sprite =
            DialogueManager.Instance.currentNPC.data.avatar;
        AvatarImage.preserveAspect = true;

        // New character transition
        if (lastNPCName != currentNPCName)
        {
            lastNPCName = currentNPCName;

            if (nameCoroutine != null)
            {
                StopCoroutine(nameCoroutine);
            }

            StartCoroutine(
                FadeInCharacter()
            );

            nameCoroutine =
                StartCoroutine(
                    TypeNPCName(currentNPCName)
                );
        }
        else
        {
            npcNameText.text = currentNPCName;
        }

        ClearOptions();

        foreach (OptionData option in node.options)
        {
            if (!DialogueManager.Instance
                .MeetsRequirements(option.requirements))
                continue;

            GameObject obj =
                Instantiate(
                    PF_DialogueOption,
                    optionContainer
                );

            DialogueOption dialogueOption =
                obj.GetComponent<DialogueOption>();

            dialogueOption.optionText.text =
                option.text;

            bool foundFriendshipEffect = false;

            foreach (EffectData effect in option.effects)
            {
                if (effect.variable == "friendship")
                {
                    foundFriendshipEffect = true;

                    int value =
                        int.Parse(effect.value);

                    if (value > 0)
                    {
                        dialogueOption.ToggleImage(
                            "positive"
                        );
                    }
                    else if (value < 0)
                    {
                        dialogueOption.ToggleImage(
                            "negative"
                        );
                    }
                    else
                    {
                        dialogueOption.ToggleImage(
                            "neutral"
                        );
                    }

                    break;
                }
            }

            if (!foundFriendshipEffect)
            {
                dialogueOption.ToggleImage(
                    "neutral"
                );
            }

            dialogueOption.button.onClick
                .AddListener(() =>
                {
                    DialogueManager.Instance
                .SelectOption(option);
                });
        }
    }
    IEnumerator TypeNPCName(string npcName)
    {
        npcNameText.text = "";

        foreach (char c in npcName)
        {
            npcNameText.text += c;

            yield return new WaitForSeconds(
                nameTypingSpeed
            );
        }
    }
    IEnumerator FadeInCharacter()
    {
        avatarCanvasGroup.alpha = 0f;

        while (avatarCanvasGroup.alpha < 1f)
        {
            avatarCanvasGroup.alpha +=
                Time.deltaTime * avatarFadeSpeed;

            yield return null;
        }

        avatarCanvasGroup.alpha = 1f;
    }

    public void ClearOptions()
    {
        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }
    }
    public void UpdateMoney(int amount)
    {
        Ref_Money.text = amount.ToString();
    }
}
