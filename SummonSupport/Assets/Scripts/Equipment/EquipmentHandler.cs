using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public static class EquipmentHandler
{

    public static Dictionary<Equipment_SO, int> ItemInventory = new();
    public static Dictionary<EquipmentSlot, Equipment_SO> EquippedItems = new();

    public static void InitializeEquippedItemsDict()
    {
        List<EquipmentSlot> allItemSlots = Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>().ToList();
        foreach (EquipmentSlot slot in allItemSlots)
        {
            EquippedItems.Add(slot, null);
        }
    }
    public static void AddItemToInventory(Equipment_SO item, int num = 1)
    {
        if (item != null)
        {
            if (ItemInventory.TryGetValue(item, out int currentlyOwned))
                ItemInventory[item] += num;
            else
                ItemInventory.Add(item, num);
        }
    }
    public static void SubtractItemfromInventory(Equipment_SO item, int num)
    {
        if (item != null)
        {
            if (ItemInventory.TryGetValue(item, out int currentlyOwned) && num <= currentlyOwned)
                ItemInventory[item] -= num;
            if (ItemInventory[item] == 0)
                RemoveItemFromInventory(item);
        }
    }
    public static void RemoveItemFromInventory(Equipment_SO item)
    {
        if (item != null)
        {
            if (ItemInventory.TryGetValue(item, out int currentlyOwned))
                ItemInventory.Remove(item);
        }
    }
    public static void EquipItem(Equipment_SO item, EquipmentSlot equipmentSlot)
    {
        if (EquippedItems != null)
        {
            if (item != null)
            {
                EquippedItems[equipmentSlot] = item; // equip item in slot if item exists
            }
            if (equipmentSlot == EquipmentSlot.LeftHand || equipmentSlot == EquipmentSlot.RightHand)
            {
                EquippedItems[EquipmentSlot.TwoHand] = null; //unequip two hander if equiping either a left or right hand weapon
            }
            if (equipmentSlot == EquipmentSlot.TwoHand)
            {
                EquippedItems[EquipmentSlot.RightHand] = null; //unequip left and right hander if equiping a two handed weapon
                EquippedItems[EquipmentSlot.LeftHand] = null;
            }
        }
    }



}




public enum ItemNameEnum
{
    EmptyItem,
    HealthPotion1,
}

