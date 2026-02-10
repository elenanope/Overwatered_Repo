using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_ObjectType", menuName = "Scriptable Objects/SO_ObjectType")]
public class SO_ObjectType : ScriptableObject
{
    [SerializeField] string objectName;
    [SerializeField] string objectDescription;
    //[SerializeField] int objectUse;
    public enum objectUse { Herramienta, Consumible, Entretenimiento}; //corrección necesaria?
    [SerializeField] objectUse use;
    [SerializeField] GameObject objectPrefab;
    [SerializeField] bool canBeDropped;

    public string ObjectName { get { return objectName; } }
    public string ObjectDescription { get { return objectDescription; } }
    public objectUse ObjectUse { get { return use; } }
    //public int ObjectUse { get { return objectUse; } }

}
