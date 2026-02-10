using UnityEngine;

public class NPCAI : MonoBehaviour
{
    [SerializeField] bool isSuggestingMinigame;
    [SerializeField] bool isInDialogue = false;

    public void Talk(Transform playerTransform)
    {
        if (!isSuggestingMinigame)
        {

        }
        else
        {
            if(!isInDialogue)
            {
                isInDialogue = true;
                //StartCoroutine(MinigameManager.Instance.EnterMinigame(2, false, gameObject.GetComponent<DialogueActivator>()));//poner luego otra opción si sí tiene papel especial
                MinigameUpdater.Instance.SavePos(playerTransform);
            }
        }
    }
}
