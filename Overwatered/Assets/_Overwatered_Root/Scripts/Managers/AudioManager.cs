using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("AudioManager is null!");
            }
            return instance;
        }
    }

    public int musicVolume;
    public int sfxVolume;
    [SerializeField] int minMusicVolume;
    [SerializeField] int maxMusicVolume;//equilibrar músicas en programa externo
    [SerializeField] int minSFXVolume;
    [SerializeField] int maxSFXVolume;
    [SerializeField] AudioSource primaryMusicSource;
    [SerializeField] AudioSource secondaryMusicSource;//ponerlos en el mismo gameObject que este script
    //[SerializeField] AudioSource[] musicSources;
    [SerializeField] AudioSource playerSource;
    [SerializeField] AudioClip[] music;
    //[SerializeField] AudioSource[] sfxSources;
    //[SerializeField] AudioClip[] playerSFX;//poner en player


    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        primaryMusicSource.loop = true;
        secondaryMusicSource.loop = true;
    }
    private void Update()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            //música main menu
        }
        else if(SceneManager.GetActiveScene().buildIndex == 1) //encontrar todos los gameobjects con audio sources y ponerles el volumen que toque??
        {
            //música normal
        }
        else if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            //minijuego
        }
    }
    public void ChangeMusicVolume(int _musicVolume)
    {
        primaryMusicSource.volume = _musicVolume;
        secondaryMusicSource.volume = _musicVolume;
    }
    public void ChangeSFXVolume(int _sfxVolume)
    {
        playerSource.volume = _sfxVolume;
    }
}
