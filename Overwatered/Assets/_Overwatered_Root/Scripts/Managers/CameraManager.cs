using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;

    public static CameraManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("DialogueCamera is null!");
            }
            return instance;
        }
    }
    [Header("Blend Between Cameras")]
    [SerializeField] Transform camera1;
    [SerializeField] Transform camera2; 
    Vector3 camera1Pos;
    Quaternion camera1Rot;
    Vector3 camera2Pos;
    Quaternion camera2Rot;
    [SerializeField] float lerpTime = 0.5f;
    [SerializeField] float rotationTimeOffset = 1.4f;
    [SerializeField] float positionTimeOffset = 1.4f;

    bool toDialogueCam = true;

    [Header("Dialogue Camera Settlement")]
    [SerializeField] Transform head1Trans;
    //[SerializeField] Transform head2Trans;
    [SerializeField] float frontDistance;
    [SerializeField] float verticalWeight;
    Vector3 head1;
    public Vector3 head2;
    Vector2 head1Top;
    Vector2 head2Top;
    [SerializeField] Vector3 middlePoint;
    [SerializeField] Vector3 cameraPoint;
    [SerializeField] float angles;

    Vector2 cameraTemp;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Update()
    {
        //DialogueDistance();
    }
    public void LerpBetweenCameras()//bool toDialogue
    {
        DialogueDistance();
        StartCoroutine(SwitchCamera());
        //DialogueDistance();

    }
    IEnumerator SwitchCamera()
    {
        Vector3 pos = camera1.transform.position;
        Quaternion rot = camera1.transform.rotation;
        float rotOffset;
        float posOffset;
        float lateProgress;
        float fasterMult = 1f;

        float progressPos = 0.0f;  //This value is used for LERP
        float progressRot = 0.0f;  //This value is used for LERP

        GameManager.Instance.cameraReady = false;
        //si hubiera más de dos cámaras cambiantes, poner en el método como variante auxiliar
        if (toDialogueCam)
        {
            camera1Pos = camera1.position;
            camera1Rot = camera1.rotation;
        } 
        camera2Pos = camera2.position;
        camera2Rot = camera2.rotation;

        rotOffset = !toDialogueCam ?  rotationTimeOffset : 1;
        posOffset = toDialogueCam ?  positionTimeOffset : 1;

        if(toDialogueCam)
        {
            camera1.gameObject.GetComponent<ThirdPersonCamController>().enabled = false;
            camera1.gameObject.GetComponent<CinemachineOrbitalFollow>().enabled = false;
            camera1.gameObject.GetComponent<CinemachineRotationComposer>().enabled = false;
            camera1.gameObject.GetComponent<CinemachineInputAxisController>().enabled = false;
        }
            
        if(Mathf.Abs(rot.y - (!toDialogueCam ? camera1Rot.y : camera2Rot.y)) > 360)
        {
            posOffset = 1;
            rotOffset = 1;
        }
        while (progressPos < 1.0f)
        {
            camera1.transform.position = Vector3.Lerp(pos, !toDialogueCam ? camera1Pos : camera2Pos, progressPos * posOffset);
            camera1.transform.rotation = Quaternion.Lerp(rot, !toDialogueCam ? camera1Rot : camera2Rot, progressRot * rotOffset);
            yield return new WaitForEndOfFrame();
            progressPos += Time.deltaTime * lerpTime * fasterMult;
            progressRot += Time.deltaTime * lerpTime * fasterMult;
            if (rotOffset != 1) lateProgress = progressRot;
            else lateProgress = progressPos;
            if (lateProgress > (!toDialogueCam ? 0.5f : 0.35f) && (!toDialogueCam? rotOffset : posOffset) != 1)
            {
                    rotOffset = 1;
                    posOffset = 1;
                    pos = camera1.transform.position;
                    rot = camera1.transform.rotation;
                    progressPos = 0.0f;
                    progressRot = 0.0f;
                fasterMult = 2f;
            }
        }
        if (!toDialogueCam)
        {
            camera1.gameObject.GetComponent<ThirdPersonCamController>().enabled = true;
            camera1.gameObject.GetComponent<CinemachineOrbitalFollow>().enabled = true;
            camera1.gameObject.GetComponent<CinemachineRotationComposer>().enabled = true;
            camera1.gameObject.GetComponent<CinemachineInputAxisController>().enabled = true;
            GameManager.Instance.cameraReady = true;
        }
        //Set final transform
        camera1.transform.SetPositionAndRotation(!toDialogueCam ? camera1Pos : camera2Pos, !toDialogueCam ?  camera1Rot : camera2Rot);
        toDialogueCam = !toDialogueCam;
    }
    public void DialogueDistance()//close FOV 9 for dialogueCam
    {
        Vector2 direction;
        Vector2 tempDifference;
        Vector2 middleTemp;
        Vector3 lookDir;
        head1 = head1Trans.position;
        //head2 = head2Trans.position; 

        head1Top = new Vector2(head1.x, head1.z);
        head2Top = new Vector2(head2.x, head2.z);
        tempDifference = head2Top - head1Top;
        //middlePoint = new Vector3((head1Top.x + head2Top.x) / 2,  (head1.y + head2.y)/2 * verticalWeight + Mathf.Abs(head1.y - head2.y), (head1Top.y + head2Top.y) / 2);
        middleTemp = new Vector2((head1Top.x + head2Top.x) / 2, (head1Top.y + head2Top.y) / 2);
        middlePoint = new Vector3(middleTemp.x, 0f, middleTemp.y);
        direction = new Vector2(tempDifference.y, -tempDifference.x).normalized;

        cameraTemp = middleTemp + direction * frontDistance;
        cameraPoint = new Vector3(cameraTemp.x, (head1.y + head2.y) / 2 + verticalWeight * Mathf.Abs(head1.y - head2.y), cameraTemp.y);

        camera2.transform.position = cameraPoint; 
        lookDir = middlePoint - camera2.transform.position;
        lookDir.y = 0f;
        camera2.transform.rotation = Quaternion.LookRotation(lookDir);
    }
    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(head1, head2);
        Gizmos.DrawSphere(middlePoint, 0.1f);
        Gizmos.DrawSphere(cameraTemp, 0.1f);
    }
}
