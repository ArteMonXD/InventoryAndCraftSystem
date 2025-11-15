using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultCraftSlot : CraftingSlot
{
    bool isCraeate;
    [SerializeField] private TextMeshProUGUI quantityText;

    public void Initialize()
    {
        gridIndex = 10;
        craftingSystem = FindFirstObjectByType<CraftingSystem>();
        isCraeate = false;

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

    protected override void StartDrag()
    {
        if (!isCraeate)
            return;

        base.StartDrag();
    }

    public void SwitchStatus(bool value)
    {
        if (value)
        {
            isCraeate = true;
            itemIcon.color = Color.white;
        }
        else
        {
            isCraeate = false;
            itemIcon.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

    public override void Clear()
    {
        base.Clear();
        SwitchStatus(false);
        quantityText.gameObject.SetActive(false);
    }

    public override void DisplayItem(ItemStack itemStack)
    {
        base.DisplayItem(itemStack);

        if (itemStack != null && itemStack.IsStackable && itemStack.Quantity > 1)
        {
            quantityText.text = itemStack.Quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    protected override void OnDragEnd(DragAndDropMessage message)
    {
        base.OnDragEnd(message);
        if (!message.TryGetTo(out CraftingSlotIdentifier craftToSlot) || craftToSlot.Slot != gridIndex)
        {
            SwitchStatus(false);
        }
    }
}
