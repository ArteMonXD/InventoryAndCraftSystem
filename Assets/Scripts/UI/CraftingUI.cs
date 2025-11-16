using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private Transform slotGridParent;
    [SerializeField] private GameObject slotUIPrefab;
    private CraftingSlot[] craftingSlots;
    [SerializeField] private ResultCraftSlot resultCraftSlot;
    [SerializeField] private Button craftButton;
    [SerializeField] private TextMeshProUGUI craftButtonText;

    private CraftingSystem craftingSystem;

    private bool isInitialized = false;

    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.LogWarning("CraftingUI already initialized!");
            return;
        }

        // Находим системы
        craftingSystem = FindFirstObjectByType<CraftingSystem>();

        if (craftingSystem == null)
        {
            Debug.LogError("CraftingSystem not found for CraftingUI!");
            return;
        }

        // Настраиваем кнопку
        craftButton.onClick.AddListener(OnCraftButtonClicked);

        // Инициализируем UI
        InitializeUI();
        SubscribeToEvents();

        Debug.Log("CraftingUI initialized successfully");
        isInitialized = true;
    }

    private void InitializeUI()
    {
        if (isInitialized) return;

        FillCraftingGrid();
        resultCraftSlot.Initialize();
        UpdateAvailableRecipes();
        Debug.Log("Crafting UI updated");
    }

    private void SubscribeToEvents()
    {
        if (craftingSystem != null)
        {
            craftingSystem.OnItemAdded += OnItemAdded;
            craftingSystem.OnItemRemoved += OnItemRemoved;
            craftingSystem.OnItemsMoved += OnItemsMoved;
        }
    }

    private void OnItemsMoved(int from, int to)
    {
        if (!isInitialized) return;

        if (IsPositionValid(from) && IsPositionValid(to))
        {
            // Получаем актуальные данные из инвентаря

            // Обновляем UI
            if (!craftingSystem.GetSlotStatus(from))
            {
                if(from == 10)
                {
                    resultCraftSlot.Clear();
                }
                else
                {
                    craftingSlots[from].Clear();
                }  
            }
            else
            {
                if (from == 10)
                {
                    resultCraftSlot.DisplayItem(craftingSystem.GetItem(from));
                }
                else
                {
                    craftingSlots[from].DisplayItem(craftingSystem.GetItem(from));
                }
            }

            if (!craftingSystem.GetSlotStatus(to))
            {
                craftingSlots[to].Clear();
            }
            else
            {
                craftingSlots[to].DisplayItem(craftingSystem.GetItem(to));
            }

            UpdateAvailableRecipes();

            Debug.Log($"UI: Items moved from {from} to {to}");
        }
    }

    private void OnItemRemoved(int pos)
    {
        if (!isInitialized) return;
        Debug.Log($"UI: Start item removed from {pos}");
        if (IsPositionValid(pos))
        {
            Debug.Log($"UI: Check Valid pos item removed from {pos} -> {IsPositionValid(pos)}");
            if (pos == 10)
            {
                resultCraftSlot.Clear();
            }
            else
            {
                craftingSlots[pos].Clear();
            }

            UpdateAvailableRecipes();

            Debug.Log($"UI: Item removed from {pos}");
        }
    }

    private void OnItemAdded(int pos, ItemStack stack)
    {
        if (!isInitialized) return;

        if (IsPositionValid(pos))
        {
            if(pos == 10)
            {
                resultCraftSlot.DisplayItem(stack);
            }
            else
            {
                craftingSlots[pos].DisplayItem(stack);
            }

            UpdateAvailableRecipes();

            Debug.Log($"UI: Item added at {pos} - {stack.ItemData.itemName}");
        }
    }

    private void FillCraftingGrid()
    {
        craftingSlots = new CraftingSlot[9];

        // Очищаем старые слоты если есть
        foreach (Transform child in slotGridParent)
        {
            Destroy(child.gameObject);
        }

        // Создаем новые слоты
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            var slotGO = Instantiate(slotUIPrefab, slotGridParent);
            var slotUI = slotGO.GetComponent<CraftingSlot>();

            if (slotUI != null)
            {
                slotUI.Initialize(i);
                craftingSlots[i] = slotUI;
            }
            else
            {
                Debug.LogError("CraftSlot component not found on slot prefab!");
            }
        }

        Debug.Log($"Created {craftingSlots.Length} craft slots");

        resultCraftSlot.DisplayItem(null);
        craftButton.interactable = false;
    }

    private void UpdateAvailableRecipes()
    {
        if (craftingSystem == null) return;

        var availableRecipes = craftingSystem.GetAvailableRecipe();

        if (availableRecipes != null)
        {
            DisplayRecipe(availableRecipes);
            Debug.Log($"Found {availableRecipes.RecipeName} available recipe");
        }
        else
        {
            craftButtonText.text = "Cannot Craft";
            craftButton.interactable = false;
            Debug.Log("No available recipes");
        }
    }

    private void DisplayRecipe(CraftingRecipe recipe)
    {
        if (recipe == null) return;

        // Отображаем результат
        var resultItemData = ItemRegistry.Instance.GetItemData(recipe.ResultItemId);
        if (resultItemData != null)
        {
            ItemStack resultItem = new ItemStack(resultItemData, recipe.ResultQuantity);
            resultCraftSlot.DisplayItem(resultItem);
        }

        craftButton.interactable = true;
        craftButtonText.text = $"Craft {recipe.RecipeName}";
    }

    private void OnCraftButtonClicked()
    {
        if (!isInitialized) return;

        bool success = craftingSystem.TryCraftItem();
        if (success)
        {
            UpdateCraftingUI();
            resultCraftSlot.SwitchStatus(true);
            Debug.Log($"Successfully crafted");
        }
        else
        {
            Debug.LogWarning($"Failed to craft");
        }
    }

    private void UpdateCraftingUI()
    {
        foreach (CraftingSlot slot in craftingSlots)
        {
            if (slot == null) continue;
            var item = craftingSystem.GetItem(slot.GridIndex);
            slot.DisplayItem(item);
        }
        UpdateAvailableRecipes();
    }

    private bool IsPositionValid(int position)
    {
        return (position < craftingSlots.Length && position > -1) || position == 10;
    }

    private void OnDestroy()
    {
        if (craftingSystem != null && isInitialized)
        {
            craftingSystem.OnItemAdded -= OnItemAdded;
            craftingSystem.OnItemRemoved -= OnItemRemoved;
            craftingSystem.OnItemsMoved -= OnItemsMoved;
        }
    }
}
