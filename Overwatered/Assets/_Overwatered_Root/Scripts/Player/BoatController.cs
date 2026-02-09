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
        closestPoint = new Vector3(closestPoint.x, closestPoint.y + 1, closestPoint.z);
        touchedPoint = new Vector2(closestPoint.x, closestPoint.z);
        actualPoint = new Vector2(playerControls.gameObject.transform.position.x, playerControls.gameObject.transform.position.z);
        directionToLand = touchedPoint - actualPoint;
        playerControls.shorePoint = new Vector3(actualPoint.x + directionToLand.x * margin, closestPoint.y, actualPoint.y + directionToLand.y * margin);
        Debug.Log(playerControls.shorePoint);
        //playerControls.shorePoint = closestPoint;
        //añadir margen (detectar hacia donde hay/no hay superficie y mandarte más hacia donde si que haya)
    }
}
