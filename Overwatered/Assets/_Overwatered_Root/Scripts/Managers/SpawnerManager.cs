using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    private static SpawnerManager instance;

    public static SpawnerManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("SpawnerManager is null!");
            }
            return instance;
        }
    }

    public GameObject[] pickableObjects;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        for (int i = 0; i < pickableObjects.Length; i++)
        {
            pickableObjects[i].SetActive(GameManager.Instance.inventoryData.pickedUpObjects[i]);
        }
    }
}
