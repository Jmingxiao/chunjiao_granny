using UnityEngine;

[CreateAssetMenu(fileName = "TutorialData", menuName = "Shawarma Granny/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [System.Serializable]
    public class TutorialSlide
    {
        public string title;
        [TextArea(3, 8)]
        public string description;
        public Sprite image;
    }
    
    public TutorialSlide[] slides = new TutorialSlide[]
    {
        new TutorialSlide
        {
            title = "Welcome to Shawarma Granny!",
            description = "Learn how to run your own shawarma restaurant and become the best shawarma chef in town!"
        },
        new TutorialSlide
        {
            title = "Cooking Station",
            description = "Use the cooking station to prepare meat. Raw meat needs to be cooked before serving to customers."
        },
        new TutorialSlide
        {
            title = "Vegetable Station",
            description = "Prepare fresh vegetables at this station. Customers love fresh veggies in their shawarma!"
        },
        new TutorialSlide
        {
            title = "Assembly Station",
            description = "Combine cooked meat and vegetables to assemble a delicious shawarma wrap."
        },
        new TutorialSlide
        {
            title = "Counter Station",
            description = "Serve shawarma to your customers at the counter. Happy customers will tip more!"
        },
        new TutorialSlide
        {
            title = "Seating Area",
            description = "Customers will eat their shawarma in the seating area. Make sure to clean tables between customers."
        },
        new TutorialSlide
        {
            title = "Upgrading Your Restaurant",
            description = "Earn money to upgrade your equipment and restaurant. Better equipment means faster cooking!"
        },
        new TutorialSlide
        {
            title = "Tips and Tricks",
            description = "1. Keep an eye on cooking timers\n2. Serve customers in order of arrival\n3. Upgrade equipment regularly\n4. Keep your stations clean\n5. Balance quality and speed"
        }
    };
} 