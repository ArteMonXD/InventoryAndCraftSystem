using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Scriptable Objects/CraftingRecipe")]
public class CraftingRecipe : ScriptableObject
{
    public string RecipeName;
    public List<Ingredient> Ingredients;
    public string ResultItemId;
    public int ResultQuantity = 1;
    public int IngredientsQuantity 
    {
        get
        {
            int sum = 0;
            foreach(Ingredient ing in Ingredients)
            {
                sum += ing.Quantity;
            }
            return sum;
        }
    }
}

[System.Serializable]
public class Ingredient
{
    public string ItemId;
    public int Quantity;

    public Ingredient(string itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}
