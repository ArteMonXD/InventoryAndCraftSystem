using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class CraftingSlot : SlotAbstract
{
    protected CraftingSystem craftingSystem;

    [SerializeField] protected int gridIndex;
    public int GridIndex => gridIndex;

    public virtual void Initialize(int index)
    {
        gridIndex = index;
        craftingSystem = FindFirstObjectByType<CraftingSystem>();

        // Выключаем Raycast Target у дочерних элементов
        if (itemIcon != null)
            itemIcon.raycastTarget = false;

        // ПОДПИСКА НА СОБЫТИЯ DRAG & DROP
        DragDropController.OnDragBegin += OnDragBegin;
        DragDropController.OnDragEnd += OnDragEnd;

        Clear();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Slot UI Pointer Enter");
        var item = craftingSystem.GetItem(gridIndex);
        if (craftingSystem.GetSlotStatus(gridIndex))
        {
            TooltipSystem.Instance.ShowTooltipForItemStack(item, eventData.position);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Slot UI Pointer Exit");
        TooltipSystem.Instance.HideTooltip();
    }

    protected override void StartDrag()
    {
        var status = craftingSystem.GetSlotStatus(gridIndex);
        if (!status) return;

        Debug.Log($"Starting drag from in craft grid {gridIndex}");

        // Создаем DragPreview в позиции курсора
        Vector3 mousePosition = Input.mousePosition;
        var dragObject = Instantiate(dragPreviewPrefab, mousePosition, Quaternion.identity, transform.root);
        var dragController = dragObject.GetComponent<DragDropController>();
        if (dragController != null)
        {
            var item = craftingSystem.GetItem(gridIndex);

            dragController.Initialize(
                item,
                PlaceType.CraftPanel,
                gridIndex,
                craftingSystem.GetItem(gridIndex)
            );
        }
    }

    protected override void OnDragBegin(DragAndDropMessage message)
    {
        // Можно добавить визуальный эффект для исходного слота
        if (message.TryGetFrom(out CraftingSlotIdentifier craftFromSlot) && craftFromSlot.Slot == gridIndex)
        {
            // Например, сделать слот полупрозрачным
            itemIcon.color = new Color(1, 1, 1, 0.5f);
        }
    }

    protected override void OnDragEnd(DragAndDropMessage message, bool success)
    {
        if (message.TryGetFrom(out CraftingSlotIdentifier craftFromSlot) && craftFromSlot.Slot == gridIndex)
        {
            // Например, сделать слот полупрозрачным
            itemIcon.color = Color.white;
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!craftingSystem.GetSlotStatus(gridIndex)) return;

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
            Debug.Log($"Right clicked on {craftingSystem.GetItem(gridIndex).ItemData.itemName}");
        }
    }
}
