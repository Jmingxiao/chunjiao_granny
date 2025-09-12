using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject tutorialPanel;
    
    [Header("Main Menu Buttons")]
    public Button startGameButton;
    public Button settingsButton;
    public Button tutorialButton;
    public Button quitButton;
    
    [Header("Settings Elements")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;
    public Button backFromSettingsButton;
    
    [Header("Tutorial Elements")]
    public Button backFromTutorialButton;
    public Button nextTutorialButton;
    public Button prevTutorialButton;
    public Image tutorialImage;
    public TextMeshProUGUI tutorialText;
    
    [Header("Tutorial Content")]
    public Sprite[] tutorialSprites;
    public string[] tutorialTexts;
    private int currentTutorialIndex = 0;
    
    [Header("Animation")]
    public Animator logoAnimator;
    public float buttonAnimationDelay = 0.1f;
    
    private void Start()
    {
        // Set up button listeners
        if (startGameButton) startGameButton.onClick.AddListener(StartGame);
        if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
        if (tutorialButton) tutorialButton.onClick.AddListener(OpenTutorial);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);
        
        if (backFromSettingsButton) backFromSettingsButton.onClick.AddListener(CloseSettings);
        if (backFromTutorialButton) backFromTutorialButton.onClick.AddListener(CloseTutorial);
        
        if (nextTutorialButton) nextTutorialButton.onClick.AddListener(NextTutorial);
        if (prevTutorialButton) prevTutorialButton.onClick.AddListener(PreviousTutorial);
        
        // Initialize panels
        ShowMainPanel();
        
        // Set initial settings values
        InitializeSettings();
        
        // Start with first tutorial slide
        UpdateTutorialSlide();
    }
    
    #region Panel Management
    
    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        tutorialPanel.SetActive(false);
    }
    
    public void ShowSettingsPanel()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        tutorialPanel.SetActive(false);
    }
    
    public void ShowTutorialPanel()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }
    
    #endregion
    
    #region Button Actions
    
    public void StartGame()
    {
        // Load the game scene
        SceneManager.LoadScene("Shawarma Game");
    }
    
    public void OpenSettings()
    {
        ShowSettingsPanel();
    }
    
    public void CloseSettings()
    {
        // Save settings before closing
        SaveSettings();
        ShowMainPanel();
    }
    
    public void OpenTutorial()
    {
        currentTutorialIndex = 0;
        UpdateTutorialSlide();
        ShowTutorialPanel();
    }
    
    public void CloseTutorial()
    {
        ShowMainPanel();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Tutorial Management
    
    public void NextTutorial()
    {
        if (currentTutorialIndex < tutorialSprites.Length - 1)
        {
            currentTutorialIndex++;
            UpdateTutorialSlide();
        }
    }
    
    public void PreviousTutorial()
    {
        if (currentTutorialIndex > 0)
        {
            currentTutorialIndex--;
            UpdateTutorialSlide();
        }
    }
    
    private void UpdateTutorialSlide()
    {
        if (tutorialSprites != null && tutorialSprites.Length > 0 && 
            tutorialTexts != null && tutorialTexts.Length > 0)
        {
            // Update tutorial image
            if (tutorialImage != null && currentTutorialIndex < tutorialSprites.Length)
            {
                tutorialImage.sprite = tutorialSprites[currentTutorialIndex];
            }
            
            // Update tutorial text
            if (tutorialText != null && currentTutorialIndex < tutorialTexts.Length)
            {
                tutorialText.text = tutorialTexts[currentTutorialIndex];
            }
            
            // Update navigation buttons
            if (prevTutorialButton != null)
            {
                prevTutorialButton.interactable = (currentTutorialIndex > 0);
            }
            
            if (nextTutorialButton != null)
            {
                nextTutorialButton.interactable = (currentTutorialIndex < tutorialSprites.Length - 1);
            }
        }
    }
    
    #endregion
    
    #region Settings Management
    
    private void InitializeSettings()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.Save();
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        // TODO: Update actual music volume via AudioManager
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        // TODO: Update actual SFX volume via AudioManager
    }
    
    private void OnFullscreenToggled(bool isFullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        Screen.fullScreen = isFullscreen;
    }
    
    #endregion
} 