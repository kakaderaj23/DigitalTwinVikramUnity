using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject welcomeCanvas;
    public Button proceedButton;
    public GameObject pauseCanvas;
    public Button exitButton;

    [Header("Player Reference")]
    public GameObject playerGameObject;

    private bool isPaused = false;

    void Start()
    {
        // Show welcome screen
        welcomeCanvas.SetActive(true);
        proceedButton.onClick.AddListener(OnProceedClicked);

        // Hide pause UI initially
        pauseCanvas.SetActive(false);
        isPaused = false;

        exitButton.onClick.AddListener(OnExitClicked);
    }

    void Update()
    {
        // Toggle pause menu with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pauseCanvas.SetActive(isPaused);
        }
    }

    // Called when "Proceed" button is clicked
    public void OnProceedClicked()
    {
        welcomeCanvas.SetActive(false);

        // Set gameStarted = true on the player's Walking Script
        var walkingScript = playerGameObject.GetComponent<WalkingScript>();
        if (walkingScript != null)
        {
            walkingScript.gameStarted = true;
        }
        else
        {
            Debug.LogWarning("WalkingScript not found on playerGameObject.");
        }
    }

    // Called when "Exit" button is clicked
    public void OnExitClicked()
    {
        Debug.Log("Exiting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
