using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool isInMinigame;
    public int lastMinigameResult; // o lose, 1 empate, 2 ganar
    public int minigameScene;//mismo orden que las escenas: 0 main menu, 1 normal, 2 papership
    


    private void Awake()
    {
        instance = this; 
        DontDestroyOnLoad(this.gameObject);
    }

    public void EnterMinigame(int minigame, bool hasSpecialObject, Transform npcTransform) //mirar si en el inventario llevas un objeto especial para ese minijuego
    {
        lastMinigameResult = -1;
        minigameScene = minigame;
        SceneManager.LoadScene(minigameScene);
    }
    
    public void ExitMinigame(int endResult) // + int gameNumber?
    {
        lastMinigameResult = endResult;
        SceneManager.LoadScene(1);
        GameManager.Instance.FindReferences();
    }
}
