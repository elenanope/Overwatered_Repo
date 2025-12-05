using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class SceneReferences : MonoBehaviour
{
    public ThirdPersonCamController camController;
    //poner los paneles en el player para que puedan ser privados?
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject inventoryPanel;
    public CinemachineCamera cinemachineCamera;
    public Camera cameraComponent;
    public CinemachineCamera dialogueCam;
    public CinemachineTargetGroup targetGroup;
    public CinemachineRotationComposer dialogueCamRot;
    public Image fadePanel;

    private void Start()
    {
        GameManager.Instance.FindReferences();
    }
}
