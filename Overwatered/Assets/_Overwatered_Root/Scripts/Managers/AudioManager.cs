using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    /*[Header("Music and SFX Volume")]
    [SerializeField] float musicVolume;
    [SerializeField] float sfxVolume;
    [SerializeField] int minMusicVolume;
    [SerializeField] int maxMusicVolume;//equilibrar músicas en programa externo
    [SerializeField] int minSFXVolume;
    [SerializeField] int maxSFXVolume;*/
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio Clips and Sources")]
    //[SerializeField] AudioSource primaryMusicSource;
    //[SerializeField] AudioSource secondaryMusicSource;//ponerlos en el mismo gameObject que este script
    [SerializeField] AudioSource ownSource;
    [SerializeField] AudioClip[] music;
    [SerializeField] AudioReferences audioRefs; 
    public AudioSource[] musicSources;
    public AudioSource[] sfxSources;
    public SO_AudioData audioLibrary;
    //[SerializeField] AudioClip[] playerSFX;//poner en player


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);//al volver a la escena inicial ya no va bien
        }
        //primaryMusicSource.loop = true;
        //secondaryMusicSource.loop = true;
    }
    private void Start()
    {
        ChangeMusicVolume();
        ChangeSFXVolume(); 
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
    void ListenToSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.value = audioLibrary.musicVolume;
            musicSlider.onValueChanged.AddListener((v) => {
                audioLibrary.musicVolume = v;
                ChangeMusicVolume();
            });
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = audioLibrary.sfxVolume;
            sfxSlider.onValueChanged.AddListener((v) => {
                audioLibrary.sfxVolume = v;
                ChangeSFXVolume();
                sfxSources[0].Play();
            });
        }
    }
    public void FindAudioReferences()
    {
        GameObject.Find("References").TryGetComponent(out audioRefs);
        if (audioRefs != null)
        {
            if (audioRefs.musicSources.Length == 1 && musicSources[0] == null) musicSources[0] = audioRefs.musicSources[0];
            ListenToSliders();
            //update volume on all sources
            foreach (AudioSource source in audioRefs.musicSources)
            {
                source.volume = audioLibrary.musicVolume;//ajustar
                source.loop = true;
            }
            foreach (AudioSource sfxSource in audioRefs.sfxSources)
            {
                sfxSource.volume = audioLibrary.sfxVolume;//ajustar
            }
        }
        else
        {
            Debug.Log("References were not found");
        }
    }
    public void ChangeMusicVolume()
    {
        //musicVolume = _musicVolume;
        foreach (AudioSource source in musicSources)
        {
            source.volume = audioLibrary.musicVolume;
        }
        if (audioRefs != null)
        {
            foreach (AudioSource source in audioRefs.musicSources)
            {
                source.volume = audioLibrary.musicVolume;
            }
        }
    }
    public void ChangeSFXVolume( )
    {
        //sfxVolume = _sfxVolume; 
        ownSource.volume = audioLibrary.sfxVolume;
        foreach (AudioSource sfxSource in sfxSources)
        {
            sfxSource.volume = audioLibrary.sfxVolume;
        }
        if (audioRefs != null)
        {
            foreach (AudioSource sfxSource in audioRefs.sfxSources)
            {
                sfxSource.volume = audioLibrary.sfxVolume;
            }
        }
    }
    public void PlaySound(int indexInLibrary, bool loopable)
    {
        ownSource.pitch = 1f;
        if (loopable) ownSource.loop = true;
        else ownSource.loop = false;
        ownSource.clip = audioLibrary.sfx[indexInLibrary];
        ownSource.Play();
    }
    public void StopSound()
    {
        ownSource.Stop();
    }
    public void NPCSpeak(float customPitch)
    {
        ownSource.pitch = customPitch;
        ownSource.loop = false;
        if(!ownSource.isPlaying)
        {
            ownSource.clip = audioLibrary.sfxNPC[Random.Range(0, audioLibrary.sfxNPC.Length)];
            ownSource.Play();
        }
    }
}
