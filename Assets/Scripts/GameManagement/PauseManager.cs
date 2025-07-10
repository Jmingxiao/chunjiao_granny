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
    }

    public void ResumeGame()
    {
        gamemanager.PauseGame();
        pauseCanvas.gameObject.SetActive(false);
    }
    private void Start()
    {
        pauseCanvas.gameObject.SetActive(false);
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
    }
  
}
