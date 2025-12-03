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
                Debug.Log("GameManager is null!");
            }
            return instance;
        }
    }

    public bool isInMinigame;
    public bool waslastGameWon;
    public int minigameNumber;//mismo orden que las escenas: 0 main menu, 1 normal, 2 papership

    public void EnterMinigame(int minigame, bool hasSpecialObject) //mirar si en el inventario llevas un objeto especial para ese minijuego
    {
        waslastGameWon = false;
        minigameNumber = minigame;
        SceneManager.LoadScene(minigameNumber);
    }
    
    public void ExitMinigame(bool hasWon) // + int gameNumber?
    {
        waslastGameWon = hasWon;
        SceneManager.LoadScene(1);
    }
}
