using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    [SerializeField] private List<CraftingRecipe> recipes;

    private ItemStack[] items = new ItemStack[9];
    private ItemStack resultCraft;

    private bool isInitialized = false;

    public event System.Action<CraftingRecipe> OnCraftingPossible;
    public event System.Action<CraftingRecipe> OnCraftingComplete;
    public event System.Action<int, ItemStack> OnItemAdded;
    public event System.Action<int, ItemStack> OnItemRemoved;
    public event System.Action<int, int> OnItemsMoved;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.LogWarning("CraftingSystem already initialized!");
            return;
        }

        DragDropController.OnDragEnd += HandleDragEnd;

        items = new ItemStack[9];

        Debug.Log($"CraftingSystem initialized with {recipes.Count} recipes");
        isInitialized = true;
    }

    public bool TryCraftItem()
    {
        var currentAvailableRecipe = GetAvailableRecipe();
        if (currentAvailableRecipe == null)
            return false;

        // Добавляем результат
        ItemStack craftItem = ItemRegistry.Instance.CreateItemStack(currentAvailableRecipe.ResultItemId, currentAvailableRecipe.ResultQuantity);
        if (craftItem == null)
        {
            Debug.LogWarning($"Crafted {currentAvailableRecipe.RecipeName} but no space in inventory!");
            return false;
        }

        // Убираем ресурсы
        for(int i = 0; i < items.Length; i++)
        {
            items[i] = null;
        }

        resultCraft = craftItem;
        Debug.Log($"Successfully crafted {currentAvailableRecipe.RecipeName}");
        OnCraftingComplete?.Invoke(currentAvailableRecipe);
        return true;
    }

    private bool CanCraft(List<Ingredient> ingredients, out CraftingRecipe recipe)
    {
        if(ingredients.Count == 0)
        {
            recipe = null;
            return false;
        }
        CraftingRecipe result = null;
        foreach (var r in recipes)
        {
            Ingredient[] difference = r.Ingredients.Except(ingredients).ToArray();
            if (difference.Length == 0)
            {
                result = r;
                break;
            }
        }

        if(result != null)
        {
            recipe = result;
            return true;
        }
        else
        {
            recipe = null;
            return false;
        }
    }

    // Метод для получения доступных рецептов
    public CraftingRecipe GetAvailableRecipe()
    {
        var ingredients = new List<Ingredient>();
        foreach (var stack in items)
        {
            if(stack == null)
                continue;
            if(ingredients.Any(i => i.ItemId == stack.ItemId))
            {
                ingredients[ingredients.FindIndex(i => i.ItemId == stack.ItemId)].Quantity++;
            }
            else
            {
                ingredients.Add(new Ingredient(stack.ItemId, 1));
            }
        }

        if(CanCraft(ingredients, out CraftingRecipe recipe))
        {
            return recipe;
        }
        else
        {
            return null;
        }
    }

    //Обработка завершения перетаскивания
    private void HandleDragEnd(DragAndDropMessage message)
    {
        Debug.Log($"Drag & Drop: {message.From.ToString()}");

        if (message.To != null && message.TryGetFrom(out CraftingSlotIdentifier craftFromSlot) && message.TryGetTo(out CraftingSlotIdentifier craftToSlot))
        {
            if (IsPositionValid(craftToSlot.Slot)) // Валидная позиция
            {
                TryMoveItem(craftFromSlot.Slot, craftToSlot.Slot);
            }
        }
        else if (message.To != null && message.TryGetFrom(out craftFromSlot) && !message.TryGetTo(out craftToSlot))
        {
            if (IsPositionValid(craftFromSlot.Slot) || craftFromSlot.Slot == 10) // Валидная позиция
            {
                ClearItem(craftFromSlot.Slot);
            }
        }
        else if (message.To != null && !message.TryGetFrom(out craftFromSlot) && message.TryGetTo(out craftToSlot))
        {
            if (IsPositionValid(craftToSlot.Slot)) // Валидная позиция
            {
                TryAddItem(message.Item, craftToSlot.Slot);
            }
        }
        else if (message.To == null)
        {
            if (!message.TryGetFrom(out craftFromSlot)) return;
            // Drop outside inventory - удаляем предмет
            ClearItem(craftFromSlot.Slot);
            Debug.Log($"Item dropped out of inventory from {craftFromSlot.Slot}");
        }
    }

    // Перемещение предмета между слотами
    public bool TryMoveItem(int fromPosition, int toPosition)
    {
        if ((!IsPositionValid(fromPosition) && fromPosition != 10) || !IsPositionValid(toPosition))
            return false;

        if (!GetSlotStatus(fromPosition)) return false;

        // SWAP: Если целевой слот пустой или содержит другой предмет
        if (!GetSlotStatus(toPosition))
        {
            // Просто перемещаем
            items[toPosition] = GetItem(fromPosition);
            ClearItem(fromPosition);
            OnItemsMoved?.Invoke(fromPosition, toPosition);
            return true;
        }
        else
        {
            if (fromPosition == 10)
            {
                return false;
            }
            // Меняем местами
            ItemStack toItemStack = GetItem(toPosition);
            items[toPosition] = items[fromPosition];
            items[fromPosition] = toItemStack;
            OnItemsMoved?.Invoke(fromPosition, toPosition);
            return true;
        }
    }

    // Вспомогательные методы
    private bool IsPositionValid(int position)
    {
        return position < items.Length && position > -1;
    }

    public bool GetSlotStatus(int index)
    {
        if(index == 10)
        {
            return resultCraft != null;
        }
        return items[index] != null;
    }

    public void ClearItem(int index)
    {
        if(index == 10)
        {
            OnItemRemoved?.Invoke(index, resultCraft);
            resultCraft = null;
            return;
        }

        if (!IsPositionValid(index) || !GetSlotStatus(index))
            return;

        OnItemRemoved?.Invoke(index, items[index]);
        items[index] = null;
    }

    public ItemStack GetItem(int index)
    {
        if (index == 10)
        {
            return resultCraft;
        }
        return items[index];
    }

    public bool TryAddItem(ItemStack itemStack, int position)
    {
        if (!IsPositionValid(position) || itemStack == null)
            return false;

        if (GetSlotStatus(position))
        {
            return false;
        }

        items[position] = itemStack;
        OnItemAdded?.Invoke(position, itemStack);
        return true;
    }
}
