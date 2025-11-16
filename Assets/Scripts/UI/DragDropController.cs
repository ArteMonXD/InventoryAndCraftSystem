using TMPro;
using Unity.VisualScripting;
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
    public static event System.Action<DragAndDropMessage, bool> OnDragEnd;

    public void Initialize<T>(ItemStack item, PlaceType type, T slotData, ItemStack itemStack)
    {
        draggedItem = item;
        Debug.Log($"Drag & Drop Message: Initialize {itemStack.ItemId}");
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
        if(FindTargetSlotUnderMouse(out SlotIdentifier targetSlot))
        {
            dragDropSlotData.AddToData(targetSlot);

            // Уведомляем систему
            OnDragEnd?.Invoke(dragDropSlotData, true);
        }
        else
        {
            dragDropSlotData.AddToData(targetSlot);

            // Уведомляем систему
            OnDragEnd?.Invoke(dragDropSlotData, false);
        }

        Debug.Log($"Custom drag ended from {dragDropSlotData.From.ToString()}/{dragDropSlotData.To?.ToString()}");

        // Уничтожаем
        Destroy(gameObject);
    }

    private bool FindTargetSlotUnderMouse(out SlotIdentifier slotIdentifier)
    {
        // Создаем PointerEventData для raycast'а
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            Debug.Log("Find slot: " + result.gameObject.name);
            var slotUI = result.gameObject.GetComponent<SlotUI>();
            Debug.Log("Find slotUI?: " + slotUI != null);
            if (slotUI != null)
            {
                if(!dragDropSlotData.TryGetFrom(out InventorySlotIdentifier inventorySlotIdentifier) || slotUI.GridPosition != inventorySlotIdentifier.Slot)
                {
                    slotIdentifier = new InventorySlotIdentifier(slotUI.GridPosition);
                    return true;
                }
                else
                {
                    slotIdentifier = null;
                    return false;
                }
            }

            var craftingSlot = result.gameObject.GetComponent<CraftingSlot>();
            Debug.Log("Find craftingSlot?: " + craftingSlot != null);
            if (craftingSlot != null)
            {
                if((!dragDropSlotData.TryGetFrom(out CraftingSlotIdentifier craftingSlotIdentifier) || craftingSlot.GridIndex != craftingSlotIdentifier.Slot) && craftingSlot.GridIndex != 10)
                {
                    slotIdentifier = new CraftingSlotIdentifier(craftingSlot.GridIndex);
                    return true;
                }
                else
                {
                    slotIdentifier = null;
                    return false;
                }
            }
            Debug.Log("Find?: " + result.gameObject.GetComponentInChildren<CraftingSlot>() != null);
        }

        slotIdentifier = null;
        return true;
    }
}
