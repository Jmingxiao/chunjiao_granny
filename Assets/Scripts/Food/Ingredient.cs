using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
   [SerializeField] private IngredientData data;
    
    public IngredientData Data => data;
    
    void Start()
    {
        if (data == null)
        {
            Debug.LogWarning($"食材 {gameObject.name} 没有设置IngredientData!");
        }
    }
    
    public void SetIngredientData(IngredientData ingredientData)
    {
        data = ingredientData;
    }
}
