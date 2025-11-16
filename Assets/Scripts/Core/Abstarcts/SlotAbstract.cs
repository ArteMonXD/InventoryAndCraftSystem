using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SlotAbstract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] protected Image itemIcon;
    [SerializeField] protected GameObject dragPreviewPrefab;

    public virtual void DisplayItem(ItemStack itemStack)
    {
        if (itemStack == null)
        {
            Clear();
            return;
        }

        itemIcon.sprite = itemStack.ItemData.icon;
        itemIcon.gameObject.SetActive(true);
    }

    public virtual void Clear()
    {
        itemIcon.gameObject.SetActive(false);
    }

    public abstract void OnPointerEnter(PointerEventData eventData);

    public abstract void OnPointerExit(PointerEventData eventData);

    protected abstract void StartDrag();

    protected abstract void OnDragBegin(DragAndDropMessage message);

    protected abstract void OnDragEnd(DragAndDropMessage message, bool success);
    
    protected virtual void OnDestroy()
    {
        DragDropController.OnDragBegin -= OnDragBegin;
        DragDropController.OnDragEnd -= OnDragEnd;
    }

    public abstract void OnPointerDown(PointerEventData eventData);
}
