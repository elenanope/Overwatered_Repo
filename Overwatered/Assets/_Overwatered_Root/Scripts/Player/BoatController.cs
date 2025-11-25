using UnityEngine;

public class BoatController : MonoBehaviour
{
    public bool hasPlayer; 
    PlayerController playerControls;
    Collider objectTouched;
    Vector3 closestPoint;
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
    }
    public void SendClosestPoint()
    {
        closestPoint = objectTouched.ClosestPoint(transform.position);
        closestPoint = new Vector3(closestPoint.x, closestPoint.y + 1, closestPoint.z);
        playerControls.shorePoint = closestPoint;
    }
}
