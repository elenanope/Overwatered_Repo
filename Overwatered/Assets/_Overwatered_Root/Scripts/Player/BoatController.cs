using UnityEngine;

public class BoatController : MonoBehaviour
{
    bool hasPlayer; 
    PlayerController playerControls;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ExitWater"))
        {
            if (hasPlayer && !playerControls.isNearLand) playerControls.isNearLand = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ExitWater"))
        {
            if (hasPlayer && playerControls.isNearLand) playerControls.isNearLand = false;
        }
    }
    public void RegisterPlayer(PlayerController playerController)
    {
        playerControls = playerController;
    }
}
