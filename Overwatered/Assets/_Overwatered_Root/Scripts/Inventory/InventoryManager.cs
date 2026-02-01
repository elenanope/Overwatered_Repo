using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private SlotClass[] tempItems;
    private SlotClass[] imaginaryItems;
    //poner bool para ver si está lleno, en refreshUI
    GameObject[] slots;
    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot;
    private SlotClass currentSlot;
    [SerializeField]bool isMovingItem;
    [SerializeField]bool bubbleOpened;
    bool singleMove;

    [SerializeField] ItemClass migajas;
    [SerializeField] ItemClass rebanadas;
    [SerializeField] ItemClass barras;
    [SerializeField] ItemClass agua;
    int migajasNumber;
    int rebanadasNumber;
    int barrasNumber;
    int aguaNumber;
    int itemsSelected;
    [SerializeField] Button tradeButton;
    private void Start()
    {
        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];
        tempItems = new SlotClass[slots.Length];
        imaginaryItems = new SlotClass[slots.Length];

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new SlotClass();
            tempItems[i] = new SlotClass();
            imaginaryItems[i] = new SlotClass();
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
            if (GameManager.Instance.tradeMode)
            {
                currentSlot = GetClosestSlot();
                if (currentSlot != null)
                {
                    for (int i = 0; i < slots.Length; i++)
                    {
                        if (items[i] == currentSlot)
                        {
                            if(currentSlot.GetItem() != null)
                            {
                                if (items[i].GetItem().GetMisc() != null)
                                {
                                    tempItems[i].AddItem(items[i].GetItem(), items[i].GetQuantity());
                                    items[i].Clear();
                                    slots[i].gameObject.transform.GetChild(2).gameObject.SetActive(true);
                                    itemsSelected++;
                                    if(itemsSelected == 1) tradeButton.enabled = true;
                                }
                                else
                                {
                                    Debug.Log("no basura");
                                    //esto no es basura!!
                                }
                            }
                            else//aqwui no es posible llegar
                            {
                                items[i].AddItem(tempItems[i].GetItem(), tempItems[i].GetQuantity());
                                tempItems[i].Clear();
                                slots[i].gameObject.transform.GetChild(2).gameObject.SetActive(false);
                                if (itemsSelected > 0) itemsSelected--; 
                                if (itemsSelected == 0) tradeButton.enabled = false;
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                if (!bubbleOpened)
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
    }
    public void OnOptions(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !bubbleOpened && !isMovingItem)
        {
            if (!GameManager.Instance.tradeMode)
            {
                currentSlot = GetClosestSlot();
                if (currentSlot != null)
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

                    selectionBubble.transform.position = new Vector3(selectedSlotTrans.x, selectedSlotTrans.y, selectedSlotTrans.z);
                    selectionBubble.SetActive(true);
                    if (currentSlot.GetQuantity() > 1) selectionBubble.transform.GetChild(0).gameObject.SetActive(true);
                    else selectionBubble.transform.GetChild(0).gameObject.SetActive(false);

                    if (currentSlot.GetItem().GetConsumable() != null) selectionBubble.transform.GetChild(1).gameObject.SetActive(true);
                    else selectionBubble.transform.GetChild(1).gameObject.SetActive(false);

                    selectionBubble.transform.GetChild(2).gameObject.SetActive(false);
                    /*if (currentSlot.GetItem().GetMisc() != null) selectionBubble.transform.GetChild(2).gameObject.SetActive(true);
                    else selectionBubble.transform.GetChild(2).gameObject.SetActive(false);*/
                }
            }
        }
    }
    public string SubmitTrade()
    {
        int totalTrash = 0;
        migajasNumber = 0;
        rebanadasNumber = 0;
        barrasNumber = 0;
        aguaNumber = 0;
        for (int i = 0;i < tempItems.Length;i++)
        {
            if (tempItems[i].GetItem() != null)totalTrash += tempItems[i].GetItem().GetMisc().trashAdded * tempItems[i].GetQuantity();
        }
        while (totalTrash > 0)
        {
            if (totalTrash >= 10)
            {
                totalTrash -= 10;
                aguaNumber++;
                barrasNumber++;
            }
            if (totalTrash >= 4)
            {
                totalTrash -= 4;
                rebanadasNumber += 2;
            }
            if(totalTrash >= 1)
            {
                totalTrash -= 1;
                migajasNumber++;
            }
        }
        if(aguaNumber > 0)
        {
            if(barrasNumber > 0)
            {
                if (rebanadasNumber > 0)
                {
                    if (migajasNumber > 0)
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua, {barrasNumber} barras, {rebanadasNumber} rebanadas y {migajasNumber} migas de pan.";
                    }
                    else
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua, {barrasNumber} barras, {rebanadasNumber} rebanadas de pan.";
                    }
                }
                else
                {
                    if (migajasNumber > 0)
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua, {barrasNumber} barras y {migajasNumber} migas de pan.";
                    }
                    else
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua, {barrasNumber} barras de pan.";
                    }
                }
            }
            else
            {
                if(rebanadasNumber > 0)
                {
                    if (migajasNumber > 0)
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua, {rebanadasNumber} rebanadas y {migajasNumber} migas de pan.";
                    }
                    else
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua, {rebanadasNumber} rebanadas de pan.";
                    }
                }
                else
                {
                    if(migajasNumber > 0)
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua y {migajasNumber} migas de pan.";
                    }
                    else
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {aguaNumber} botellas de agua.";
                    }
                }
            }
        }
        else
        {
            if(barrasNumber > 0)
            {
                if (rebanadasNumber > 0)
                {
                    if (migajasNumber > 0)
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {barrasNumber} barras, {rebanadasNumber} rebanadas y {migajasNumber} migas de pan.";
                    }
                    else
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {barrasNumber} barras, {rebanadasNumber} rebanadas de pan.";
                    }
                }
                else
                {
                    if (migajasNumber > 0)
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {barrasNumber} barras y {migajasNumber} migas de pan.";
                    }
                    else
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {barrasNumber} barras de pan.";
                    }
                }
            }
            else
            {
                if(rebanadasNumber > 0)
                {
                    if (migajasNumber > 0)
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {rebanadasNumber} rebanadas y {migajasNumber} migas de pan.";
                    }
                    else
                    {
                        return $"A cambio de todo eso, te puedo ofrecer {rebanadasNumber} rebanadas de pan.";
                    }
                }
                else
                {
                    return $"No te puedo ofrecer nada";//después quitar
                }
            }
        }

    }
    public void TrashVisibility(bool on)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Image image = slots[i].transform.GetChild(0).GetComponent<Image>();
            if (on && items[i].GetItem()!= null)
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().color = new Color(image.color.r, image.color.g, image.color.b, image.color.a * 2);
                Debug.Log("Complete alpha:" + image.color.a);
            }
            else if (!on && items[i].GetItem() != null)
            {
                if (items[i].GetItem().GetMisc() == null)
                {
                    slots[i].transform.GetChild(0).GetComponent<Image>().color = new Color(image.color.r, image.color.g, image.color.b, image.color.a / 2);
                    Debug.Log("Half alpha:" + image.color.a);
                    itemsSelected = 0;
                    tradeButton.enabled = false;
                    if (!tradeButton.gameObject.activeSelf)tradeButton.gameObject.SetActive(true); 
                }
            }
        }
    }
    public bool FindMisc()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (items[i].GetItem() != null)
            {
                if (items[i].GetItem().GetMisc() != null)
                {
                    return true;
                }

            }
        }
        return false;
    }
    public void TradeResult(bool accepted)//CAMBIAR: se añade dos veces??
    {

        if (accepted)
        {
            List<bool> itemsToCheck = new List<bool>();
            int timesAdded = 0;
            int objectLeft = 0;
            bool[] rewardIndex = new bool[4];
            ItemClass currentItem = null;
            int currentNumber = 0;
            if (aguaNumber > 0) rewardIndex[0] = true;
            if (barrasNumber > 0) rewardIndex[1] = true;
            if (rebanadasNumber > 0) rewardIndex[2] = true;
            if (migajasNumber > 0) rewardIndex[3] = true;

            for (int i = 0; i < items.Length; i++)
            {
                if (tempItems[i].GetItem() != null)
                {
                    imaginaryItems[i] = new SlotClass();
                }
                else imaginaryItems[i] = new SlotClass(items[i]);
            }

            for (int i = 0; i < rewardIndex.Length; i++)
            {
                if (rewardIndex[i] != false)
                {
                    if(i == 0)
                    {
                        currentItem = agua;
                        currentNumber = aguaNumber;
                    }
                    else if(i == 1)
                    {
                        currentItem = barras ;
                        currentNumber = barrasNumber;
                    }
                    else if(i == 2)
                    {
                        currentItem = rebanadas ;
                        currentNumber = rebanadasNumber;
                    }
                    else if(i == 3)
                    {
                        currentItem = migajas;
                        currentNumber = migajasNumber;
                    }
                    if (currentNumber > 0)//esto se debería de poder quitar
                    {
                        if (currentItem.GetItem().stackLimit > currentNumber) itemsToCheck.Add(TempAdd(currentItem, currentNumber));
                        else
                        {
                            timesAdded = currentNumber / currentItem.GetItem().stackLimit;
                            objectLeft = currentNumber % currentItem.GetItem().stackLimit;
                            if (timesAdded != 0)
                            {
                                if (objectLeft > 0)
                                {
                                    for (int a = 0; a < timesAdded - 1; a++)
                                    {
                                        itemsToCheck.Add(TempAdd(currentItem, currentItem.GetItem().stackLimit));
                                    }
                                    itemsToCheck.Add(TempAdd(currentItem, objectLeft));
                                }
                                else
                                {
                                    for (int a = 0; a < timesAdded; a++)
                                    {
                                        itemsToCheck.Add(TempAdd(currentItem, currentItem.GetItem().stackLimit));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            for (int i = 0; i < itemsToCheck.Count; i++)
            {
                if (!itemsToCheck[i])
                {
                    UndoTrade();
                    Debug.Log($"numeros totales {itemsToCheck.Count}, item  {i}");
                    return;
                }
            }

            for (int i = 0; i < tempItems.Length; i++)
            {
                tempItems[i].Clear();
            }
            for (int i = 0; i < rewardIndex.Length; i++)//si se llega hasta aqui, todo se puede llevar a cabo
            {
                if (rewardIndex[i] != false)
                {
                    if (i == 0)
                    {
                        currentItem = agua;
                        currentNumber = aguaNumber;
                    }
                    else if (i == 1)
                    {
                        currentItem = barras;
                        currentNumber = barrasNumber;
                    }
                    else if (i == 2)
                    {
                        currentItem = rebanadas;
                        currentNumber = rebanadasNumber;
                    }
                    else if (i == 3)
                    {
                        currentItem = migajas;
                        currentNumber = migajasNumber;
                    }
                    if (currentNumber > 0)//esto se debería de poder quitar
                    {
                        Debug.Log("intercambio exitoso");
                        //se han añadido tus ganancias!
                        if (currentItem.GetItem().stackLimit > currentNumber) Add(currentItem, currentNumber);
                        else
                        {
                            timesAdded = currentNumber % currentItem.GetItem().stackLimit;
                            if (timesAdded * currentItem.GetItem().stackLimit != currentNumber)
                            {
                                objectLeft = currentNumber - timesAdded * currentItem.GetItem().stackLimit;
                            }
                            if (timesAdded != 0)
                            {
                                if (objectLeft > 0)
                                {
                                    for (int a = 0; a < timesAdded - 1; a++)
                                    {
                                        Add(currentItem, currentItem.GetItem().stackLimit);
                                    }
                                    Add(currentItem, objectLeft);
                                }
                                else
                                {
                                    for (int a = 0; a < timesAdded; a++)
                                    {
                                        Add(currentItem, currentItem.GetItem().stackLimit);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            UndoTrade();
        }
        for (int i = 0;i < slots.Length;i++)
        {
            slots[i].gameObject.transform.GetChild(2).gameObject.SetActive(false);
        }
        tradeButton.gameObject.SetActive(false);
        GameManager.Instance.tradeMode = false;
    }
    public bool TempAdd(ItemClass item, int quantity)
    {
        SlotClass slot = Contains(item, quantity, imaginaryItems);
            if (slot != null && slot.GetItem().isStackable)
            {
                slot.AddQuantity(quantity); //añadir tmb límite de stack, que si se pasa, busque otro slot!!!
                return true;
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (imaginaryItems[i].GetItem() == null)//está vacío
                    {
                        imaginaryItems[i].AddItem(item, quantity);
                        return true;
                    }
                }
            }
        
        return false;
    }
    void UndoTrade()
    {
        for (int i = 0; i < items.Length; i++)
        {
            imaginaryItems[i].Clear();
        }
        for (int i = 0; i < tempItems.Length; i++)
        {
            if (tempItems[i].GetItem() != null)
            {
                if(tempItems[i].GetItem().GetMisc() != null)
                {
                    items[i].AddItem(tempItems[i].GetItem(), tempItems[i].GetQuantity());
                    //Debug.Log($"puesto:{i}, nombre:{tempItems[i].GetItem().itemName}, número:{tempItems[i].GetQuantity()}");
                    tempItems[i].Clear();
                }
                
            }
        }
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.transform.GetChild(2).gameObject.SetActive(false);
        }
        tradeButton.gameObject.SetActive(false);
        GameManager.Instance.tradeMode = false;
        Debug.Log("intercambio cancelado");
    }//comprobar metodo
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
        SlotClass slot = Contains(item, quantity, items);
        if (slot != null && slot.GetItem().isStackable)
        {
            slot.AddQuantity(quantity); //añadir tmb límite de stack, que si se pasa, busque otro slot!!!
            RefreshUI();
            return true;
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].GetItem() == null)//está vacío
                {
                    items[i].AddItem(item, quantity);

                    RefreshUI();
                    return true;
                }
            }
        }
        return false;
    }
    public bool Remove(ItemClass item)
    {
        //items.Remove(item);
        SlotClass temp = Contains(item, 0, items);//comprobar
        if (temp != null)
        {
            if(temp.GetQuantity() > 1) temp.SubQuantity(1);
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
    public SlotClass Contains(ItemClass item, int quantityToAdd, SlotClass[] slot)
    {
        for (int i = 0; i < slot.Length; i++)
        {
            if (slot[i].GetItem() == null) continue;//por si acaso??
            if (slot[i].GetItem() == item && slot[i].GetQuantity() + quantityToAdd <= item.stackLimit)//si va a poder tener espacio
            {
                return slot[i];
            }
        }
        return null;

        
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
