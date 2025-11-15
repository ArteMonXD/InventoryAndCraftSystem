using UnityEngine;

public class InventorySlot
{
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private ItemStack itemStack;

    public Vector2Int GridPosition => gridPosition;
    public ItemStack ItemStack => itemStack;
    public bool IsEmpty => itemStack == null || itemStack.Quantity <= 0;
    public bool HasItem => !IsEmpty;

    public InventorySlot(Vector2Int position)
    {
        gridPosition = position;
        itemStack = null;
    }

    public void SetItem(ItemStack newItemStack)
    {
        itemStack = newItemStack;
    }

    public void Clear()
    {
        itemStack = null;
    }

    public bool CanAcceptItem(ItemStack itemToAdd)
    {
        if (IsEmpty) return true;

        // Проверяем можно ли добавить в стак
        if (itemStack.ItemId == itemToAdd.ItemId && itemStack.IsStackable)
        {
            return itemStack.Quantity < itemStack.MaxStackSize;
        }

        return false;
    }

    public int GetSpaceAvailable()
    {
        if (IsEmpty || !itemStack.IsStackable)
            return 0;

        return itemStack.MaxStackSize - itemStack.Quantity;
    }

    public bool TryAddToStack(ItemStack itemToAdd, out int addedQuantity)
    {
        addedQuantity = 0;

        if (!CanAcceptItem(itemToAdd))
            return false;

        int spaceAvailable = GetSpaceAvailable();
        int quantityToAdd = Mathf.Min(itemToAdd.Quantity, spaceAvailable);

        itemStack.IncreaseQuantity(quantityToAdd);
        itemToAdd.DecreaseQuantity(quantityToAdd);
        addedQuantity = quantityToAdd;

        return quantityToAdd > 0;
    }

    public ItemStack SplitStack(int quantityToSplit)
    {
        if (IsEmpty || !itemStack.IsStackable || quantityToSplit >= itemStack.Quantity)
            return null;

        var splitStack = new ItemStack(itemStack.ItemData, quantityToSplit);
        itemStack.DecreaseQuantity(quantityToSplit);

        return splitStack;
    }

    public override string ToString()
    {
        if (IsEmpty)
            return $"Slot [{gridPosition.x},{gridPosition.y}]: Empty";

        return $"Slot [{gridPosition.x},{gridPosition.y}]: {itemStack.ItemData.itemName} x{itemStack.Quantity}";
    }
}
