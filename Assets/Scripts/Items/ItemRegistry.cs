using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    [SerializeField] private List<ItemData> allItems;

    private Dictionary<string, ItemData> itemDictionary;

    public static ItemRegistry Instance { get; private set; }

    public void Initialize()
    {
        if (Instance != null)
        {
            Debug.LogWarning("ItemRegistry already initialized!");
            return;
        }

        Instance = this;
        InitializeDictionary();

        Debug.Log($"ItemRegistry initialized with {itemDictionary.Count} items");
    }

    private void InitializeDictionary()
    {
        itemDictionary = new Dictionary<string, ItemData>();
        foreach (var item in allItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemId))
            {
                if (itemDictionary.ContainsKey(item.itemId))
                {
                    Debug.LogWarning($"Duplicate item ID: {item.itemId}");
                }
                else
                {
                    itemDictionary[item.itemId] = item;
                }
            }
        }
    }

    // Простой метод получения данных предмета
    public ItemData GetItemData(string itemId)
    {
        if (itemDictionary.TryGetValue(itemId, out var itemData))
            return itemData;

        Debug.LogError($"Item with ID '{itemId}' not found in registry!");
        return null;
    }

    // Создание стака предметов (простая утилита)
    public ItemStack CreateItemStack(string itemId, int quantity = 1)
    {
        var itemData = GetItemData(itemId);
        if (itemData != null)
            return new ItemStack(itemData, quantity);

        return null;
    }

    // Генерация случайных стартовых предметов
    public List<ItemStack> GenerateRandomStartingItems()
    {
        var randomItems = new List<ItemStack>();

        // Ресурсы для старта игры
        string[] startingResources = { "wood", "stone", "iron" };

        for (int i = 0; i < 8; i++)
        {
            var randomItemId = startingResources[Random.Range(0, startingResources.Length)];
            var quantity = Random.Range(1, 5);
            randomItems.Add(CreateItemStack(randomItemId, quantity));
        }

        return randomItems;
    }
}
