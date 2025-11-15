using TMPro;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemStatsText;

    private RectTransform tooltipRect;
    private Vector2 offset = new Vector2(15, -15);
    private float showDelay = 0.5f;
    private float hideDelay = 0.3f;

    private float currentShowTimer;
    private float currentHideTimer;
    [SerializeField] private bool isWaitingToShow = false;
    [SerializeField] private bool isWaitingToHide = false;

    private ItemStack currentItemStack;
    private Vector2 lastMousePosition;

    public static TooltipSystem Instance { get; private set; }

    public void Initialize()
    {
        if (Instance != null)
        {
            Debug.LogWarning("TooltipSystem already initialized!");
            return;
        }

        Instance = this;
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipPanel.SetActive(false);

        Debug.Log("TooltipSystem initialized");
    }

    public void RequestShowTooltip(ItemStack itemStack, Vector2 screenPosition)
    {
        if (itemStack == null)
        {
            RequestHideTooltip();
            return;
        }

        // Если уже показываем этот же предмет - просто обновляем позицию
        if (tooltipPanel.activeInHierarchy && currentItemStack == itemStack)
        {
            lastMousePosition = screenPosition;
            return;
        }

        // Отменяем скрытие если оно было запланировано
        if (isWaitingToHide)
        {
            isWaitingToHide = false;
            currentHideTimer = 0f;
        }

        currentItemStack = itemStack;
        lastMousePosition = screenPosition;
        isWaitingToShow = true;
        currentShowTimer = showDelay;

        // Устанавливаем данные сразу
        itemNameText.text = itemStack.ItemData.itemName;
        itemDescriptionText.text = itemStack.ItemData.description;

        string stats = "";
        if (itemStack.ItemData.IsStackable)
        {
            stats += $"Max Stack: {itemStack.ItemData.maxStackSize}\n";
            stats += $"Quantity: {itemStack.Quantity}/{itemStack.MaxStackSize}";
        }
        else
        {
            stats += "Not Stackable";
        }

        itemStatsText.text = stats;
    }

    public void RequestHideTooltip()
    {
        if (!tooltipPanel.activeInHierarchy && !isWaitingToShow)
            return;

        isWaitingToShow = false;
        currentShowTimer = 0f;

        isWaitingToHide = true;
        currentHideTimer = hideDelay;
    }

    public void ForceHideTooltip()
    {
        isWaitingToShow = false;
        isWaitingToHide = false;
        tooltipPanel.SetActive(false);
        currentItemStack = null;
    }

    private void Update()
    {
        // Обработка показа
        if (isWaitingToShow)
        {
            currentShowTimer -= Time.deltaTime;
            if (currentShowTimer <= 0)
            {
                ShowTooltip();
            }
        }

        // Обработка скрытия
        if (isWaitingToHide)
        {
            currentHideTimer -= Time.deltaTime;
            if (currentHideTimer <= 0)
            {
                HideTooltip();
            }
        }

        // Обновление позиции если тултип активен
        if (tooltipPanel.activeInHierarchy)
        {
            PositionTooltip(Input.mousePosition);
        }
    }

    private void ShowTooltip()
    {
        if (currentItemStack == null)
        {
            ForceHideTooltip();
            return;
        }

        tooltipPanel.SetActive(true);
        PositionTooltip(lastMousePosition);
        isWaitingToShow = false;

        Debug.Log($"Tooltip shown: {currentItemStack.ItemData.itemName}");
    }

    private void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        isWaitingToHide = false;
        currentItemStack = null;

        Debug.Log("Tooltip hidden");
    }

    private void PositionTooltip(Vector2 screenPosition)
    {
        Vector2 position = screenPosition + offset;

        // Проверяем чтобы тултип не выходил за экран
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (position.x + tooltipSize.x > screenSize.x)
            position.x = screenPosition.x - tooltipSize.x - offset.x;

        if (position.y - tooltipSize.y < 0)
            position.y = screenPosition.y + tooltipSize.y + Mathf.Abs(offset.y);

        tooltipRect.position = position;
    }
}
