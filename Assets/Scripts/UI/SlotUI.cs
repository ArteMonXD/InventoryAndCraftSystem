using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : SlotAbstract, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI quantityText;

    private InventorySystem inventory;

    private Vector2Int gridPosition;

    public Vector2Int GridPosition => gridPosition;

    public void Initialize(Vector2Int position)
    {
        gridPosition = position;
        inventory = FindFirstObjectByType<InventorySystem>();

        // Выключаем Raycast Target у дочерних элементов
        if (itemIcon != null)
            itemIcon.raycastTarget = false;

        if (quantityText != null)
        {
            var textGraphic = quantityText.GetComponent<Graphic>();
            if (textGraphic != null)
                textGraphic.raycastTarget = false;
        }

        // ПОДПИСКА НА СОБЫТИЯ DRAG & DROP
        DragDropController.OnDragBegin += OnDragBegin;
        DragDropController.OnDragEnd += OnDragEnd;

        Clear();
    }

    public override void DisplayItem(ItemStack itemStack)
    {
        base.DisplayItem(itemStack);

        if (itemStack.IsStackable && itemStack.Quantity > 1)
        {
            quantityText.text = itemStack.Quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity > 1)
        {
            quantityText.text = quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    public override void Clear()
    {
        base.Clear();
        quantityText.gameObject.SetActive(false);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        var slot = inventory.GetSlot(gridPosition);
        if (!slot.IsEmpty)
        {
            //TooltipSystem.Instance.RequestShowTooltip(slot.ItemStack, eventData.position);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        //TooltipSystem.Instance.RequestHideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var slot = inventory.GetSlot(gridPosition);
        if (slot.IsEmpty) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Input.GetKey(KeyCode.LeftShift) && slot.ItemStack.IsStackable)
            {
                // Split stack on Shift+Click
                int splitAmount = Mathf.Max(1, slot.ItemStack.Quantity / 2);
                inventory.TrySplitStack(gridPosition, splitAmount);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Quick use or other action
            Debug.Log($"Right clicked on {slot.ItemStack.ItemData.itemName}");
        }
    }

    protected override void StartDrag()
    {
        var slot = inventory.GetSlot(gridPosition);
        if (slot.IsEmpty) return;

        Debug.Log($"Starting drag from {gridPosition}");

        // Создаем DragPreview в позиции курсора
        Vector3 mousePosition = Input.mousePosition;
        var dragObject = Instantiate(dragPreviewPrefab, mousePosition, Quaternion.identity, transform.root);

        var dragController = dragObject.GetComponent<DragDropController>();
        if (dragController != null)
        {
            dragController.Initialize(
                slot.ItemStack,
                PlaceType.Inventory,
                gridPosition,
                slot.ItemStack
            );
        }
    }

    protected override void OnDragBegin(DragAndDropMessage message)
    {
        // Можно добавить визуальный эффект для исходного слота
        if (message.TryGetFrom(out InventorySlotIdentifier inventorySlotIdentifier) && inventorySlotIdentifier.Slot == gridPosition)
        {
            // Например, сделать слот полупрозрачным
            itemIcon.color = new Color(1, 1, 1, 0.5f);
        }
    }

    protected override void OnDragEnd(DragAndDropMessage message)
    {
        // Восстанавливаем визуал
        if (message.TryGetFrom(out InventorySlotIdentifier inventorySlotIdentifier) && inventorySlotIdentifier.Slot == gridPosition)
        {
            itemIcon.color = Color.white;
        }          

        // Если drop произошел на этот слот - обрабатываем
        //if (toPosition == gridPosition && fromPosition != gridPosition)
        //{
        //    inventory.TryMoveItem(fromPosition, toPosition);
        //}
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        var slot = inventory.GetSlot(gridPosition);
        if (slot.IsEmpty) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                // Start drag
                StartDrag();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Quick use or other action
            Debug.Log($"Right clicked on {slot.ItemStack.ItemData.itemName}");
        }
    }
}
