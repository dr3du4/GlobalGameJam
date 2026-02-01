using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager I;
    public AudioSource GameplayMusicSource;
    public AudioSource MenuMusicSource;
    public AudioSource looseMusicSource;
    public AudioSource winMusicSource;
    public AudioSource cantGoThere;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        if(!MenuMusicSource.isPlaying)
        {
            MenuMusicSource.Play();
            GameplayMusicSource.Stop();
        }
    }

    public void PlayGameplayMusic()
    {
        if (!GameplayMusicSource.isPlaying)
        {
            MenuMusicSource.Stop();
            GameplayMusicSource.Play();
        }
    }

    public void PlaySFX(AudioSource sfxSource)
    {
        if (!sfxSource.isPlaying)
        {
            sfxSource.Play();
        }
    }
}
