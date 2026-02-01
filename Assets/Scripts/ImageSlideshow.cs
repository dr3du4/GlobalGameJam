using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ImageSlideshow : MonoBehaviour
{
    [Header("Images")]
    public GameObject[] images;
    
    [Header("Navigation")]
    public string nextSceneName = "MainMenu";
    
    private int currentIndex = 0;
    
    void Start()
    {
        // Pokaż tylko pierwszy obraz
        UpdateVisibility();
    
    }
    
    public void OnNextClicked()
    {
        currentIndex++;
        
        // Sprawdź czy to był ostatni
        if (currentIndex >= images.Length)
        {
            // Załaduj nową scenę
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            UpdateVisibility();
        }
    }
    
    void UpdateVisibility()
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
            {
                images[i].SetActive(i == currentIndex);
            }
        }
    }
}

