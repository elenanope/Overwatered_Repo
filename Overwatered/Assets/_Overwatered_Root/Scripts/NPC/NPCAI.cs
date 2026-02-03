using UnityEngine;

public class NPCAI : MonoBehaviour
{
    [SerializeField] bool isSuggestingMinigame;
    [SerializeField] bool isInDialogue = false;

    public void Talk(Transform playerTransform)
    {
        //se rotará este personaje
        // otra cámara transicionará de la posición de la anterior a un punto medio entre ambos personajes donde puedas ver al npc a la derecha (ej.) y la cámara esté detrás de ti?
        //Debug.Log($"Soy {name} y te estoy hablando :)");

        //si te va a invitar a un minijuego, diálogo especial

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
