using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public string ItemId => ItemData.itemId;
    public ItemData ItemData { get; private set; }
    public int Quantity { get; private set; }
    public int MaxStackSize => ItemData.maxStackSize;
    public bool IsStackable => ItemData.IsStackable;

    public ItemStack(ItemData itemData, int quantity = 1)
    {
        ItemData = itemData;
        Quantity = Mathf.Clamp(quantity, 1, itemData.maxStackSize);
    }

    public void IncreaseQuantity(int amount)
    {
        Quantity = Mathf.Min(Quantity + amount, MaxStackSize);
    }

    public void DecreaseQuantity(int amount)
    {
        Quantity = Mathf.Max(0, Quantity - amount);
    }

    public ItemStack SplitStack(int amount)
    {
        if (!IsStackable || amount <= 0 || amount >= Quantity)
            return null;

        DecreaseQuantity(amount);
        return new ItemStack(ItemData, amount);
    }

    public bool CanMergeWith(ItemStack other)
    {
        return other != null &&
               ItemId == other.ItemId &&
               IsStackable &&
               other.IsStackable;
    }

    public int GetMergeAmount(ItemStack other)
    {
        if (!CanMergeWith(other)) return 0;

        int spaceAvailable = MaxStackSize - Quantity;
        return Mathf.Min(other.Quantity, spaceAvailable);
    }

    public override string ToString()
    {
        return $"{ItemData.itemName} x{Quantity}";
    }
}
