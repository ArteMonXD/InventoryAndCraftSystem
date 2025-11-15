using UnityEngine;

public enum PlaceType { Inventory, CraftPanel}

public class DragAndDropMessage
{
    private SlotIdentifier fromData;
    public SlotIdentifier From => fromData;

    private SlotIdentifier toData;
    public SlotIdentifier To => toData;

    public ItemStack Item;

    public DragAndDropMessage(SlotIdentifier from, SlotIdentifier to, ItemStack item)
    {
        fromData = from;
        toData = to;
        Item = item;
    }

    public DragAndDropMessage(SlotIdentifier from, ItemStack item)
    {
        fromData = from;
        Item = item;
    }

    public void AddFromData(SlotIdentifier from)
    {
        fromData = from;
    }

    public void AddToData(SlotIdentifier to)
    {
        toData = to;
    }

    public bool TryGetFrom<T>(out T value) where T : class
    {
        return TryGetIdentifierValue(From, out value);
    }

    public bool TryGetTo<T>(out T value) where T : class
    {
        return TryGetIdentifierValue(To, out value);
    }

    private bool TryGetIdentifierValue<T>(SlotIdentifier identifier, out T value) where T : class
    {
        value = null;
        if (identifier is T typedIdentifier)
        {
            value = typedIdentifier;
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return $"DragAndDrop: {From} -> {To}";
    }
}

public abstract class SlotIdentifier
{
    public abstract object GetValue();
    public abstract PlaceType GetPlaceType();
}

[System.Serializable]
public class InventorySlotIdentifier : SlotIdentifier
{
    public Vector2Int Slot;

    public InventorySlotIdentifier(Vector2Int slot)
    {
        Slot = slot;
    }

    public override object GetValue() => Slot;

    public override PlaceType GetPlaceType() => PlaceType.Inventory;

    public override string ToString() => $"InventorySlot({Slot.x},{Slot.y})";
}

[System.Serializable]
public class CraftingSlotIdentifier : SlotIdentifier
{
    public int Slot;

    public CraftingSlotIdentifier(int slot)
    {
        Slot = slot;
    }

    public override object GetValue() => Slot;

    public override PlaceType GetPlaceType() => PlaceType.CraftPanel;

    public override string ToString() => $"CraftingSlot({Slot})";
}
