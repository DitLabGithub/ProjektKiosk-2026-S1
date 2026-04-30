using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    public Canvas mainCanvas;
    public MainMenu Ref_MainMenu;
    public LevelSelect Ref_LevelSelect;

    [Header("Prefabs")]
    public GameObject PF_MainMenu;
    public GameObject PF_LevelSelect;
    public GameObject PF_DialogueOption;

    [Header("Dialogue")]
    public TMP_Text npcText;
    public TMP_Text npcNameText;
    public Transform optionContainer;
    public GameObject NPCTextBox;





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

    public void ShowNode(NodeData node)
    {
        npcText.text = node.text;

        npcNameText.text = DialogueManager.Instance.currentNPC.data.name;

        ClearOptions();

        foreach (OptionData option in node.options)
        {

            if (!DialogueManager.Instance.MeetsRequirements(option.requirements))
                continue;

            GameObject obj =
                Instantiate(PF_DialogueOption, optionContainer);

            DialogueOption dialogueOption =
                obj.GetComponent<DialogueOption>();

            dialogueOption.optionText.text = option.text;

            dialogueOption.button.onClick.AddListener(() =>
            {
                DialogueManager.Instance.SelectOption(option);
            });
        }
    }
    public void ClearOptions()
    {
        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
