using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Manager")]
    [SerializeField] GameObject dialogueMark = null;
    //[SerializeField] DialogueActivator dialoguerInfo = null;

    bool playerInRange;
    [Tooltip("Mark this if your dialogue appears only when player is in range")]
    [SerializeField] bool areaDialogue;

    [SerializeField] GameObject dialoguePanel;
    //[SerializeField] GameObject dialogueSubpanel = null;
    //[SerializeField] TMP_Text dialoguerName = null;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField, TextArea(4, 6)] string[] dialogueLines;

    float typingTime;
    bool didDialogueStart;
    int lineIndex;
    public bool dialogueOver;

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
        dialogueMark.SetActive(false);
        lineIndex = 0;
        //show emotion and name of the person
        StartCoroutine(ShowLine());
    }
    private void NextDialogueLine()
    {
        lineIndex++;
        if(lineIndex < dialogueLines.Length)
        {
            StartCoroutine(ShowLine());
        }
        else
        {
            didDialogueStart = false;
            dialoguePanel.SetActive(false);
            dialogueMark.SetActive(true);
            dialogueOver = true;
        }
    }

    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = dialogueLines[lineIndex];

        foreach(char ch in dialogueLines[lineIndex])
        {
            //dialogueText.text += ch;
            dialogueText.maxVisibleCharacters ++;
            yield return new WaitForSeconds(typingTime);
        }
    }

    public void OnInfo(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if(!areaDialogue || (areaDialogue && playerInRange))
            if(!didDialogueStart)
            {
                StartDialogue();
            }
            else if(dialogueText.maxVisibleCharacters == dialogueLines[lineIndex].Length)
            {
                NextDialogueLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText.maxVisibleCharacters = dialogueLines[lineIndex].Length;
                //dialogueText.text = dialogueLines[lineIndex];
            }
        }
    }

    /*#region Dialogue Management
    void StartDialogue()
    {
        didDialogueStart = true;
        dialoguePanel.SetActive(true);
        StartCoroutine(ShowLine());
    }

    void NextDialogueLine()
    {
        didDialogueStart = false;
        dialogueOver = true;
        dialoguePanel.SetActive(false);
    }

    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;

        foreach (char ch in dialogueLines[lineIndex])
        {
            dialogueText.text += ch;
            yield return new WaitForSecondsRealtime(typingTime);
        }
        if (lineIndex < 5 || lineIndex == 6) yield return new WaitForSecondsRealtime(2);
        else yield return new WaitForSecondsRealtime(1);
        NextDialogueLine();
    }

    public void Dialogue()
    {
        if (!didDialogueStart)
        {
            StopCoroutine(ShowLine());
            StartDialogue();
        }
        else
        {
            StopCoroutine(ShowLine());
            StartDialogue();
        }
    }
    #endregion
    public void ShowNotification(bool isPositive)
    {
        Debug.Log("se llega aqui");
        //StartCoroutine(ActivateText(isPositive));
    }
    /*IEnumerator ActivateText(bool isPositive)
    {
        if (isPositive) pointsText.SetActive(true);
        else strikesText.SetActive(true);
        yield return new WaitForSeconds(2);
        if (isPositive) pointsText.SetActive(false);
        else strikesText.SetActive(false);
    }*/
}
