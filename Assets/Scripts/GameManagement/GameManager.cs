using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Game state
    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; }

    // Game statistics
    public int Money { get; private set; }
    public int CustomersSatisfied { get; private set; }
    public int CurrentDay { get; private set; }
    
    // Scene names
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "GamePlay";

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            ///DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetDefaultValues();
        
        
        // Check current scene
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == mainMenuSceneName)
        {
            CurrentState = GameState.MainMenu;
        }
        else if (currentScene.name == gameSceneName)
        {
            
            CurrentState = GameState.Playing;
        }
    }

    private void SetDefaultValues()
    {
        Money = 100;
        CustomersSatisfied = 0;
        CurrentDay = 1;
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1;
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Debug.Log("Game started!");
        Time.timeScale = 1;    
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ReturnToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        
        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0;
        }
        else if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1;
        }
    }

    public void EndDay()
    {
        CurrentDay++;
        Debug.Log($"Day {CurrentDay} started!");
    }

    public void AddMoney(int amount)
    {
        Money += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            return true;
        }
        return false;
    }

    public void CustomerServed(int satisfaction)
    {
        CustomersSatisfied++;
        AddMoney(satisfaction * 5); // Money earned depends on satisfaction
    }

    public void GameOver()
    {
        CurrentState = GameState.GameOver;
        Debug.Log("Game Over!");
    }
    
    public void RestartGame()
    {
        SetDefaultValues();
        StartGame();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
} 