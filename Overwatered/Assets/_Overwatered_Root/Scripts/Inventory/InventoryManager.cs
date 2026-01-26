using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditorInternal.VersionControl.ListControl;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] ItemClass itemToAdd;
    [SerializeField] ItemClass itemToRemove;

    [SerializeField] private GameObject itemCursor;
    [SerializeField] private GameObject selectionBubble;

    [SerializeField] private GameObject slotHolder;
    [SerializeField] private SlotClass[] startingItems;
    private SlotClass[] items;

    GameObject[] slots;
    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot;
    private SlotClass currentSlot;
    int clickState; // 0 no pulsado, 1 izquierdo, 2 derecho
    [SerializeField]bool isMovingItem;
    [SerializeField]bool bubbleOpened;
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
        if(itemToAdd != null) Add(itemToAdd, 1);
        if(itemToRemove != null) Remove(itemToRemove);
        RefreshUI();
    }

    private void Update()
    {
        itemCursor.SetActive(isMovingItem);
        itemCursor.transform.position = Mouse.current.position.ReadValue();
    }
    public void OnTouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !bubbleOpened)
        {
            if (isMovingItem)
            {
                EndItemMove();
            }
            else
            {
                BeginItemMove(false);
            }
        }
    }
    public void OnOptions(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !bubbleOpened && !isMovingItem)
        {
            currentSlot = GetClosestSlot();
            Vector3 selectedSlotTrans = new Vector3(0f, 0f, 0f);
            //encontrar slotApretado y a X distancia (esquiina inferior a menos que sea el último slot, se lleva ahí la selectionBubble)
            bubbleOpened = true;
            for (int i = 0; i < slots.Length; i++)
            {
                if (items[i] == currentSlot)
                {
                    selectedSlotTrans = slots[i].transform.position;
                    break;
                }
            }
            selectionBubble.transform.position = new Vector3(selectedSlotTrans.x + 10, selectedSlotTrans.y + 10, selectedSlotTrans.z);
            selectionBubble.SetActive(true);
            if (currentSlot.GetQuantity()>1)
            {
                selectionBubble.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                selectionBubble.transform.GetChild(0).gameObject.SetActive(false);
            }
            if(currentSlot.GetItem().GetConsumable() != null)
            {
                selectionBubble.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                selectionBubble.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }
    public void SelectionBubble(int selectionNumber)
    {
        if(selectionNumber == 0)
        {
            BeginItemMove(true);
        }
        else if(selectionNumber == 1)
        {
            if((int)currentSlot.GetItem().GetConsumable().consumableType == 0)
            {
                Debug.Log("Se ha añadido la siguiente cantidad de comida:" + currentSlot.GetItem().GetConsumable().foodAdded + "y la siguiente cantidad de agua:" + currentSlot.GetItem().GetConsumable().waterAdded);
                //playerController Eat(currentSlot.GetItem().GetConsumable().foodAdded, currentSlot.GetItem().GetConsumable().waterAdded)
            }
            else if((int)currentSlot.GetItem().GetConsumable().consumableType == 1)
            {
                Debug.Log("Se ha añadido la siguiente cantidad de agua:" + currentSlot.GetItem().GetConsumable().waterAdded);
                //playerController Drink(currentSlot.GetItem().GetConsumable().waterAdded)
            }
            currentSlot.SubQuantity(1);
            if(currentSlot.GetQuantity() <= 0) currentSlot.Clear();
            RefreshUI();
            //lo consume, se va el objeto al mundo real, se lo come/bebe depende de lo que sea y gana vida
        }
        else if(selectionNumber == 2)
        {
            //back
        }
        bubbleOpened = false;
        selectionBubble.SetActive(false);
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
    private bool BeginItemMove(bool singleMove)
    {
        originalSlot = !singleMove ? GetClosestSlot() : currentSlot;
        if (originalSlot == null || originalSlot.GetItem() == null) return false;

        if (!singleMove)
        {

            Debug.Log("first");
            movingSlot = new SlotClass(originalSlot);
            originalSlot.Clear();
            itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;
            isMovingItem = true;
            RefreshUI();
            return true;

        }
        else
        {
            Debug.Log("second");
            if (originalSlot.GetQuantity() > 1)
            {
                movingSlot = new SlotClass(originalSlot.GetItem(), 1);
                originalSlot.SubQuantity(1);
                itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;
                isMovingItem = true;
                RefreshUI();
                return true;
            }
            else return false;
        }
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
                else//si no lo puedes dejar: vuelve al sitio original (otra opción sería intercambiar objetos, pero entonces ya complica si tiene que volver a su origen)
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
