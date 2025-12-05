using UnityEngine;

public class MinigameUpdater : MonoBehaviour
{
    [SerializeField] GameObject player;
    

    private void Awake()
    {
        ReloadPlayerPos();
    }

    public void SavePos(Transform npcTransform)
    {
        //npcPos = npcTransform;
    }
    void ReloadPlayerPos()
    {
        if(GameManager.Instance.gameData.gameHasStarted)
        {
            player.transform.position = GameManager.Instance.gameData.lastPlayerPos;
            player.transform.rotation = GameManager.Instance.gameData.lastPlayerRot;
        }
        else
        {
            player.transform.position = GameManager.Instance.gameData.initialPlayerPos;
            player.transform.rotation = GameManager.Instance.gameData.initialPlayerRot;
            GameManager.Instance.gameData.gameHasStarted = true;
        }
    }
}
