using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueDetection : MonoBehaviour
{
    [Tooltip("Do not reference, unless it is a minigame/is not area activated")]
    [SerializeField]DialogueActivator npcActivator;
    [SerializeField] DialogueManager manager;
    bool playerInRange;

    private void Start()
    {
        if (GameManager.Instance.exitingGame)
        {
            GameManager.Instance.dialogueManager.currentDialoguer = GameManager.Instance.npcManager.npcDialogueActivator[GameManager.Instance.gameData.lastNPCNumber];
            //se da por hecho que esto solo se llama una vez (quien tenga ese activator)
            // o lose, 1 empate, 2 ganar
            if (GameManager.Instance.gameOutcome == 0) GameManager.Instance.dialogueManager.currentDialoguer.lineToRead = 1;
            else if (GameManager.Instance.gameOutcome == 1) GameManager.Instance.dialogueManager.currentDialoguer.lineToRead = 2;
            else if (GameManager.Instance.gameOutcome == 2) GameManager.Instance.dialogueManager.currentDialoguer.lineToRead = 3;
            GameManager.Instance.ChangeCamera();
            manager.DialogueCall();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 8 && !playerInRange)
        {
            npcActivator = other.gameObject.GetComponent<DialogueActivator>();
            playerInRange = true;
            manager.currentDialoguer = npcActivator;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            playerInRange = false;
            npcActivator = null;
            manager.currentDialoguer = null;
        }
    }
    public void OnInfo(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !GameManager.Instance.menuOpened && npcActivator != null)
        {
            if (!npcActivator.dialogueInfo[npcActivator.lineToRead].areaDialogue || (npcActivator.dialogueInfo[npcActivator.lineToRead].areaDialogue && playerInRange))
            {
                manager.DialogueCall();
            }
        }
    }
}
