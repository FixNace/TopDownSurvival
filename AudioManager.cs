using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Настройки Громкости")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Источники")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Клипы")]
    public AudioClip menuMusic;
    public AudioClip battleMusic;
    public AudioClip shopMusic;
    public AudioClip bossMusic; // добавить!!!!!!!!!
    public AudioClip shootPistol;
    public AudioClip shootShotgun;
    public AudioClip hitEnemy;
    public AudioClip enemyDeath; // <-- НОВЫЙ ЗВУК СМЕРТИ
    public AudioClip hitPlayer;
    public AudioClip abilityUse;
    public AudioClip reload;
    public AudioClip buyItem;
    public AudioClip clickUI;

    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();
    private float globalSFXCooldown = 0.05f;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start() { ApplyVolume(); }

    public void SetVolumes(float music, float sfx)
    {
        musicVolume = music;
        sfxVolume = sfx;
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    // Обычный метод (с задержкой, чтобы не было шума)
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (soundCooldowns.ContainsKey(clip.name))
        {
            if (Time.unscaledTime - soundCooldowns[clip.name] < globalSFXCooldown) return;
        }
        soundCooldowns[clip.name] = Time.unscaledTime;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Метод для ВАЖНЫХ звуков (Смерть, Ульта) - играет мгновенно
    public void PlayPrioritySFX(AudioClip clip)
    {
        if (clip == null) return;
        // Игнорируем кулдаун
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayCombatSFX(AudioClip clip)
    {
        if (Time.timeScale == 0) return;
        PlaySFX(clip);
    }
}