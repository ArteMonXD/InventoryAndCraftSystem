using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public string description;
    public int maxStackSize = 1;
    public Sprite icon;

    public bool IsStackable => maxStackSize > 1;
}
