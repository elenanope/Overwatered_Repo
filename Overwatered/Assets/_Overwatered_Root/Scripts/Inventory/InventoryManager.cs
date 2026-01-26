using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] ItemClass itemToAdd;
    [SerializeField] ItemClass itemToRemove;

    [SerializeField] private GameObject slotHolder;
    [SerializeField] private SlotClass[] startingItems;
    private SlotClass[] items;

    GameObject[] slots;
    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot;
    bool interacting;
    [SerializeField]bool isMovingItem;
    private void Start()
    {
        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new SlotClass();
        }

        for (int i = 0; i < startingItems.Length; i++)
        {
            items[i] = startingItems[i];
        }

        for (int i = 0; i < slotHolder.transform.childCount; i++) slots[i] = slotHolder.transform.GetChild(i).gameObject;
        Add(itemToAdd, 1);
        Remove(itemToRemove);
        RefreshUI();
    }

    private void Update()
    {
        if(interacting)
        {
            interacting = false;
            if(isMovingItem)
            {
                EndItemMove();
            }
            else
            {
                BeginItemMove();
            }
        }
    }
    public void OnTouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) interacting = true;
    }
    #region Inventory Bases
    public void RefreshUI()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            try
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon; 
                if(items[i].GetItem().isStackable) slots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = items[i].GetQuantity() + "";
                else
                {
                    slots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
                }
            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            }
        }
    }
    public bool Add(ItemClass item, int quantity)
    {
        //items.Add(item);


        SlotClass slot = Contains(item);
        if (slot != null && slot.GetItem().isStackable) slot.AddQuantity(1); //añadir tmb límite de stack
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].GetItem() == null)//está vacío
                {
                    items[i].AddItem(item, quantity);
                    break;
                }
            }
            /*if(items.Count < slots.Length)
            {
                items.Add(new SlotClass(item, 1));
            }
            else
            {
                Debug.Log("No te cabe!");
                return false;
            }*/
        }

        RefreshUI();
        return true;
    }
    public bool Remove(ItemClass item)
    {
        //items.Remove(item);
        SlotClass temp = Contains(item);
        if (temp != null)
        {
            if(temp.GetQuantity() > 1)
            temp.SubQuantity(1);
            else
            {
                int slotToRemoveIndex = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == item)
                    {
                        slotToRemoveIndex = i;
                        break;
                    }
                }
                items[slotToRemoveIndex].Clear();
            }
        }
        else
        {
            return false;
        }
        RefreshUI();
        return true;
    }

    public SlotClass Contains(ItemClass item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetItem() == item)//está vacío
            {
                return items[i];
            }
        }
        return null;

        /*foreach (SlotClass slot in items)
        {
            if(slot.GetItem() == item) return slot;
        }*/
    }
#endregion
    #region Moving Objects
    private bool BeginItemMove()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.GetItem() == null) return false;

        movingSlot = new SlotClass(originalSlot);
        originalSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        return true;
    }
    private bool EndItemMove()
    {
        tempSlot = GetClosestSlot();
        //poner el originalSlot para que devuelva el objeto a donde estaba al principio

        if (tempSlot == null)//si lo sueltas en un sitio random
        {
            originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();
        }
        else
        {
            if (tempSlot.GetItem() != null)
            {
                if (tempSlot.GetItem() == movingSlot.GetItem())
                {
                    if (tempSlot.GetItem().isStackable)
                    {
                        tempSlot.AddQuantity(movingSlot.GetQuantity());
                        movingSlot.Clear();
                    }
                    else
                    {
                        //Add(movingSlot.GetItem(), movingSlot.GetQuantity());
                        originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                        movingSlot.Clear();

                        //return false;
                    }
                }
                else//si no lo puedes dejar: vuelve al inicio
                {
                    originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                    movingSlot.Clear();
                }
            }
            else
            {
                tempSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                movingSlot.Clear();
            }
        }

        isMovingItem = false;
        RefreshUI();
        return true;
    }

    SlotClass GetClosestSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Vector2.Distance(slots[i].transform.position, Mouse.current.position.ReadValue()) <= 32) //añadir opción tmb para mando?
                return items[i];
        }
        return null;
    }
    #endregion
}
