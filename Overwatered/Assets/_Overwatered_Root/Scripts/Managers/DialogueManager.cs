using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Manager")]

    [SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject selectionPanel;
    //[SerializeField] GameObject dialogueSubpanel = null;
    //[SerializeField] TMP_Text dialoguerName = null;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] TMP_Text options1Text;
    [SerializeField] TMP_Text options2Text;
    [SerializeField] TMP_Text options3Text;
    [Tooltip("Do not reference, unless it is a minigame/is not area activated")]
    public DialogueActivator currentDialoguer;

    float typingTime;
    bool didDialogueStart;
    int lineIndex;
    int dialogueIndex; //0 greeting, 1 mainpart, 2 goodbye
    public bool dialogueOver;
    bool randomized;
    int randomNumber;
    string textToRead;
    [SerializeField] int choiceStatus; //0 choice asked, 1 choice made,2 answer given
    [SerializeField] int optionChosen;

    //falta impedir que se muevan

    private void Awake()
    {
        didDialogueStart = false;
        dialogueOver = true;
    }

    private void StartDialogue()
    {
        GameManager.Instance.playerInDialogue = true;
        choiceStatus = 0;
        if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].areaDialogue) GameManager.Instance.ChangeCamera();
        GameManager.Instance.SetNPCTarget(currentDialoguer.gameObject.transform);
        didDialogueStart = true;
        dialogueOver = false;
        dialoguePanel.SetActive(true);
        if(currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark != null) currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark.SetActive(false);
        lineIndex = dialogueIndex = 0;
        //show emotion and name of the person
        ShowLine();
    }
    private void NextDialogueLine()
    {
        lineIndex++;
        dialogueIndex++;
        if (!randomized)
        {
            if (lineIndex < currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLinesDefault.Length && choiceStatus == 0)
            {
                //mostrar emoción correspondiente

                //si el objeto para el nombre no es null, sale el nombre 
                ShowLine();
            }
            else
            {
                if(currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].multiOption)//te enseña las opciones
                {
                    if(choiceStatus == 0)
                    {

                        choiceStatus = 1;
                        selectionPanel.SetActive(true);
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        options1Text.text = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueOptions1;
                        options2Text.text = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueOptions2;
                        options3Text.text = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueOptions3;
                        //empezar métodos de multiopción, si ya ha acabado -> CloseDialogue()
                    }
                    else if(choiceStatus == 2)
                    {
                        if ((optionChosen == 0 && lineIndex < currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].options1Answer.Length)
                            || (optionChosen == 1 && lineIndex < currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].options2Answer.Length)
                            || (optionChosen == 2 && lineIndex < currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].options3Answer.Length))
                        {
                            ShowLine();
                        }
                        else
                        {
                            CloseDialogue();
                        }

                    }
                }
                else
                {
                    if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueStopper)
                    {
                        CloseDialogue();
                    }
                    else//esto comprobar
                    {
                        currentDialoguer.lineToRead++;
                        lineIndex = 0;
                        ShowLine();
                        //sigue a siguiente linea como si fuera otro diálogo
                    }
                }
            }
        }
        else
        {
            //if(dialogueIndex == 0 && lineIndex < currentDialoguer.greetings[randomNumber].Length))
            if(dialogueIndex >= 3)
            {
                didDialogueStart = false;
                dialoguePanel.SetActive(false);
                if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark != null) currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark.SetActive(true);
                dialogueOver = true;
                if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].areaDialogue && !currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].willGame) GameManager.Instance.ChangeCamera();
                GameManager.Instance.playerInDialogue = false;
            }
            else
            {
                ShowLine();
            }
        }
    }
    private void ShowLine()
    {
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;
        if(randomized)
        {
            if(dialogueIndex == 0) randomNumber = Random.Range(0, currentDialoguer.greetings.Length);
            else if(dialogueIndex == 1) randomNumber = Random.Range(0, currentDialoguer.funFacts.Length);
            else if(dialogueIndex == 2) randomNumber = Random.Range(0, currentDialoguer.goodbyes.Length);
            if (dialogueIndex == 0) textToRead = currentDialoguer.greetings[randomNumber];
            else if (dialogueIndex == 1) textToRead = currentDialoguer.funFacts[randomNumber];
            else if (dialogueIndex == 2) textToRead = currentDialoguer.goodbyes[randomNumber];
        }
        else
        {
            if(!currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].multiOption) textToRead = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLinesDefault[lineIndex];
            else
            {
                if(choiceStatus == 0)
                {
                    textToRead = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLinesDefault[lineIndex];
                }
                else
                {
                    if (optionChosen == 0)
                    {
                        textToRead = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].options1Answer[lineIndex];
                    }
                    else if (optionChosen == 1)
                    {
                        textToRead = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].options2Answer[lineIndex];
                    }
                    else if (optionChosen == 2)
                    {
                        textToRead = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].options3Answer[lineIndex];
                    }
                }
                
            }
        }

        dialogueText.text = textToRead;
        StartCoroutine(ShowingLine());
    }
    IEnumerator ShowingLine()
    {
        foreach (char ch in textToRead)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(typingTime);
        }
    }
    void CloseDialogue()
    {
        choiceStatus = 0;
        didDialogueStart = false;
        dialoguePanel.SetActive(false);
        if(selectionPanel != null)selectionPanel.SetActive(false);
        if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark != null) currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark.SetActive(true);
        dialogueOver = true;
        if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].willGame && optionChosen == 0)
        {
            StartCoroutine(MinigameManager.Instance.EnterMinigame(currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].gameScene, false, currentDialoguer.activatorReference));
            optionChosen = 0;
        }
        else
        {
            if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].areaDialogue) GameManager.Instance.ChangeCamera();
            optionChosen = 0;
        }
        
        if (currentDialoguer.dialogueInfo.Length > 1 && currentDialoguer.lineToRead < currentDialoguer.dialogueInfo.Length) currentDialoguer.lineToRead++; // o esto tmb se cambiará por NPC AI
        if (GameManager.Instance.gameOutcome >= 0) currentDialoguer.lineToRead = 0;
        GameManager.Instance.playerInDialogue = false;
    }
    public void DialogueCall()
    {
        Debug.Log("Call");
        if(choiceStatus != 1)//si no estás con el panel de selection abierto
        {
            DialoguerOrder();
            if (!didDialogueStart)
            {
                StartDialogue();
            }
            else if (dialogueText.maxVisibleCharacters == textToRead.Length)
            {
                NextDialogueLine();
            }
            else
            {
                StopAllCoroutines();

                //else se queda en esa útlima/ se resetea a 0
                dialogueText.maxVisibleCharacters = textToRead.Length;
                //dialogueText.text = dialogueLines[lineIndex];
            }
        }
        
    }
    public void ChooseOption(int optionNumber)//poner en botones
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        choiceStatus = 2;
        lineIndex = -1;
        optionChosen = optionNumber;
        DialogueCall();
    }
    void DialoguerOrder()
    {
        if(currentDialoguer.isRandom)
        {
            randomized = true;
        }
        else
        {
            randomized = false;
        }
    }


}
