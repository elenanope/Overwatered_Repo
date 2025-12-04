using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

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

    public ThirdPersonCamController camController;
    //poner los paneles en el player para que puedan ser privados?
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject inventoryPanel;
    public CinemachineCamera cinemachineCamera;
    [SerializeField] Camera cameraComponent;
    [SerializeField] CinemachineCamera dialogueCam;
    [SerializeField] CinemachineTargetGroup targetGroup;
    CinemachineRotationComposer dialogueCamRot;
    bool overworldCamActive = true;
    bool charactersHidden = false;
    float actualXOffset;
    float actualZOffset;


    private void Awake()
    {
        instance = this;
        dialogueCam.TryGetComponent<CinemachineRotationComposer>(out dialogueCamRot);
        actualXOffset = dialogueCamRot.TargetOffset.x;
        actualZOffset = dialogueCamRot.TargetOffset.z;
    }
    private void Update()
    {
        if(charactersHidden) //mejorar esta rotacion
        {
            actualZOffset = Mathf.Lerp(dialogueCamRot.TargetOffset.z, -6, Time.deltaTime * 3);
            if (dialogueCamRot.TargetOffset.z < -0.5f) actualXOffset = Mathf.Lerp(dialogueCamRot.TargetOffset.x, -2, Time.deltaTime); //ajustar
            //if (dialogueCamRot.TargetOffset.z < -0.5f) actualXOffset = Mathf.Lerp(dialogueCamRot.TargetOffset.x, -2, Time.deltaTime); //uno de estos va mal
            if (dialogueCamRot.TargetOffset.x <= -1.9f && dialogueCamRot.TargetOffset.z <= -5.9f) charactersHidden = false;
            dialogueCamRot.TargetOffset = new Vector3(actualXOffset, 0, actualZOffset);
        }
        //if(winPanel == null) winPanel = GameObject.Find("WinPanel");
        //if(losePanel == null) losePanel = GameObject.Find("LosePanel");
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
}
