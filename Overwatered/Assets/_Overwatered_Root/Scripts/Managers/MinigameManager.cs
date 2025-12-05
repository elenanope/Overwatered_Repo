using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
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

    public bool isInMinigame;
    public int lastMinigameResult; // o lose, 1 empate, 2 ganar
    public int minigameScene;//mismo orden que las escenas: 0 main menu, 1 normal, 2 papership


    private void Awake()
    {
        instance = this; 
        DontDestroyOnLoad(this.gameObject);
    }


    public IEnumerator EnterMinigame(int minigame, bool hasSpecialObject, Transform npcTransform) //mirar si en el inventario llevas un objeto especial para ese minijuego
    {
        lastMinigameResult = -1;
        minigameScene = minigame;
        isInMinigame = true;
        GameManager.Instance.StartFade(1);
        yield return new WaitForSeconds(GameManager.Instance.fadeTime);
        SceneManager.LoadScene(minigameScene);
        GameManager.Instance.FindReferences();
    }
    
    public IEnumerator ExitMinigame(int endResult) // + int gameNumber?
    {
        lastMinigameResult = endResult;
        isInMinigame = false;
        GameManager.Instance.StartFade(1);
        yield return new WaitForSeconds(GameManager.Instance.fadeTime);
        SceneManager.LoadScene(1);
        GameManager.Instance.FindReferences();
    }
}
