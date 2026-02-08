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
    //poner los paneles en el player para que puedan ser privados?
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject inventoryPanel;
    public CinemachineCamera cinemachineCamera;
    public Camera cameraComponent;
    public CinemachineCamera dialogueCam;
    public CinemachineTargetGroup targetGroup;
    public CinemachineRotationComposer dialogueCamRot;
    public Transform mapCamera;
    public Image fadePanel;
    public EventSystem eventSystem;
    public Transform head1Trans;

    private void Awake()
    {
        if(GameManager.Instance != null)GameManager.Instance.FindReferences();
    }
    private void Start()
    {
        //GameManager.Instance.StartFade(0);
    }
    #region Game States Methods [move to other script?]
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
