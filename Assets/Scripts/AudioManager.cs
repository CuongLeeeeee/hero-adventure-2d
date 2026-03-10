using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Player Sounds")]
    public AudioClip hurt;
    public AudioClip attack;
    public AudioClip dead;

    [Header("Footstep Sounds")]
    public AudioClip footstepGrass;
    public AudioClip footstepWater;
    public AudioClip footstepGround; 

    [Header("Music")]
    public AudioClip backgroundMusic;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic(backgroundMusic);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // FOOTSTEP FUNCTIONS

    public void PlayGrassStep()
    {
        PlaySFX(footstepGrass);
    }

    public void PlayWaterStep()
    {
        PlaySFX(footstepWater);
    }

    public void PlayGroundStep()
    {
        PlaySFX(footstepGround);
    }
}