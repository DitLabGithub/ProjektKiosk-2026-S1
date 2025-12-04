using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startScreen;
    public GameObject dialogue1;
    public GameObject dialogue2;
    public GameObject dialogue3;

    [Header("Fade & Typewriter")]
    public FadeController fadeController;
    public TypeWriter typewriter;

    [Header("Dialogue Text Fields")]
    public TMP_Text dialogue1Top;
    public TMP_Text dialogue1Bottom;
    public TMP_Text dialogue2Top;
    public TMP_Text dialogue2Bottom;
    public TMP_Text dialogue3Top;
    public TMP_Text dialogue3Bottom;

    [Header("Custom Font")]
    public TMP_FontAsset customFont;

    private int currentDialogueIndex = 0;
    private bool canClickToContinue = false;

    // Hardcoded multiline dialogue strings
    private readonly string dialogue1TopText = "The year is 2030.\n A new information age is upon us. With that, the rise of the New European Union.";
    private readonly string dialogue1BottomText = "Corporations and organizations harvest and control personal data in every way they can.";

    private readonly string dialogue2TopText = "Everything is digitalized - even your identity.\n Digital ID's are offered by companies that collect data, and local governments that use them as a form of surveillance.";
    private readonly string dialogue2BottomText = "NEU citizens have been offered a new \"SSI\" ID. \n An alternative that grants them control over their data. How this affects daily life is yet to be seen.";

    private readonly string dialogue3TopText = "You work at a humble kiosk shop.\n These kiosks are one of the last few places citizens can shop in without companies and organizations keeping tabs on what they buy.";
    private readonly string dialogue3BottomText = "You've just returned from your time off during the holidays...";

    void Start()
    {
        AssignCustomFont();
        ClearAllDialogueTexts();

        startScreen.SetActive(true);
        dialogue1.SetActive(false);
        dialogue2.SetActive(false);
        dialogue3.SetActive(false);
    }

    void AssignCustomFont()
    {
        TMP_Text[] allTexts = {
            dialogue1Top, dialogue1Bottom,
            dialogue2Top, dialogue2Bottom,
            dialogue3Top, dialogue3Bottom
        };

        foreach (var t in allTexts)
        {
            if (t != null && customFont != null)
                t.font = customFont;
        }
    }

    void ClearAllDialogueTexts()
    {
        dialogue1Top.text = "";
        dialogue1Bottom.text = "";
        dialogue2Top.text = "";
        dialogue2Bottom.text = "";
        dialogue3Top.text = "";
        dialogue3Bottom.text = "";
    }

    public void StartGame()
    {
        // Play button click sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGenericButtonClick();
        }

        startScreen.SetActive(false);
        currentDialogueIndex = 1;
        StartCoroutine(ShowDialogueSequence(currentDialogueIndex));

    }

    IEnumerator ShowDialogueSequence(int dialogueNumber)
    {
        // Show the correct dialogue panel
        dialogue1.SetActive(dialogueNumber == 1);
        dialogue2.SetActive(dialogueNumber == 2);
        dialogue3.SetActive(dialogueNumber == 3);

        TMP_Text topUI = null;
        TMP_Text bottomUI = null;
        string topString = "", bottomString = "";
     

        switch (dialogueNumber)
        {
            case 1:
                topUI = dialogue1Top;
                bottomUI = dialogue1Bottom;
                topString = dialogue1TopText;
                bottomString = dialogue1BottomText;
                break;
            case 2:
                topUI = dialogue2Top;
                bottomUI = dialogue2Bottom;
                topString = dialogue2TopText;
                bottomString = dialogue2BottomText;
                break;
            case 3:
                topUI = dialogue3Top;
                bottomUI = dialogue3Bottom;
                topString = dialogue3TopText;
                bottomString = dialogue3BottomText;
                break;
        }

        // Assign full text before typing (text cleared in Typewriter)
        //topUI.text = topString;
        //bottomUI.text = bottomString;

        yield return fadeController.FadeOut(1f);
        yield return typewriter.TypeTwoTextsFromTMP(topUI, topString, bottomUI, bottomString);
        canClickToContinue = true;
    }

    void Update()
    {
        if (canClickToContinue && Input.GetMouseButtonDown(0))
        {
            canClickToContinue = false;
            if (currentDialogueIndex < 3)
            {
                currentDialogueIndex++;
                StartCoroutine(FadeToNextDialogue(currentDialogueIndex));
            }
            else
            {
                StartCoroutine(FadeOutAndLoadScene());
            }
        }
    }

    IEnumerator FadeToNextDialogue(int dialogueNumber)
    {
        yield return fadeController.FadeIn(1f);

        dialogue1.SetActive(false);
        dialogue2.SetActive(false);
        dialogue3.SetActive(false);

        yield return ShowDialogueSequence(dialogueNumber);
    }

    IEnumerator FadeOutAndLoadScene()
    {
        yield return fadeController.FadeIn(1f);
        SceneManager.LoadScene("TutorialScene"); //TESTING: Skip tutorial, go directly to gameplay
    }

    public void ExitGame()
    {
        // Play button click sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGenericButtonClick();
        }

        Application.Quit();
    }
}
