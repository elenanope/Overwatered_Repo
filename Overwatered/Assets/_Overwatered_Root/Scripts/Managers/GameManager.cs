using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
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

    public ThirdPersonCamController camController;
    //poner los paneles en el player para que puedan ser privados?
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject inventoryPanel;
    public CinemachineCamera cinemachineCamera;
    [SerializeField] Camera cameraComponent;
    [SerializeField] CinemachineCamera dialogueCam;
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] CinemachineRotationComposer dialogueCamRot;
    bool overworldCamActive = true;
    bool charactersHidden = false;
    float actualXOffset;
    float actualZOffset;

    [SerializeField] Image fadePanel;
    public float fadeTime = 2f;
    public bool faded;
    public bool fading;
    int goalAlpha;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        StartFade(0);
        actualXOffset = dialogueCamRot.TargetOffset.x;
        actualZOffset = dialogueCamRot.TargetOffset.z;
    }
    private void Update()
    {
        if(charactersHidden) //mejorar esta rotacion
        {
            actualZOffset = Mathf.Lerp(dialogueCamRot.TargetOffset.z, -6, Time.deltaTime * 3);
            if (dialogueCamRot.TargetOffset.z < -0.5f) actualXOffset = Mathf.Lerp(dialogueCamRot.TargetOffset.x, -2, Time.deltaTime); //ajustar
            if (dialogueCamRot.TargetOffset.x <= -1.9f && dialogueCamRot.TargetOffset.z <= -5.9f) charactersHidden = false;
            dialogueCamRot.TargetOffset = new Vector3(actualXOffset, 0, actualZOffset);
        }
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
            }
        }
    }
    public void ChangeCamera()
    {
        StartCoroutine(ChangeCamCoroutine());
    }
     IEnumerator ChangeCamCoroutine()
    {
        Vector3 NPCToCam;
        Vector3 PlayerToCam;
        if (!overworldCamActive)
        {
            cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = true;
            cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = true;
            charactersHidden = false;
            actualXOffset = 0f;
            actualZOffset = 0f;
            dialogueCamRot.TargetOffset.x = 0f;
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
            //hacerlo midiendo distancias entre cámara y cada personaje
            NPCToCam = cameraComponent.WorldToScreenPoint(targetGroup.Targets[0].Object.position);
            PlayerToCam = cameraComponent.WorldToScreenPoint(targetGroup.Targets[1].Object.position);
            if(Mathf.Abs(NPCToCam.x - PlayerToCam.x) < 50f) //esto va raro
            {
                charactersHidden = true;
                //dialogueCamRot.TargetOffset.x = -3f;
                //dialogueCamRot.TargetOffset.z = -5f;
            }
            Debug.Log("NPC está a " + NPCToCam.x);
            Debug.Log("Player está a " + PlayerToCam.x);
            overworldCamActive = !overworldCamActive;
            yield break;
        }
    }
    public void SetNPCTarget(Transform npcTransform)
    {
        targetGroup.Targets[0].Object = npcTransform;
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
            if (sceneReferences.targetGroup != null) targetGroup = sceneReferences.targetGroup;
            if (sceneReferences.dialogueCamRot != null) dialogueCamRot = sceneReferences.dialogueCamRot;
            if (sceneReferences.fadePanel != null) fadePanel = sceneReferences.fadePanel;
            StartFade(0);
        }
        else
        {
            Debug.Log("References were not found");
        }
    }

    public void StartFade(int desiredAlpha)
    {
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
    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
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
