using UnityEngine;
using UnityEngine.UI;

public class TmpMusicTester : MonoBehaviour
{
    public Button menuMusicButton;
    public Button gameplayMusicButton;
    public Button looseMusicButton;
    public Button winMusicButton;

    void Start()
    {
        menuMusicButton.onClick.AddListener(() =>
        {
            MusicManager.I?.PlayMenuMusic();
        });

        gameplayMusicButton.onClick.AddListener(() =>
        {
            MusicManager.I?.PlayGameplayMusic();
        });

        looseMusicButton.onClick.AddListener(() =>
        {
            MusicManager.I?.PlaySFX(MusicManager.I.looseMusicSource);
        });

        winMusicButton.onClick.AddListener(() =>
        {
            MusicManager.I?.PlaySFX(MusicManager.I.winMusicSource);
        });
    }
}