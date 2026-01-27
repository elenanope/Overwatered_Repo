using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditorInternal.VersionControl.ListControl;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
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
    bool singleMove;
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
        selectionBubble.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = !GameManager.Instance.isEating;//hacer más óptimo?
    }
    public void OnTouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !bubbleOpened)
        {
            if(!bubbleOpened)
            {
                if (isMovingItem)
                {
                    EndItemMove();
                }
                else
                {
                    BeginItemMove();
                }
            }
            else
            {
                bubbleOpened = false;
                selectionBubble.SetActive(false);
            }
            
        }
    }
    public void OnOptions(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !bubbleOpened && !isMovingItem)
        {
            currentSlot = GetClosestSlot();
            if(currentSlot != null)
            {
                Vector3 selectedSlotTrans = new Vector3(0f, 0f, 0f);
                //encontrar slotApretado y a X distancia (esquiina inferior a menos que sea el último slot, se lleva ahí la selectionBubble)
                bubbleOpened = true;
                for (int i = 0; i < slots.Length; i++)
                {
                    if (items[i] == currentSlot)
                    {
                        if (i == 4) i = 3;
                        selectedSlotTrans = slots[i].transform.position;
                        break;
                    }
                }
                //if (selectedSlotIndex >= 4 && (selectedSlotIndex - 4) % 5 == 0) selectionBubble.transform.position = new Vector3(selectedSlotTrans.x - 10, selectedSlotTrans.y + 10, selectedSlotTrans.z);
                
                 selectionBubble.transform.position = new Vector3(selectedSlotTrans.x , selectedSlotTrans.y , selectedSlotTrans.z);
                selectionBubble.SetActive(true);
                if (currentSlot.GetQuantity() > 1) selectionBubble.transform.GetChild(0).gameObject.SetActive(true);
                else selectionBubble.transform.GetChild(0).gameObject.SetActive(false);

                if (currentSlot.GetItem().GetConsumable() != null) selectionBubble.transform.GetChild(1).gameObject.SetActive(true);
                else selectionBubble.transform.GetChild(1).gameObject.SetActive(false);

                if (currentSlot.GetItem().GetMisc() != null) selectionBubble.transform.GetChild(2).gameObject.SetActive(true);
                else selectionBubble.transform.GetChild(2).gameObject.SetActive(false);
            }
            
        }
    }
    public void SelectionBubble(int selectionNumber)
    {
        if(selectionNumber == 0)
        {
            singleMove = true;
            BeginItemMove();
        }
        else if(selectionNumber == 1)
        {
            if((int)currentSlot.GetItem().GetConsumable().consumableType == 0)
            {
                Debug.Log("Se ha añadido la siguiente cantidad de comida:" + currentSlot.GetItem().GetConsumable().foodAdded + "y la siguiente cantidad de agua:" + currentSlot.GetItem().GetConsumable().waterAdded);
                playerController.Consume(false, currentSlot.GetItem().itemNumber, currentSlot.GetItem().GetConsumable().waterAdded, currentSlot.GetItem().GetConsumable().foodAdded);
            }
            else if((int)currentSlot.GetItem().GetConsumable().consumableType == 1)
            {
                Debug.Log("Se ha añadido la siguiente cantidad de agua:" + currentSlot.GetItem().GetConsumable().waterAdded);
                playerController.Consume(true, currentSlot.GetItem().itemNumber, currentSlot.GetItem().GetConsumable().waterAdded, currentSlot.GetItem().GetConsumable().foodAdded);
            }
            currentSlot.SubQuantity(1);
            if(currentSlot.GetQuantity() <= 0) currentSlot.Clear();
            RefreshUI();
            //lo consume, se va el objeto al mundo real, se lo come/bebe depende de lo que sea y gana vida
        }
        else if(selectionNumber == 2)
        {
            Debug.Log("se usa el objeto");
        }
        else if (selectionNumber == 3)
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
    private bool BeginItemMove()
    {
        originalSlot = !singleMove ? GetClosestSlot() : currentSlot;
        if (originalSlot == null || originalSlot.GetItem() == null) return false;

        if (!singleMove)
        {
            movingSlot = new SlotClass(originalSlot);
            originalSlot.Clear();

        }
        else
        {
            movingSlot = new SlotClass(originalSlot.GetItem(), 1);
            originalSlot.SubQuantity(1);
        }
        itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;
        isMovingItem = true;
        RefreshUI();
        return true;
    }
    private bool EndItemMove()
    {
        tempSlot = GetClosestSlot();
        //poner el originalSlot para que devuelva el objeto a donde estaba al principio
        if(originalSlot != null)
        {
            if (tempSlot == null)//si lo sueltas en un sitio random
            {
                if (singleMove) originalSlot.AddQuantity(1);
                else originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
            }
            else
            {
                if (tempSlot.GetItem() != null)
                {
                    if (tempSlot.GetItem() == movingSlot.GetItem())
                    {
                        if (tempSlot.GetItem().isStackable && (movingSlot.GetQuantity() + tempSlot.GetQuantity() <= 3))//ajustar cantidad
                        {
                            tempSlot.AddQuantity(movingSlot.GetQuantity());
                        }
                        else
                        {
                            //poner sonido de X
                            if (singleMove) originalSlot.AddQuantity(1);
                            else originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                        }
                    }
                    else//si no lo puedes dejar: vuelve al sitio original (otra opción sería intercambiar objetos, pero entonces ya complica si tiene que volver a su origen)
                    {
                        //o hacerlo para que si originalSlot == null, se ponga en el primero disponible 
                        currentSlot = new SlotClass(tempSlot);//a=b
                        tempSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());//b=c 
                        movingSlot.AddItem(currentSlot.GetItem(), currentSlot.GetQuantity());//a=c 
                        itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;

                        if (singleMove) singleMove = false;
                        RefreshUI();
                        return true;
                        /* NO SE INTERCAMBIAN EL QUE LLEVAS Y EL QUE TOCAS, EL QUE LLEVAS VUELVO A SU LUGAR ORIGINAL
                         * if (singleMove) originalSlot.AddQuantity(1);
                        else originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());*/
                    }
                }
                else
                {
                    tempSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                }
            }
        }
        else
        {
            Add(movingSlot.GetItem(), movingSlot.GetQuantity());
        }
        
        movingSlot.Clear();
        if (singleMove) singleMove = false;
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
