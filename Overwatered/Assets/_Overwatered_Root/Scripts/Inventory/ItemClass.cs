using UnityEngine;

public abstract class ItemClass : ScriptableObject
{
    [Header ("Item")]
    public string itemName;
    public Sprite itemIcon;
    public string itemDescription;
    public bool isStackable;
    //public GameObject itemPrefab;
    public abstract ItemClass GetItem();
    public abstract ToolClass GetTool();
    public abstract MiscClass GetMisc();
    public abstract ConsumableClass GetConsumable();
}
