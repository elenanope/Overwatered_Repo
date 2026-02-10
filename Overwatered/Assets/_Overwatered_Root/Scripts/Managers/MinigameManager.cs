using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    private static MinigameManager instance;

    public static MinigameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("MinigameManager is null!");
            }
            return instance;
        }
    }

    public int lastMinigameResult; // o lose, 1 empate, 2 ganar
    public int minigameScene;//mismo orden que las escenas: 0 main menu, 1 normal, 2 papership

    private void Awake()
    {
        instance = this; 
        DontDestroyOnLoad(this.gameObject);
    }


    public void EnterMinigame(int minigame, bool hasSpecialObject, int currentActivator) //mirar si en el inventario llevas un objeto especial para ese minijuego
    {
        
        GameManager.Instance.gameData.lastNPCNumber = currentActivator;
        lastMinigameResult = -1;
        minigameScene = minigame;
        GameManager.Instance.StartFade(1);//unificar esto (en gameManager en el futuro)
        GameManager.Instance.nextScene = minigameScene;
    }
    
    public void ExitMinigame(int endResult) // + int gameNumber?
    {
        GameManager.Instance.gameOutcome = endResult;
        if(endResult == 2) GameManager.Instance.inventoryData.pickedUpObjects[0] = true;
        GameManager.Instance.menuOpened = false;//quitar?
        GameManager.Instance.StartFade(1);
        GameManager.Instance.exitingGame = true;
        GameManager.Instance.nextScene = 1;

    }
}
