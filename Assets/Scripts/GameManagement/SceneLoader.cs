using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    
    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider loadingBar;
    public Text loadingText;
    
    [Header("Transition Effects")]
    public Animator transitionAnimator;
    public float transitionTime = 1.0f;
    
    private string sceneToLoad;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Hide loading screen initially
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
    
    public void LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        StartCoroutine(LoadSceneWithTransition());
    }
    
    private IEnumerator LoadSceneWithTransition()
    {
        // Start transition if animator exists
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
        }
        
        // Show loading screen
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }
        
        // Load scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;
        
        float progress = 0f;
        
        // Update loading bar
        while (progress < 0.9f)
        {
            progress = Mathf.Lerp(progress, operation.progress, Time.deltaTime * 5f);
            
            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }
            
            if (loadingText != null)
            {
                loadingText.text = $"Loading: {Mathf.RoundToInt(progress * 100)}%";
            }
            
            yield return null;
        }
        
        // Let's add a small delay to ensure everything is ready
        yield return new WaitForSeconds(0.5f);
        
        // Allow scene activation
        operation.allowSceneActivation = true;
        
        // Wait for scene to fully load
        while (!operation.isDone)
        {
            yield return null;
        }
        
        // Hide loading screen
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
        
        // End transition if animator exists
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("End");
        }
    }
} 