using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotGridParent;
    [SerializeField] private GameObject slotUIPrefab;

    private InventorySystem inventory;
    private SlotUI[,] slotUIs;
    private bool isInitialized = false;

    // Явная инициализация
    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.LogWarning("InventoryUIController already initialized!");
            return;
        }

        // Находим системы
        inventory = FindFirstObjectByType<InventorySystem>();
        if (inventory == null)
        {
            Debug.LogError("InventorySystem not found for InventoryUIController!");
            return;
        }

        // Инициализируем UI
        InitializeUI();
        SubscribeToEvents();

        Debug.Log($"InventoryUIController initialized: {inventory.Width}x{inventory.Height} grid");
        isInitialized = true;
    }

    private void InitializeUI()
    {
        slotUIs = new SlotUI[inventory.Width, inventory.Height];

        // Очищаем старые слоты если есть
        foreach (Transform child in slotGridParent)
        {
            Destroy(child.gameObject);
        }

        // Создаем новые слоты
        for (int x = 0; x < inventory.Width; x++)
        {
            for (int y = 0; y < inventory.Height; y++)
            {
                var slotGO = Instantiate(slotUIPrefab, slotGridParent);
                var slotUI = slotGO.GetComponent<SlotUI>();

                if (slotUI != null)
                {
                    slotUI.Initialize(new Vector2Int(x, y));
                    slotUIs[x, y] = slotUI;
                }
                else
                {
                    Debug.LogError("SlotUI component not found on slot prefab!");
                }
            }
        }

        Debug.Log($"Created {inventory.Width * inventory.Height} inventory slots");
    }

    private void SubscribeToEvents()
    {
        if (inventory != null)
        {
            inventory.OnItemAdded += OnItemAdded;
            inventory.OnItemRemoved += OnItemRemoved;
            inventory.OnItemsMoved += OnItemsMoved;
            inventory.OnStackSplit += OnStackSplit;
        }
    }

    //Безопасное обновление UI
    private void OnItemAdded(Vector2Int position, ItemStack itemStack)
    {
        if (!isInitialized) return;

        if (IsPositionValid(position))
        {
            slotUIs[position.x, position.y].DisplayItem(itemStack);
            Debug.Log($"UI: Item added at {position} - {itemStack.ItemData.itemName}");
        }
    }

    private void OnItemRemoved(Vector2Int position, ItemStack itemStack)
    {
        if (!isInitialized) return;
        Debug.Log($"UI: Start item removed from {position}");
        if (IsPositionValid(position))
        {

            if (itemStack == null || itemStack.Quantity <= 0)
            {
                slotUIs[position.x, position.y].Clear();
                Debug.Log($"UI: Item removed from {position}");
            }
            else
            {
                slotUIs[position.x, position.y].UpdateQuantity(itemStack.Quantity);
                Debug.Log($"UI: Item quantity updated at {position} - {itemStack.Quantity}");
            }
        }
    }

    private void OnItemsMoved(Vector2Int from, Vector2Int to)
    {
        if (!isInitialized) return;

        if (IsPositionValid(from) && IsPositionValid(to))
        {
            // Получаем актуальные данные из инвентаря
            var fromSlot = inventory.GetSlot(from);
            var toSlot = inventory.GetSlot(to);

            // Обновляем UI
            if (fromSlot.IsEmpty)
            {
                slotUIs[from.x, from.y].Clear();
            }
            else
            {
                slotUIs[from.x, from.y].DisplayItem(fromSlot.ItemStack);
            }

            if (toSlot.IsEmpty)
            {
                slotUIs[to.x, to.y].Clear();
            }
            else
            {
                slotUIs[to.x, to.y].DisplayItem(toSlot.ItemStack);
            }

            Debug.Log($"UI: Items moved from {from} to {to}");
        }
    }

    private void OnStackSplit(Vector2Int position, int splitQuantity)
    {
        if (!isInitialized) return;

        if (IsPositionValid(position))
        {
            var slot = inventory.GetSlot(position);
            if (!slot.IsEmpty)
            {
                slotUIs[position.x, position.y].UpdateQuantity(slot.ItemStack.Quantity);
                Debug.Log($"UI: Stack split at {position} - new quantity: {slot.ItemStack.Quantity}");
            }
        }
    }

    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < inventory.Width &&
               position.y >= 0 && position.y < inventory.Height;
    }

    // Принудительное обновление всего UI
    public void RefreshAllSlots()
    {
        if (!isInitialized) return;

        for (int x = 0; x < inventory.Width; x++)
        {
            for (int y = 0; y < inventory.Height; y++)
            {
                var slot = inventory.GetSlot(new Vector2Int(x, y));
                if (!slot.IsEmpty)
                {
                    slotUIs[x, y].DisplayItem(slot.ItemStack);
                }
                else
                {
                    slotUIs[x, y].Clear();
                }
            }
        }

        Debug.Log("Inventory UI completely refreshed");
    }

    private void OnDestroy()
    {
        if (inventory != null && isInitialized)
        {
            inventory.OnItemAdded -= OnItemAdded;
            inventory.OnItemRemoved -= OnItemRemoved;
            inventory.OnItemsMoved -= OnItemsMoved;
            inventory.OnStackSplit -= OnStackSplit;
        }
    }
}
