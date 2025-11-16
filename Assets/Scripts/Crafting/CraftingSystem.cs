using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CraftingSystem : MonoBehaviour
{
    [SerializeField] private List<CraftingRecipe> recipes;

    private ItemStack[] items = new ItemStack[9];
    private ItemStack resultCraft;

    private bool isInitialized = false;

    public event System.Action<CraftingRecipe> OnCraftingPossible;
    public event System.Action<CraftingRecipe> OnCraftingComplete;
    public event System.Action<int, ItemStack> OnItemAdded;
    public event System.Action<int> OnItemRemoved;
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
        Debug.Log($"Can Craft Process: Check Ingredients {ingredients.Count}");
        if (ingredients.Count == 0)
        {
            recipe = null;
            return false;
        }
        CraftingRecipe result = null;
        foreach (var r in recipes)
        {
            Ingredient[] difference = r.Ingredients.Except(ingredients).ToArray();
            Debug.Log($"Can Craft Process: Check Recipes {difference.Length == 0} / {r.Ingredients.Count} / {r.IngredientsQuantity}");
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
                int index = ingredients.FindIndex(i => i.ItemId == stack.ItemId);
                ingredients[index].Quantity++;
                Debug.Log($"Get Available Recipe Process: Increment {stack.ItemId} -> {ingredients[index].Quantity}");
            }
            else
            {
                ingredients.Add(new Ingredient(stack.ItemId, 1));
                Debug.Log($"Get Available Recipe Process: Add {stack.ItemId} -> {ingredients[ingredients.Count -1].Quantity}");
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
    private void HandleDragEnd(DragAndDropMessage message, bool success)
    {
        if (!success) return;
        Debug.Log($"Crafting System Drag & Drop: {message.From.ToString()} -> {message.To?.ToString()}");
        Debug.Log($"Crafting System Drag  & Drop Status: {message.TryGetFrom(out CraftingSlotIdentifier craftFromSlot)}/{message.TryGetTo(out CraftingSlotIdentifier craftToSlot)}");

        if (message.To != null && message.TryGetFrom(out craftFromSlot) && message.TryGetTo(out craftToSlot))
        {
            if (IsPositionValid(craftToSlot.Slot)) // Валидная позиция
            {
                TryMoveItem(craftFromSlot.Slot, craftToSlot.Slot);
            }
        }
        else if (message.To != null && message.TryGetFrom(out craftFromSlot) && !message.TryGetTo(out craftToSlot))
        {
            if (IsPositionValid(craftFromSlot.Slot)) // Валидная позиция
            {
                ClearItem(craftFromSlot.Slot);
            }
        }
        else if (message.To != null && !message.TryGetFrom(out craftFromSlot) && message.TryGetTo(out craftToSlot))
        {
            if (resultCraft != null)
                return;

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

        if (toPosition == 10) return false;

        if (!GetSlotStatus(toPosition))
        {
            // Просто перемещаем
            items[toPosition] = GetItem(fromPosition);
            ClearItem(fromPosition);
            Debug.Log($"Craft UI: Move {fromPosition} -> {toPosition}");
            OnItemsMoved?.Invoke(fromPosition, toPosition);
            return true;
        }
        // SWAP: Если целевой слот пустой или содержит другой предмет
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
            Debug.Log($"Craft UI: SWAP {fromPosition} -> {toPosition}");
            OnItemsMoved?.Invoke(fromPosition, toPosition);
            return true;
        }
    }

    // Вспомогательные методы
    private bool IsPositionValid(int position)
    {
        return (position < items.Length && position > -1) || position == 10;
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
        if (!IsPositionValid(index) || !GetSlotStatus(index))
            return;

        OnItemRemoved?.Invoke(index);
        if (index == 10)
            resultCraft = null;
        else
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

        Debug.Log($"Try Add Item Check Valid Data: {!IsPositionValid(position)} || {itemStack == null}");

        if (GetSlotStatus(position))
        {
            return false;
        }

        Debug.Log($"Try Add Item Check Slot Status: {GetSlotStatus(position)}");

        if (position == 10)
        {
            return false;
        }

        Debug.Log($"Try Add Item Check Result Slot: {position == 10}");

        items[position] = itemStack;
        Debug.Log($"Craft UI: Add {itemStack.ItemId} -> {position}");
        OnItemAdded?.Invoke(position, itemStack);
        return true;
    }
}
