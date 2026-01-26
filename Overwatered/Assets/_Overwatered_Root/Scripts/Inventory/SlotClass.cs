using System;
using UnityEngine;

[System.Serializable] // esto que es
public class SlotClass
{
    [SerializeField] ItemClass item;
    [SerializeField] int quantity;
    public SlotClass()
    {
        item = null;
        quantity = 0;
    }

    public SlotClass (ItemClass _item, int _quantity)
    {
        item = _item;
        quantity = _quantity;
    }
    public SlotClass (SlotClass slot)
    {
        item = slot.item;
        quantity = slot.quantity;
    }
    public void Clear()
    {
        this.item = null;
        this.quantity = 0;
    }

    public ItemClass GetItem() { return item; }
    public int GetQuantity() { return quantity; }
    public void AddQuantity(int _quantity)
    {
        quantity += _quantity;
    }
    public void SubQuantity(int _quantity)
    {
        quantity -= _quantity;
    }
    public void AddItem(ItemClass item, int quantity)//#3 revisar
    {
        this.item = item;
        this.quantity = quantity;
    }
}
