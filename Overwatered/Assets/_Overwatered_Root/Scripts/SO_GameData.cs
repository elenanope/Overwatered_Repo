using UnityEngine;

[CreateAssetMenu(fileName = "SO_GameData", menuName = "Scriptable Objects/SO_GameData")]
public class SO_GameData : ScriptableObject
{
    public bool gameHasStarted;
    public Vector3 initialPlayerPos;
    public Quaternion initialPlayerRot;
    public Vector3 lastPlayerPos;
    public Quaternion lastPlayerRot;
}
