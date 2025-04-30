using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject welcomeCanvas;
    public Button proceedButton;
    public GameObject pauseCanvas;
    public Button exitButton;
    public GameObject gameplayRoot;

    private bool isPaused = false;

    void Start()
    {
        // Show welcome screen
        welcomeCanvas.SetActive(true);
        proceedButton.onClick.AddListener(OnProceedClicked);
        // Hide game & pause UI initially
        gameplayRoot.SetActive(false);
        pauseCanvas.SetActive(false);
    }

    void Update()
    {
        // Toggle pause menu with ESC
        if (gameplayRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pauseCanvas.SetActive(isPaused);
        }
    }

    // Called when "Proceed" button is clicked
    public void OnProceedClicked()
    {
        welcomeCanvas.SetActive(false);
        gameplayRoot.SetActive(true); // Start the actual game
    }

    // Called when "Exit" button is clicked
    public void OnExitClicked()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}
