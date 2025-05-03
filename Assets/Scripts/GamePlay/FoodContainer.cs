using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : MonoBehaviour
{
    private SpriteRenderer meshRenderer;
    Material material;
    int foodCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<SpriteRenderer>();
        if (meshRenderer)
            material = meshRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed");
            foodCount++;
            foodCount%= 3;
            material.SetInt("_Blend", foodCount);
        }
    }
}
