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
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject inventoryPanel;
    public CinemachineCamera cinemachineCamera;
    [SerializeField] Camera cameraComponent;
    [SerializeField] CinemachineCamera dialogueCam;
    [SerializeField] CinemachineRotationComposer dialogueCamRot;
    public EventSystem eventSystem;
    public Transform mapCamera;
    public bool menuOpened;
    public bool tradeMode;
    public bool playerInDialogue;
    public bool isEating;
    public bool gameOver;
    bool overworldCamActive = true;
    bool charactersHidden = false;
    float actualXOffset;
    float actualZOffset;

    [SerializeField] Image fadePanel;
    public float fadeTime = 2f;
    public bool faded;
    public bool fading;
    public bool exitingGame;
    public int gameOutcome = -1;
    int nextScene = -1;
    int goalAlpha;

    [Header("Dialogue Camera")]
    [SerializeField] Transform head1Trans;
    [SerializeField] Transform head2Trans;
    [SerializeField] float frontDistance;
    [SerializeField] float verticalWeight;
    Vector3 head1;
    Vector3 head2;
    Vector2 head1Top;
    Vector2 head2Top;
    [SerializeField] Vector3 middlePoint;
    [SerializeField] Vector3 cameraPoint;
    [SerializeField] float angles;
    [SerializeField] GameObject cameraDialogue;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
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
                if (nextScene >= 0) LoadScene(nextScene);
                if (goalAlpha == 0) fadePanel.gameObject.SetActive(false);
            }
        }
    }
    public void ChangeCamera()
    {
        StartCoroutine(ChangeCamCoroutine());
    }
     IEnumerator ChangeCamCoroutine()
    {
        if (!overworldCamActive)
        {
            cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = true;
            cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = true;
            charactersHidden = false;
            actualXOffset = 0f;
            actualZOffset = 0f;
            dialogueCamRot.TargetOffset.x = 0f;
            dialogueCamRot.TargetOffset.y = 0.83f;
            dialogueCamRot.TargetOffset.z = 0f;
            dialogueCam.Priority = 0;
            cinemachineCamera.Priority = 1;
            overworldCamActive = !overworldCamActive;
            yield break;
        }
        else//la segunda vez va raro
        {
            cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = false;
            cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = false;
            dialogueCam.Priority = 1;
            cinemachineCamera.Priority = 0;
            yield return new WaitForSeconds(0.5f);
            DialogueDistance();

            overworldCamActive = !overworldCamActive;
            yield break;
        }
    }
    public void SetNPCTarget(int npcTrans)
    {
        Debug.Log(npcTrans);
        head2 = npcManager.npcHeads[npcTrans].position;
    }
    public void DialogueDistance()//close FOV 9 for dialogueCam
    {
        Vector2 direction;
        Vector2 tempDifference;
        Vector2 cameraTemp;
        Vector2 middleTemp;
        Vector3 lookDir;

        head1 = head1Trans.position;

        head1Top = new Vector2(head1.x, head1.z);
        head2Top = new Vector2(head2.x, head2.z);
        tempDifference = head2Top - head1Top;

        middleTemp = new Vector2((head1Top.x + head2Top.x) / 2, (head1Top.y + head2Top.y) / 2);
        middlePoint = new Vector3(middleTemp.x, 0f, middleTemp.y);
        direction = new Vector2(tempDifference.y, -tempDifference.x).normalized;

        cameraTemp = middleTemp + direction * frontDistance;
        cameraPoint = new Vector3(cameraTemp.x, (head1.y + head2.y) / 2 + verticalWeight * Mathf.Abs(head1.y - head2.y), cameraTemp.y);

        dialogueCam.gameObject.transform.position = cameraPoint;
        lookDir = middlePoint - dialogueCam.gameObject.transform.position;
        lookDir.y = 0f;
        dialogueCam.gameObject.transform.rotation = Quaternion.LookRotation(lookDir);
    }
    public void FindReferences()
    {
        GameObject.Find("References").TryGetComponent(out sceneReferences);
        if(sceneReferences != null)
        {
            if(sceneReferences.camController != null) camController = sceneReferences.camController;
            if (sceneReferences.winPanel != null) winPanel = sceneReferences.winPanel;
            if (sceneReferences.losePanel != null) losePanel = sceneReferences.losePanel;
            if (sceneReferences.inventoryPanel != null) inventoryPanel = sceneReferences.inventoryPanel;
            if (sceneReferences.cinemachineCamera != null) cinemachineCamera = sceneReferences.cinemachineCamera;
            if (sceneReferences.cameraComponent != null) cameraComponent = sceneReferences.cameraComponent;
            if (sceneReferences.dialogueCam != null) dialogueCam = sceneReferences.dialogueCam;
            if (sceneReferences.dialogueCamRot != null) dialogueCamRot = sceneReferences.dialogueCamRot;
            if (sceneReferences.fadePanel != null) fadePanel = sceneReferences.fadePanel;
            if (sceneReferences.mapCamera != null) mapCamera = sceneReferences.mapCamera;
            if (sceneReferences.eventSystem != null) eventSystem = sceneReferences.eventSystem;
            if (sceneReferences.dialogueManager != null) dialogueManager = sceneReferences.dialogueManager;
            if (sceneReferences.npcManager != null) npcManager = sceneReferences.npcManager;
            StartFade(0);
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
    #region Game States Methods [move to other script?]
    public void StartLoading(int sceneToLoad)
    {
        nextScene = sceneToLoad;
        StartFade(1);
    }
     void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
        nextScene = -1;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void DeleteGame()
    {
        gameData.gameHasStarted = false;//y todo un método de reset
    }
    #endregion
}
