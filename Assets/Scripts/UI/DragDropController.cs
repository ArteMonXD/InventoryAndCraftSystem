using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    private DragAndDropMessage dragDropSlotData;
    private ItemStack draggedItem;
    private bool isDragging;
    private Canvas canvas;

    public static event System.Action<DragAndDropMessage> OnDragBegin;
    public static event System.Action<DragAndDropMessage> OnDragEnd;

    public void Initialize<T>(ItemStack item, PlaceType type, T slotData, ItemStack itemStack)
    {
        draggedItem = item;
        if(type == PlaceType.Inventory)
        {
            if (slotData is Vector2Int data)
            {
                dragDropSlotData = new DragAndDropMessage(new InventorySlotIdentifier(data), itemStack);
            }
        }
        else 
        {
            if (slotData is int data)
            {
                dragDropSlotData = new DragAndDropMessage(new CraftingSlotIdentifier(data), itemStack);
            }
        }
        itemImage.sprite = itemStack.ItemData.icon;
        quantityText.text = itemStack.Quantity > 1 ? itemStack.Quantity.ToString() : "";

        // Настройка Canvas
        canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        // Начинаем перетаскивание
        StartDrag();
    }

    private void StartDrag()
    {
        if (draggedItem == null) return;

        Debug.Log($"Custom drag started from {dragDropSlotData.From.ToString()}");

        isDragging = true;
        transform.SetParent(transform.root);
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        // Уведомляем о начале перетаскивания
        OnDragBegin?.Invoke(dragDropSlotData);
    }

    private void Update()
    {
        if (!isDragging) return;

        // Следуем за курсором
        if (Input.mousePresent)
        {
            transform.position = Input.mousePosition;
        }

        // Завершаем перетаскивание при отпускании кнопки мыши
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void EndDrag()
    {
        if (!isDragging) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        isDragging = false;

        // Определяем целевой слот
        SlotIdentifier targetSlot = FindTargetSlotUnderMouse();
        dragDropSlotData.AddToData(targetSlot);

        // Уведомляем систему
        OnDragEnd?.Invoke(dragDropSlotData);

        Debug.Log($"Custom drag ended from {dragDropSlotData.From}/{dragDropSlotData.To}");

        // Уничтожаем
        Destroy(gameObject);
    }

    private SlotIdentifier FindTargetSlotUnderMouse()
    {
        // Создаем PointerEventData для raycast'а
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            Debug.Log("Find slot: " + result.gameObject.transform.name);
            var slotUI = result.gameObject.GetComponent<SlotUI>();
            if (slotUI != null && dragDropSlotData.TryGetFrom(out InventorySlotIdentifier inventorySlotIdentifier) && slotUI.GridPosition != inventorySlotIdentifier.Slot)
            {
                return new InventorySlotIdentifier(slotUI.GridPosition);
            }

            var craftingSlot = result.gameObject.GetComponent<CraftingSlot>();
            if(craftingSlot != null && dragDropSlotData.TryGetFrom(out CraftingSlotIdentifier craftingSlotIdentifier) && craftingSlot.GridIndex != craftingSlotIdentifier.Slot && craftingSlot.GridIndex != 10)
            {
                return new CraftingSlotIdentifier(craftingSlot.GridIndex);
            }
        }

        return null;
    }
}
