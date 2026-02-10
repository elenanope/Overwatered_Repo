using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                Debug.Log("GameManager is null!");
            }
            return instance;
        }
    }

    public SceneReferences sceneReferences;
    public SO_GameData gameData;
    public DialogueManager dialogueManager;
    public NPCManager npcManager;

    public ThirdPersonCamController camController;
    //poner los paneles en el player para que puedan ser privados?
    public GameObject inventoryPanel;
    public CinemachineCamera cinemachineCamera;
    public EventSystem eventSystem;
    public Transform mapCamera;
    public bool menuOpened;
    public bool tradeMode;
    public bool playerInDialogue;
    public bool isEating;
    public bool gameOver;

    [SerializeField] Image fadePanel;
    public float fadeTime = 2f;
    public bool faded;
    public bool fading;
    public bool exitingGame;
    public bool cameraReady;//camera ready to transition into dialogueCam
    public int gameOutcome = -1;
    public int nextScene = -1;
    int goalAlpha;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);//al volver a la escena inicial ya no va bien
        }
    }
    private void Start()
    {
        cameraReady = true;
        StartFade(0);
    }
    private void Update()
    {
        if(fading)
        {
            if(!faded)
            {
                Fade();
            }
            else
            {
                fading = false;
                faded = false;
                if (nextScene >= 0)StartCoroutine(LoadScene(nextScene));
                if (goalAlpha == 0) fadePanel.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
        if(sceneToLoad == 0)
        {
            Debug.Log("Reset");
            gameOver = false;
            Time.timeScale = 1.0f;
            cameraReady = true;
        }
        else if(sceneToLoad == 1)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        nextScene = -1;
        yield return new WaitForSeconds(1f);
        StartFade(0);
    }
    public void ChangeCamera()
    {
        CameraManager.Instance.LerpBetweenCameras();
    }
    public void SetNPCTarget(int npcTrans)
    {
        CameraManager.Instance.head2 = npcManager.npcHeads[npcTrans].position;
    }
    public void FindReferences()
    {
        GameObject.Find("References").TryGetComponent(out sceneReferences);
        if(sceneReferences != null)
        {
            if(sceneReferences.camController != null) camController = sceneReferences.camController;
            if (sceneReferences.inventoryPanel != null) inventoryPanel = sceneReferences.inventoryPanel;
            if (sceneReferences.cinemachineCamera != null) cinemachineCamera = sceneReferences.cinemachineCamera;
            if (sceneReferences.fadePanel != null) fadePanel = sceneReferences.fadePanel;
            if (sceneReferences.mapCamera != null) mapCamera = sceneReferences.mapCamera;
            if (sceneReferences.eventSystem != null) eventSystem = sceneReferences.eventSystem;
            if (sceneReferences.dialogueManager != null) dialogueManager = sceneReferences.dialogueManager;
            if (sceneReferences.npcManager != null) npcManager = sceneReferences.npcManager;
            //StartFade(0);
        }
        else
        {
            Debug.Log("References were not found");
        }
    }

    public void StartFade(int desiredAlpha)
    {
        fadePanel.gameObject.SetActive(true);
        goalAlpha = desiredAlpha;
        fading = true;
        faded = false;
    }
    void Fade()
    {
        float currentAlpha = fadePanel.color.a;

        currentAlpha = Mathf.MoveTowards(currentAlpha, goalAlpha, fadeTime * Time.deltaTime);

        fadePanel.color = new Vector4(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, currentAlpha);
        if (currentAlpha == goalAlpha)
        {
            faded = true;
        }
    }
}
