using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public static class EquipmentHandler
{
public static List<Item_SO> ItemInventory = new List<Item_SO>();
public static Dictionary<EquipmentSlots, Item_SO> EquippedItems = new Dictionary<EquipmentSlots, Item_SO>();

public static void InitializeEquippedItemsDict()
{
List<EquipmentSlots> allItemSlots = Enum.GetValues(typeof(EquipmentSlots)).Cast<EquipmentSlots>().ToList();
foreach(EquipmentSlots slot in allItemSlots)
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

public enum EquipmentSlots
{
    Head,
    Chest,
    Arms,
    LeftHand,
    Righthand,
    TwoHand,
    Finger,
    Legs,
    Feet,
    BackPack
}
public enum ItemType
{
 SelfBuffer,
 Buffer,
 SelfHealer,
 Healer,
 Debuffer,
 Potion,
 Poison,
 Quest,

}
public enum Rarity
{
Trash,
Common,
Uncommon,
Rare,
Exotic,
Legendary,
}
public enum ItemNameEnum
{
    EmptyItem,
    HealthPotion1,
}

public class Item_SO: ScriptableObject, ILootInterface
{
    [SerializeField] private Rarity itemRarity = Rarity.Trash;
    public Rarity ItemRarity => itemRarity;
    [SerializeField] private ItemType itemType = ItemType.Potion;
    public ItemType ItemType => itemType;
    [SerializeField] private int itemValue = 1;
    public int ItemValue => itemValue;
    [SerializeField] private ItemNameEnum itemEnum = ItemNameEnum.HealthPotion1;
    public ItemNameEnum ItemEnum => itemEnum;
    [SerializeField] private string itemName = "Item Name";
    public string ItemName => itemName;
    [SerializeField] private string itemDescription = "Item Description";
    public string ItemDescription => itemDescription;
    [SerializeField] private List<EquipmentSlots> equipmentSlot = new List<EquipmentSlots>();
    public List<EquipmentSlots> EquipmentSlot => equipmentSlot;

    public void Loot()
    {
        EquipmentHandler.AddItemToInventory(this);
    }
}