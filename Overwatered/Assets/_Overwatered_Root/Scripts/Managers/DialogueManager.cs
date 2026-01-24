using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Manager")]

    [SerializeField] GameObject dialoguePanel;
    //[SerializeField] GameObject dialogueSubpanel = null;
    //[SerializeField] TMP_Text dialoguerName = null;
    [SerializeField] TMP_Text dialogueText;
    [Tooltip("Do not reference, unless it is a minigame/is not area activated")]
    public DialogueActivator currentDialoguer;

    float typingTime;
    bool didDialogueStart;
    int lineIndex;
    public bool dialogueOver;
    bool customLine;

    //falta impedir que se muevan

    private void Awake()
    {
        didDialogueStart = false;
        dialogueOver = true;
    }

    private void StartDialogue()
    {
        didDialogueStart = true;
        dialogueOver = false;
        dialoguePanel.SetActive(true);
        if(currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark != null) currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark.SetActive(false);
        lineIndex = 0;
        //show emotion and name of the person
        StartCoroutine(ShowLine());
    }
    private void NextDialogueLine()
    {
        lineIndex++;
        if(lineIndex < currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLines.Length)
        {
            //mostrar emoción correspondiente

            //si el objeto para el nombre no es null, sale el nombre 
            StartCoroutine(ShowLine());
        }
        else
        {
            if(currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueStopper)
            {
                didDialogueStart = false;
                dialoguePanel.SetActive(false);
                if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark != null) currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueMark.SetActive(true);
                dialogueOver = true;
                if (currentDialoguer.dialogueInfo.Length > 1 && currentDialoguer.dialogueInfo.Length < currentDialoguer.lineToRead) currentDialoguer.lineToRead++; // o esto tmb se cambiará por NPC AI
                if (currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].willGame)
                {
                    StartCoroutine(MinigameManager.Instance.EnterMinigame(currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].gameScene, false, currentDialoguer.activatorReference));
                }
            }
            else
            {
                //sigue a siguiente linea como si fuera otro diálogo
            }
        }
    }
    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLines[lineIndex];

        foreach(char ch in currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLines[lineIndex])
        {
            //dialogueText.text += ch;
            dialogueText.maxVisibleCharacters ++;
            yield return new WaitForSeconds(typingTime);
        }
    }

    public void DialogueCall()
    {
        Debug.Log("Dialogue 2");
        if (!didDialogueStart)
        {
            StartDialogue();
            Debug.Log("Start");
        }
        else if (dialogueText.maxVisibleCharacters == currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLines[lineIndex].Length)
        {
            NextDialogueLine();
            Debug.Log("Next");
        }
        else
        {
            StopAllCoroutines();
            Debug.Log("End");

            //else se queda en esa útlima/ se resetea a 0
            dialogueText.maxVisibleCharacters = currentDialoguer.dialogueInfo[currentDialoguer.lineToRead].dialogueLines[lineIndex].Length;
            //dialogueText.text = dialogueLines[lineIndex];
        }
    }

}
