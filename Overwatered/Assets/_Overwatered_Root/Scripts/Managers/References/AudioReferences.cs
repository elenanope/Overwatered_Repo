using UnityEngine;
using UnityEngine.UI;

public class AudioReferences : MonoBehaviour
{
    public AudioSource[] musicSources;
    public AudioSource[] sfxSources;
    public Slider musicSlider;
    public Slider sfxSlider;
    private void Awake()
    {
        
    }
    private void Start()
    {
        if (musicSlider != null) AudioManager.Instance.musicSlider = musicSlider;
        if(sfxSlider != null) AudioManager.Instance.sfxSlider = sfxSlider;
        if (AudioManager.Instance != null) AudioManager.Instance.FindAudioReferences();
    }
}
