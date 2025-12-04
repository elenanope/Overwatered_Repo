using UnityEngine;

public class BoatController : MonoBehaviour
{
    public bool hasPlayer; 
    PlayerController playerControls;
    Collider objectTouched;
    Vector3 closestPoint;
    Rigidbody boatRb;
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
            if (hasPlayer && playerControls.isNearLand) playerControls.isNearLand = false;
        }
    }
    public void RegisterPlayer(PlayerController playerController)
    {
        playerControls = playerController;
        hasPlayer = true;

        Debug.Log("2" + playerControls.gameObject.transform.position);
        //boatRb.AddForce(transform.up * 1.2f, ForceMode.Force);// o velocity change
    }
    public void SendClosestPoint()
    {
        closestPoint = objectTouched.ClosestPoint(transform.position);
        closestPoint = new Vector3(closestPoint.x, closestPoint.y + 1, closestPoint.z);
        playerControls.shorePoint = closestPoint;
        //añadir margen (detectar hacia donde hay/no hay superficie y mandarte más hacia donde si que haya)
    }
}
