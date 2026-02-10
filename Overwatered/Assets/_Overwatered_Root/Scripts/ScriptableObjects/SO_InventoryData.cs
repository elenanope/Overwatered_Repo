using UnityEngine;

[CreateAssetMenu(fileName = "SO_InventoryData", menuName = "Scriptable Objects/SO_InventoryData")]
public class SO_InventoryData : ScriptableObject
{
    public SlotClass[] startingItems;
    public bool[] pickedUpObjects;//same order as in SpawnerManager
    public bool inventoryDataAvailable;
}
