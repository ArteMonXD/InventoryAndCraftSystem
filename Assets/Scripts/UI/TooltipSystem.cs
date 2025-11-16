using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemStatsText;

    private RectTransform tooltipRect;
    private Vector2 offset = new Vector2(10, -10);

    public static TooltipSystem Instance { get; private set; }

    public void Initialize()
    {
        if (Instance != null)
        {
            Debug.LogWarning("TooltipSystem already initialized!");
            return;
        }

        var textGraphic = itemNameText.GetComponent<Graphic>();
        if (textGraphic != null)
            textGraphic.raycastTarget = false;

        textGraphic = itemDescriptionText.GetComponent<Graphic>();
        if (textGraphic != null)
            textGraphic.raycastTarget = false;

        textGraphic = itemStatsText.GetComponent<Graphic>();
        if (textGraphic != null)
            textGraphic.raycastTarget = false;

        Instance = this;
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        HideTooltip();

        Debug.Log("TooltipSystem initialized");
    }

    public void ShowTooltip(ItemData itemData, Vector2 screenPosition)
    {
        if (itemData == null) return;

        // Устанавливаем текст
        itemNameText.text = itemData.itemName;
        itemDescriptionText.text = itemData.description;

        // Добавляем статистику
        string stats = "";
        if (itemData.IsStackable)
        {
            stats += $"Max Stack: {itemData.maxStackSize}\n";
        }
        else
        {
            stats += "Not Stackable\n";
        }

        itemStatsText.text = stats;

        // Позиционируем тултип
        tooltipPanel.SetActive(true);
        PositionTooltip(screenPosition);
    }

    public void ShowTooltipForItemStack(ItemStack itemStack, Vector2 screenPosition)
    {
        if (itemStack == null) return;

        ShowTooltip(itemStack.ItemData, screenPosition);

        // Добавляем информацию о количестве
        if (itemStack.IsStackable)
        {
            itemStatsText.text += $"Quantity: {itemStack.Quantity}/{itemStack.MaxStackSize}";
        }
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
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

    private void Update()
    {
        if (tooltipPanel.activeInHierarchy)
        {
            // Следим за позицией мыши когда тултип активен
            PositionTooltip(Input.mousePosition);
        }
    }
}
