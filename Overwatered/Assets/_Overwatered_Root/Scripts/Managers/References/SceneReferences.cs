using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneReferences : MonoBehaviour
{
    public ThirdPersonCamController camController; 
    public DialogueManager dialogueManager; 
    public NPCManager npcManager;
    public GameObject inventoryPanel;
    public CinemachineCamera cinemachineCamera;
    public Transform mapCamera;
    public Image fadePanel;
    public EventSystem eventSystem;

    private void Awake()
    {
        if(GameManager.Instance != null)GameManager.Instance.FindReferences();
    }
    #region Game States Methods
    public void StartLoading(int sceneToLoad)
    {
        GameManager.Instance.nextScene = sceneToLoad;
        GameManager.Instance.StartFade(1);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void DeleteGame()
    {
        GameManager.Instance.gameData.gameHasStarted = false;//y todo un método de reset
    }
    #endregion
}
