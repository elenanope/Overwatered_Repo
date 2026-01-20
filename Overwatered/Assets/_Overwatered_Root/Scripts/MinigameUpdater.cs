using UnityEngine;

public class MinigameUpdater : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject boat;
    private static MinigameUpdater instance;

    public static MinigameUpdater Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("MinigameUpdater is null!");
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        ReloadPlayerPos();
    }

    public void SavePos(Transform npcTransform)
    {
        //npcPos = npcTransform;
        //npcTransform = player.transform;
        GameManager.Instance.gameData.lastPlayerPos = npcTransform.position;
        GameManager.Instance.gameData.lastPlayerRot = npcTransform.rotation;
        GameManager.Instance.gameData.foodLeft = player.GetComponent<PlayerController>().foodLeft;
        GameManager.Instance.gameData.waterLeft = player.GetComponent<PlayerController>().waterLeft;

    }
    public void SaveBoatPos(Transform boatTransform)
    {
        GameManager.Instance.gameData.lastBoatPos = boatTransform.position;
        GameManager.Instance.gameData.lastBoatRot = boatTransform.rotation;
    }
    void ReloadPlayerPos()
    {
        if(GameManager.Instance.gameData.gameHasStarted)
        {
            player.transform.SetPositionAndRotation(GameManager.Instance.gameData.lastPlayerPos, GameManager.Instance.gameData.lastPlayerRot);
            boat.transform.SetPositionAndRotation(GameManager.Instance.gameData.lastBoatPos, GameManager.Instance.gameData.lastBoatRot);
            player.GetComponent<PlayerController>().foodLeft = GameManager.Instance.gameData.foodLeft;
            player.GetComponent<PlayerController>().waterLeft = GameManager.Instance.gameData.waterLeft;
        }
        else
        {
            player.transform.position = GameManager.Instance.gameData.initialPlayerPos;
            player.transform.rotation = GameManager.Instance.gameData.initialPlayerRot;
            GameManager.Instance.gameData.gameHasStarted = true;
        }
        Debug.Log(GameManager.Instance.gameData.initialPlayerPos);
        Debug.Log(GameManager.Instance.gameData.lastPlayerPos);
    }
}
