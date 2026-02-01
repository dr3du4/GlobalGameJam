using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "NetowrkScene";
    
    [Header("Optional References")]
    [SerializeField] private GameObject creditsPanel;

    
    void Start()
    {
        // Upewnij się że credits panel jest ukryty na starcie
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        
        // Odblokuj kursor (na wypadek gdyby był zablokowany)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Przycisk START - przejście do scendy z grą/networkingiem
    /// </summary>
    public void OnStartClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    
    /// <summary>
    /// Przycisk CREDITS - pokaż/ukryj panel z credits
    /// </summary>
    public void OnCreditsClicked()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(!creditsPanel.activeSelf);
        }
    }
    
    /// <summary>
    /// Przycisk EXIT - wyjście z gry
    /// </summary>
    public void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Zamknij panel credits (przycisk X lub kliknięcie w tło)
    /// </summary>
    public void CloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }
}
