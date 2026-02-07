using UnityEngine;

public class DialogueCamera : MonoBehaviour
{
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

    Vector2 cameraTemp;
    private void Update()
    {
        DialogueDistance();
    }
    public void DialogueDistance()//close FOV 9 for dialogueCam
    {
        Vector2 direction;
        Vector2 tempDifference;
        Vector2 middleTemp; 
        Vector3 lookDir;

        head1 = head1Trans.position; 
        head2 = head2Trans.position; 

        head1Top = new Vector2(head1.x, head1.z);
        head2Top = new Vector2(head2.x, head2.z);
        tempDifference = head2Top - head1Top;
        //middlePoint = new Vector3((head1Top.x + head2Top.x) / 2,  (head1.y + head2.y)/2 * verticalWeight + Mathf.Abs(head1.y - head2.y), (head1Top.y + head2Top.y) / 2);
        middleTemp = new Vector2((head1Top.x + head2Top.x) / 2, (head1Top.y + head2Top.y) / 2);
        middlePoint = new Vector3(middleTemp.x, 0f, middleTemp.y);
        direction = new Vector2(tempDifference.y, -tempDifference.x).normalized;

        cameraTemp = middleTemp + direction * frontDistance;
        cameraPoint = new Vector3(cameraTemp.x, (head1.y + head2.y) / 2 + verticalWeight * Mathf.Abs(head1.y - head2.y), cameraTemp.y);

        cameraDialogue.transform.position = cameraPoint; 
        lookDir = middlePoint - cameraDialogue.transform.position;
        lookDir.y = 0f;
        cameraDialogue.transform.rotation = Quaternion.LookRotation(lookDir);
    }
    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(head1, head2);
        Gizmos.DrawSphere(middlePoint, 0.1f);
        Gizmos.DrawSphere(cameraTemp, 0.1f);
    }
}
