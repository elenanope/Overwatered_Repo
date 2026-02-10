using UnityEngine;

[CreateAssetMenu(fileName = "SO_AudioData", menuName = "Scriptable Objects/SO_AudioData")]
public class SO_AudioData : ScriptableObject
{
    public float musicVolume;
    public float sfxVolume;
    public AudioClip[] sfx;
    public AudioClip[] songs;
}
