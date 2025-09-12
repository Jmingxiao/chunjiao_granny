using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameManager gamemanager;
    public Button pauseButton;
    public Button resumeButton;
    public Canvas pauseCanvas;

    public void PauseGame()
    {
        gamemanager.PauseGame();
        pauseCanvas.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
       /// StartCoroutine(ShowWithDelay(pauseButton, 1f));
    }

    public void ResumeGame()
    {
        gamemanager.PauseGame();
        pauseCanvas.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }
    private void Start()
    {
        gamemanager = GameManager.Instance;
        
        pauseCanvas.gameObject.SetActive(false);
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
    }
    IEnumerator ShowWithDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }
    
}
