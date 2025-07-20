using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Image and Button
using System.Collections.Generic;

public class CustomerMenuDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Image component for displaying the main dish sprite.")]
    [SerializeField] private Image dishImage;
    [Tooltip("List of Image components for displaying ingredient sprites. Ensure there are enough for your menus.")]
    [SerializeField] private List<Image> ingredientImages;
    [Tooltip("The Button component that the player will click to take the order.")]
    [SerializeField] private Button clickButton;

    private CustomerNPC ownerNPC; // Reference to the customer NPC that owns this bubble

    private void Awake()
    {
        // Basic validation for UI references
        if (dishImage == null) Debug.LogError("Dish Image not assigned in CustomerMenuDisplay!", this);
        if (ingredientImages == null || ingredientImages.Count == 0) Debug.LogWarning("No Ingredient Images assigned in CustomerMenuDisplay. Menu might not display correctly.", this);
        if (clickButton == null) Debug.LogError("Click Button not assigned in CustomerMenuDisplay!", this);
    }

    /// <summary>
    /// Sets up the visual display of the menu bubble and links it to its owner NPC.
    /// This method is called by the CustomerNPC when it spawns.
    /// </summary>
    /// <param name="menu">The CustomerMenu ScriptableObject containing the order details.</param>
    /// <param name="npc">The CustomerNPC instance that owns this bubble.</param>
    public void SetupMenu(CustomerMenu menu, CustomerNPC npc)
    {
        ownerNPC = npc;

        if (menu == null)
        {
            Debug.LogError("Attempted to set up menu display with a null CustomerMenu!", this);
            return;
        }

        // Set the dish sprite
        if (dishImage != null)
        {
            dishImage.sprite = menu.dishSprite;
            dishImage.gameObject.SetActive(menu.dishSprite != null);
        }

        // Set ingredient sprites, activating only as many as needed
        for (int i = 0; i < ingredientImages.Count; i++)
        {
            if (i < menu.ingredientSprites.Count)
            {
                ingredientImages[i].sprite = menu.ingredientSprites[i];
                ingredientImages[i].gameObject.SetActive(true);
            }
            else
            {
                ingredientImages[i].gameObject.SetActive(false); // Hide unused ingredient slots
            }
        }

        // Add listener for the button click (taking the order)
        if (clickButton != null)
        {
            clickButton.onClick.RemoveAllListeners(); // Remove previous listeners to prevent duplicates
            clickButton.onClick.AddListener(OnOrderBubbleClicked);
            clickButton.interactable = true; // Ensure button is interactable
        }

        // Activate the whole display object
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called when the player clicks the order bubble (button).
    /// </summary>
    private void OnOrderBubbleClicked()
    {
        if (ownerNPC != null)
        {
            Debug.Log($"Order for {ownerNPC.MenuData.dishName} taken by player!");
            ownerNPC.TakeOrder(); // Notify the NPC that its order has been taken
            // The NPC will handle deactivating/destroying this bubble
        }
        else
        {
            Debug.LogWarning("CustomerMenuDisplay clicked but no ownerNPC assigned!", this);
        }

        // Make button non-interactable immediately after click to prevent multiple clicks
        if (clickButton != null)
        {
            clickButton.interactable = false;
        }
    }
}
