using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 4;

    public int Width => width;
    public int Height => height;

    [SerializeField] private InventorySlot[,] slots;
    private bool isInitialized = false;

    public event System.Action<Vector2Int, ItemStack> OnItemAdded;
    public event System.Action<Vector2Int, ItemStack> OnItemRemoved;
    public event System.Action<Vector2Int, Vector2Int> OnItemsMoved;
    public event System.Action<Vector2Int, int> OnStackSplit;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.LogWarning("InventorySystem already initialized!");
            return;
        }

        InitializeSlots();

        // ПОДПИСКА НА СОБЫТИЯ DRAG & DROP
        DragDropController.OnDragEnd += HandleDragEnd;

        Debug.Log($"InventorySystem initialized: {width}x{height} grid");
        isInitialized = true;
    }

    //Обработка завершения перетаскивания
    private void HandleDragEnd(DragAndDropMessage message)
    {

        if(message.To != null && message.TryGetFrom(out InventorySlotIdentifier inventoryFromSlot) && message.TryGetTo(out InventorySlotIdentifier inventoryToSlot))
        {
            if (inventoryToSlot.Slot.x >= 0 && inventoryToSlot.Slot.y >= 0) // Валидная позиция
            {
                TryMoveItem(inventoryFromSlot.Slot, inventoryToSlot.Slot);
            }
        }
        else if(message.To != null && message.TryGetFrom(out inventoryFromSlot) && !message.TryGetTo(out inventoryToSlot))
        {
            if (inventoryFromSlot.Slot.x >= 0 && inventoryFromSlot.Slot.y >= 0) // Валидная позиция
            {
                RemoveItem(inventoryFromSlot.Slot, 1);
            }
        }
        else if(message.To != null && !message.TryGetFrom(out inventoryFromSlot) && message.TryGetTo(out inventoryToSlot))
        {
            if (inventoryToSlot.Slot.x >= 0 && inventoryToSlot.Slot.y >= 0) // Валидная позиция
            {
                ItemStack itemStack = new ItemStack(message.Item.ItemData, 1); // Так как проверка на перенос из панели крафта, то добавляем только 1 единицу предмета независимо от того, что храниться в message
                TryAddItem(itemStack, inventoryToSlot.Slot);
            }
        }
        else if (message.To == null)
        {
            if (!message.TryGetFrom(out inventoryFromSlot)) return;
            // Drop outside inventory - удаляем предмет
            RemoveItem(inventoryFromSlot.Slot);
            Debug.Log($"Item dropped out of inventory from {inventoryFromSlot.Slot}");
        }

        //Debug.Log($"Drag & Drop: {message.From.ToString()} -> {message.To.ToString()}");
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            DragDropController.OnDragEnd -= HandleDragEnd;
        }
    }

    private void InitializeSlots()
    {
        slots = new InventorySlot[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                slots[x, y] = new InventorySlot(new Vector2Int(x, y));
            }
        }
    }

    public void FillWithRandomItems()
    {
        if (!isInitialized)
        {
            Debug.LogError("InventorySystem not initialized!");
            return;
        }

        if (ItemRegistry.Instance == null)
        {
            Debug.LogError("ItemRegistry not available!");
            return;
        }

        var randomItems = ItemRegistry.Instance.GenerateRandomStartingItems();

        for (int i = 0; i < randomItems.Count && i < width * height; i++)
        {
            var position = new Vector2Int(i % width, i / width);
            TryAddItem(randomItems[i], position);
        }

        Debug.Log($"Inventory filled with {randomItems.Count} random items");
    }

    // Основной метод добавления предмета
    public bool TryAddItem(ItemStack itemStack, Vector2Int position)
    {
        if (!IsPositionValid(position) || itemStack == null)
            return false;

        var slot = slots[position.x, position.y];

        // Если слот пустой - просто добавляем
        if (slot.IsEmpty)
        {
            slot.SetItem(itemStack);
            OnItemAdded?.Invoke(position, itemStack);
            return true;
        }

        // Если слот занят - пытаемся добавить в стак
        if (slot.CanAcceptItem(itemStack))
        {
            if (slot.TryAddToStack(itemStack, out int addedQuantity))
            {
                if (itemStack.Quantity <= 0)
                {
                    // Предмет полностью добавлен в стак
                    OnItemAdded?.Invoke(position, itemStack);
                }
                return true;
            }
        }

        return false;
    }

    // Поиск первого свободного слота
    public bool TryAddItemToEmptySlot(ItemStack itemStack)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (slots[x, y].IsEmpty)
                {
                    return TryAddItem(itemStack, new Vector2Int(x, y));
                }
            }
        }
        return false;
    }

    // Перемещение предмета между слотами
    public bool TryMoveItem(Vector2Int fromPosition, Vector2Int toPosition)
    {
        if (!IsPositionValid(fromPosition) || !IsPositionValid(toPosition))
            return false;

        var fromSlot = slots[fromPosition.x, fromPosition.y];
        var toSlot = slots[toPosition.x, toPosition.y];

        if (fromSlot.IsEmpty) return false;

        // AUTO-STACK: Если можно добавить в существующий стак
        if (!toSlot.IsEmpty && toSlot.CanAcceptItem(fromSlot.ItemStack))
        {
            if (toSlot.TryAddToStack(fromSlot.ItemStack, out int addedQuantity))
            {
                if (fromSlot.ItemStack.Quantity <= 0)
                {
                    fromSlot.Clear();
                }
                OnItemsMoved?.Invoke(fromPosition, toPosition);
                return true;
            }
        }

        // SWAP: Если целевой слот пустой или содержит другой предмет
        if (toSlot.IsEmpty)
        {
            // Просто перемещаем
            toSlot.SetItem(fromSlot.ItemStack);
            fromSlot.Clear();
            OnItemsMoved?.Invoke(fromPosition, toPosition);
            return true;
        }
        else
        {
            // Меняем местами
            var tempItem = fromSlot.ItemStack;
            fromSlot.SetItem(toSlot.ItemStack);
            toSlot.SetItem(tempItem);
            OnItemsMoved?.Invoke(fromPosition, toPosition);
            return true;
        }
    }

    // Разделение стака (Shift+клик)
    public bool TrySplitStack(Vector2Int position, int splitQuantity)
    {
        if (!IsPositionValid(position)) return false;

        var slot = slots[position.x, position.y];
        if (slot.IsEmpty || !slot.ItemStack.IsStackable || splitQuantity <= 0)
            return false;

        if (splitQuantity >= slot.ItemStack.Quantity)
            return false; // Нельзя разделить весь стак

        var splitStack = slot.SplitStack(splitQuantity);
        if (splitStack != null)
        {
            // Ищем свободный слот для разделенного стака
            if (TryAddItemToEmptySlot(splitStack))
            {
                OnStackSplit?.Invoke(position, splitQuantity);
                return true;
            }
            else
            {
                // Если не нашли свободный слот - возвращаем обратно
                slot.ItemStack.IncreaseQuantity(splitQuantity);
            }
        }

        return false;
    }

    // Удаление предмета (выброс за пределы инвентаря)
    public bool RemoveItem(Vector2Int position, int quantity = -1)
    {
        if (!IsPositionValid(position)) return false;

        var slot = slots[position.x, position.y];
        if (slot.IsEmpty) return false;

        var itemToRemove = slot.ItemStack;

        if (quantity == -1 || quantity >= itemToRemove.Quantity)
        {
            // Удаляем весь стак
            Debug.Log("UI: Inventory Remove all stack");
            slot.Clear();
            OnItemRemoved?.Invoke(position, null);
            return true;
        }
        else
        {
            // Удаляем часть стака
            itemToRemove.DecreaseQuantity(quantity);
            OnItemRemoved?.Invoke(position, itemToRemove);
            return true;
        }
    }

    public bool RemoveItem(string itemId, int quantity)
    {
        if (quantity <= 0) return false;

        int remainingToRemove = quantity;
        var slotsWithItem = FindSlotsWithItem(itemId);

        foreach (var slot in slotsWithItem)
        {
            if (remainingToRemove <= 0) break;

            int removeFromThisStack = Mathf.Min(slot.ItemStack.Quantity, remainingToRemove);
            slot.ItemStack.DecreaseQuantity(removeFromThisStack);
            remainingToRemove -= removeFromThisStack;

            // Если стак опустел - очищаем слот
            if (slot.ItemStack.Quantity <= 0)
            {
                slot.Clear();
                OnItemRemoved?.Invoke(slot.GridPosition, slot.ItemStack);
            }
            else
            {
                // Обновляем UI для частичного удаления
                OnItemRemoved?.Invoke(slot.GridPosition, slot.ItemStack);
            }
        }

        return remainingToRemove == 0;
    }

    private List<InventorySlot> FindSlotsWithItem(string itemId)
    {
        var result = new List<InventorySlot>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var slot = slots[x, y];
                if (!slot.IsEmpty && slot.ItemStack.ItemId == itemId)
                {
                    result.Add(slot);
                }
            }
        }

        // Сортируем так, чтобы сначала шли неполные стаки (для оптимизации)
        result.Sort((a, b) => a.ItemStack.Quantity.CompareTo(b.ItemStack.Quantity));
        return result;
    }

    // Проверка наличия предмета
    public bool HasItem(string itemId, int quantity = 1)
    {
        Debug.Log("HasItem");
        int totalCount = 0;
        foreach (var slot in GetAllSlots())
        {
            if (!slot.IsEmpty && slot.ItemStack.ItemId == itemId)
            {
                totalCount += slot.ItemStack.Quantity;
                if (totalCount >= quantity) return true;
            }
        }
        return false;
    }

    public int GetItemCount(string itemId)
    {
        int total = 0;
        foreach (var slot in GetAllSlots())
        {
            if (!slot.IsEmpty && slot.ItemStack.ItemId == itemId)
            {
                total += slot.ItemStack.Quantity;
            }
        }
        return total;
    }

    // Вспомогательные методы
    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < width &&
               position.y >= 0 && position.y < height;
    }

    private List<InventorySlot> GetAllSlots()
    {
        var allSlots = new List<InventorySlot>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                allSlots.Add(slots[x, y]);
            }
        }
        return allSlots;
    }

    // Методы для UI кнопок
    public void FillWithRandomItemsButton()
    {
        ClearAllSlots();
        FillWithRandomItems();
    }

    public void ClearAllSlots()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                slots[x, y].Clear();
                OnItemRemoved?.Invoke(slots[x, y].GridPosition, null);
            }
        }
    }

    // Дебаг информация
    public void PrintInventoryState()
    {
        Debug.Log("=== INVENTORY STATE ===");
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Debug.Log(slots[x, y].ToString());
            }
        }
    }

    public InventorySlot GetSlot(Vector2Int position)
    {
        if (IsPositionValid(position))
            return slots[position.x, position.y];
        return null;
    }
}
