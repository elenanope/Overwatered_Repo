using Unity.Cinemachine;
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
    [SerializeField] CinemachineCamera dialogueCam;
    [SerializeField] CinemachineTargetGroup targetGroup;
    CinemachineRotationComposer dialogueCamRot;
    bool overworldCamActive = true;
    bool charactersHidden = false;


    private void Awake()
    {
        instance = this;
        dialogueCamRot = dialogueCam.GetComponent<CinemachineRotationComposer>();
    }
    public void ChangeCamera()
    {
        if (!overworldCamActive)
        {
            cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = true;
            cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = true;
            dialogueCamRot.TargetOffset.x = 0f;
            dialogueCam.Priority = 0;
            cinemachineCamera.Priority = 1;
        }
        else
        {
            cinemachineCamera.gameObject.GetComponent<ThirdPersonCamController>().enabled = false;
            cinemachineCamera.gameObject.GetComponent<CinemachineInputAxisController>().enabled = false;
            //definir lo siguiente mejor para que solo detecte al personaje con el que se habla y al player
            //hacerlo midiendo distancias entre cámara y cada personaje?
            RaycastHit[] hits = Physics.RaycastAll(dialogueCam.transform.position, dialogueCam.transform.forward * 20f); 
            Debug.Log($"Se han tocado {hits.Length} personajes");
            if (hits.Length > 1)
            {
                dialogueCamRot.TargetOffset.x = -8f;
            }
            dialogueCam.Priority = 1;
            cinemachineCamera.Priority = 0;
        }
        overworldCamActive = !overworldCamActive;
    }
    public void SetNPCTarget(Transform npcTransform)
    {
        targetGroup.Targets[0].Object = npcTransform;
    }
}
