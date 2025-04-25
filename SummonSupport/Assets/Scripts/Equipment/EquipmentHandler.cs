using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public static class EquipmentHandler
{
public static List<Item_SO> ItemInventory = new List<Item_SO>();
public static Dictionary<EquipmentSlot, Item_SO> EquippedItems = new Dictionary<EquipmentSlot, Item_SO>();

public static void InitializeEquippedItemsDict()
{
List<EquipmentSlot> allItemSlots = Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>().ToList();
foreach(EquipmentSlot slot in allItemSlots)
{
    EquippedItems.Add(slot, null);
}

}

public static void AddItemToInventory(Item_SO item)
{
    if(item != null) ItemInventory.Add(item);
    else Logging.Error("You tried to loot a null object. Clever?");
}

public static void EquipItem(Item_SO item)
{


}



}




public enum ItemNameEnum
{
    EmptyItem,
    HealthPotion1,
}

