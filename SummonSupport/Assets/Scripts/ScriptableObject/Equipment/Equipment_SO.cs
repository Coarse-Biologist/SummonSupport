using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "SummonSupportItems/Items")]
public class Item_SO : ScriptableObject, I_LootInterface
{
    [SerializeField] private Rarity itemRarity = Rarity.Wretched;
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
    [SerializeField] private List<EquipmentSlot> equipmentSlot = new List<EquipmentSlot>();
    public List<EquipmentSlot> EquipmentSlot => equipmentSlot;

    public void Loot()
    {
        EquipmentHandler.AddItemToInventory(this);
    }
}