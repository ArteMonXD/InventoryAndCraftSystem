using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Initialization Order")]
    [SerializeField] private ItemRegistry itemRegistry;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private CraftingSystem craftingSystem;
    [SerializeField] private TooltipSystem tooltipSystem;
    [SerializeField] private InventoryUIController inventoryUI;
    [SerializeField] private CraftingUI craftingUI;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private void Awake()
    {
        Log("=== GAME INITIALIZATION STARTED ===");
        InitializeSystems();
        Log("=== GAME INITIALIZATION COMPLETED ===");
    }

    private void InitializeSystems()
    {
        // Шаг 1: Базовые системы данных
        InitializeSystem("Item Registry", itemRegistry, () =>
            itemRegistry != null && ItemRegistry.Instance != null);

        // Шаг 2: Игровые системы
        InitializeSystem("Inventory System", inventorySystem, () =>
            inventorySystem != null);
        InitializeSystem("Crafting System", craftingSystem, () =>
            craftingSystem != null);

        // Шаг 3: UI системы
        InitializeSystem("Tooltip System", tooltipSystem, () =>
            tooltipSystem != null && TooltipSystem.Instance != null);
        InitializeSystem("Inventory UI", inventoryUI, () =>
            inventoryUI != null);
        InitializeSystem("Crafting UI", craftingUI, () =>
            craftingUI != null);

        // Шаг 4: Запуск игровой логики
        StartGameLogic();
    }

    private void InitializeSystem(string systemName, MonoBehaviour system, System.Func<bool> initializationCheck)
    {
        if (system == null)
        {
            LogError($"{systemName} is not assigned in GameInitializer!");
            return;
        }

        if (!system.gameObject.activeInHierarchy)
        {
            LogError($"{systemName} GameObject is inactive!");
            return;
        }

        Log($"Initializing {systemName}...");

        // Вызываем метод Initialize если он есть
        var initializeMethod = system.GetType().GetMethod("Initialize");
        if (initializeMethod != null)
        {
            initializeMethod.Invoke(system, null);
            Log($"{systemName} initialized via Initialize() method");
        }
        else
        {
            // Активируем компонент если он был выключен
            system.enabled = true;
            Log($"{systemName} activated");
        }

        // Проверяем успешность инициализации
        if (initializationCheck != null && !initializationCheck())
        {
            LogError($"{systemName} failed initialization check!");
        }
    }

    private void StartGameLogic()
    {
        Log("Starting game logic...");

        // Запускаем генерацию начальных предметов
        if (inventorySystem != null)
        {
            // Вызываем метод заполнения инвентаря
            var fillMethod = inventorySystem.GetType().GetMethod("FillWithRandomItems");
            if (fillMethod != null)
            {
                fillMethod.Invoke(inventorySystem, null);
                Log("Inventory filled with random items");
            }
        }

        // Обновляем UI крафта
        if (craftingUI != null)
        {
            var updateMethod = craftingUI.GetType().GetMethod("UpdateCraftingUI");
            if (updateMethod != null)
            {
                updateMethod.Invoke(craftingUI, null);
                Log("Crafting UI updated");
            }
        }
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log($"[GameInitializer] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[GameInitializer] {message}");
    }
}
