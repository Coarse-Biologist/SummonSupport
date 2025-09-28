using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "SummonSupportItems/General")]
public class Equipment_SO : ScriptableObject, I_LootInterface
{
    [field: SerializeField] public Rarity ItemRarity { private set; get; } = Rarity.Wretched;
    [field: SerializeField] public ItemType ItemType { private set; get; } = ItemType.Potion;
    [field: SerializeField] public int ItemValue { private set; get; } = 1;
    [field: SerializeField] public ItemNameEnum ItemEnum { private set; get; } = ItemNameEnum.HealthPotion1;
    [field: SerializeField] public string ItemName { private set; get; } = "Item Name";
    [field: SerializeField] public string ItemDescription { private set; get; } = "Item Description";
    [field: SerializeField] public List<EquipmentSlot> EquipmentSlot { private set; get; } = new();

    public void Loot()
    {
        EquipmentHandler.AddItemToInventory(this);
    }
}