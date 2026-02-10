using UnityEngine;

public class BoatController : MonoBehaviour
{
    public bool hasPlayer; 
    PlayerController playerControls;
    Collider objectTouched;
    Vector3 closestPoint;
    Rigidbody boatRb;
    public GameObject sticks;
    [SerializeField] float margin = 2f;
    [SerializeField] Collider[] tempColliders; 
    [SerializeField] BoxCollider solidCol; 

    private void Start()
    {
        boatRb = GetComponent<Rigidbody>();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Shore"))
        {
            if (hasPlayer && !playerControls.isNearLand)
            {
                playerControls.detection.pickUpSign.SetActive(true);
                playerControls.isNearLand = true;
            }
            objectTouched = other;
            //además, que encuentre un punto donde pueda subirse
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shore"))
        {
            if (hasPlayer && playerControls.isNearLand)
            {
                playerControls.detection.pickUpSign.SetActive(false);
                playerControls.isNearLand = false;
            }
        }
    }
    public void RegisterPlayer(PlayerController playerController)
    {
        playerControls = playerController;
        hasPlayer = true;
        sticks.SetActive(false); 
        playerControls.detection.pickUpSign.SetActive(false);
    }
    public void SendClosestPoint( float marginReduction)
    {
        Vector2 actualPoint;
        Vector2 touchedPoint;
        Vector2 directionToLand;

        margin = 2;
        margin -= marginReduction;
        closestPoint = objectTouched.ClosestPoint(transform.position);
        //HandleColliders(true);
        closestPoint = new Vector3(closestPoint.x, closestPoint.y + 1, closestPoint.z);
        touchedPoint = new Vector2(closestPoint.x, closestPoint.z);
        actualPoint = new Vector2(playerControls.gameObject.transform.position.x, playerControls.gameObject.transform.position.z);
        directionToLand = touchedPoint - actualPoint;
        playerControls.shorePoint = new Vector3(actualPoint.x + directionToLand.x * margin, closestPoint.y, actualPoint.y + directionToLand.y * margin);
        Debug.Log(playerControls.shorePoint);
    }
    public void HandleColliders(bool activate)
    {
        float minDistance = 10f;
        int closestSide = 0;
        Vector3 closestLocalPoint;

        foreach (Collider col in tempColliders)
        {
            col.enabled = activate;
        }
        if(activate)
        {
            closestLocalPoint = solidCol.ClosestPointOnBounds(closestPoint);
            //que collider desactivar: 0 delante, 1 izq, 2 derecha, 3 detrás
            Vector3[] sides = new Vector3[]{new Vector3(solidCol.bounds.center.x, solidCol.bounds.center.y, solidCol.bounds.max.z),
            new Vector3(solidCol.bounds.min.x, solidCol.bounds.center.y, solidCol.bounds.center.z),
            new Vector3(solidCol.bounds.max.x, solidCol.bounds.center.y, solidCol.bounds.center.z),
            new Vector3(solidCol.bounds.center.x, solidCol.bounds.center.y, solidCol.bounds.min.z) };
            for (int i = 0; i < sides.Length; i++)
            {
                if (Vector3.Distance(sides[i], closestLocalPoint) < minDistance)
                {
                    minDistance = Vector3.Distance(sides[i], closestLocalPoint);
                    closestSide = i;
                }
            }
            tempColliders[closestSide].enabled = false;
        }
    }
}
