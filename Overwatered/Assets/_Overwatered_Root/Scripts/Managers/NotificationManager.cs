using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] GameObject[] messages;
    [SerializeField] GameObject phoneWithChat;
    bool firstMessage;
    private void Start()
    {//meter alarma por inundación en el propio main menu
        firstMessage = true;
        //if(!GameManager.Instance.gameData.gameHasStarted)StartCoroutine(SendMessages());
        StartCoroutine(SendMessages());
    }

    IEnumerator SendMessages()
    {
        for (int i = 0; i < messages.Length; i++)
        {
            if(!firstMessage) yield return new WaitForSeconds(Random.Range(4f, 9f));
            else
            {
                yield return new WaitForSeconds(Random.Range(2f, 4f));
                firstMessage = false;
            }
            //+ sonido
            messages[i].SetActive(true);
            StartCoroutine(TurnOff(i));
        }
    }
    IEnumerator TurnOff(int index)
    {
        yield return new WaitForSeconds(14f);
        messages[index].SetActive(false);
        if(index == messages.Length - 1) phoneWithChat.SetActive(true);
    }    
    /*
     
    Hey, it's mom!
    
    Don't worry darling, we're safe here.
    No need to come here!

    Though I think your area may be in a bit of danger...

    Just to be safe, head to the <b> dock </b>, as you know, where the shops are.

    The flags mark the way, remember?

    Don't forget to stay <b>hydrated</b>
and <b>sate</b>!! XXX

    */
}
