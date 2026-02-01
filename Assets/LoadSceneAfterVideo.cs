using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class LoadSceneAfterVideo : MonoBehaviour
{

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName; // wpisz nazwę sceny (dokładnie)

    private void Awake()
    {
        if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Start()
    {
        // Dla URL / czasem też dla VideoClip warto prepare, żeby nie było czarnej klatki na start.
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += _ => videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // zabezpieczenie
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
