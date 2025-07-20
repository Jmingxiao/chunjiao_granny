using UnityEngine;
using System.Collections.Generic;

// Use this attribute to create new instances of this ScriptableObject via the Unity Editor
// Right-click in Project window -> Create -> Game -> Customer Menu
[CreateAssetMenu(fileName = "NewCustomerMenu", menuName = "Game/Customer Menu")]
public class CustomerMenu : ScriptableObject
{
    [Header("Dish Details")]
    [Tooltip("Name of the dish.")]
    public string dishName = "Mystery Dish";
    [Tooltip("Sprite representing the final dish.")]
    public Sprite dishSprite;
    [Tooltip("List of sprites representing the ingredients needed for this dish.")]
    public List<Sprite> ingredientSprites;

    [Header("Difficulty & Timing")]
    [Tooltip("A multiplier for the max waiting time, representing cooking difficulty. (e.g., 1.0 for easy, 3.0 for hard).")]
    [Range(0.5f, 5f)]
    public float cookingDifficulty = 1.0f;
    [Tooltip("Base time (seconds) a customer will wait AFTER their order is taken.")]
    public float baseOrderCountdownTime = 60f;

    /// <summary>
    /// Calculates the actual order countdown time based on base time and cooking difficulty.
    /// </summary>
    public float CalculatedOrderCountdownTime => baseOrderCountdownTime * cookingDifficulty;

    [Header("Rating Details")]
    [Tooltip("Minimum completeness percentage for a dish to be considered acceptable (0.0 to 1.0).")]
    [Range(0f, 1f)]
    public float minAcceptableCompleteness = 0.5f;

    [System.Serializable]
    public struct RatingThreshold
    {
        [Tooltip("The completeness percentage (0.0 to 1.0) at or above which this score applies.")]
        [Range(0f, 1f)]
        public float completenessPercentage;
        [Tooltip("The score awarded for this completeness level.")]
        public int score;
    }

    [Tooltip("Define scores based on completeness percentage. Ensure percentages are in ascending order.")]
    public List<RatingThreshold> ratingThresholds;

    /// <summary>
    /// Gets the rating score for a given completeness percentage.
    /// </summary>
    /// <param name="completeness">The completeness of the dish (0.0 to 1.0).</param>
    /// <returns>The rating score, or 0 if below minimum acceptable completeness.</returns>
    public int GetRating(float completeness)
    {
        if (completeness < minAcceptableCompleteness)
        {
            return 0; // Dish is not acceptable
        }

        // Sort rating thresholds in ascending order of completeness percentage
        // This ensures we always find the highest applicable score
        ratingThresholds.Sort((a, b) => a.completenessPercentage.CompareTo(b.completenessPercentage));

        int awardedScore = 0;
        foreach (var threshold in ratingThresholds)
        {
            if (completeness >= threshold.completenessPercentage)
            {
                awardedScore = threshold.score;
            }
            else
            {
                // Since thresholds are sorted, we can break early if we've passed the applicable range
                break;
            }
        }
        return awardedScore;
    }
}
