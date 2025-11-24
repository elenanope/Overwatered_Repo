using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                Debug.Log("GameManager is null!");
            }
            return instance;
        }
    }

    public ThirdPersonCamController camController;

    private void Awake()
    {
        instance = this;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
