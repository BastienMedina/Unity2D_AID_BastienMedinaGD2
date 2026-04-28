using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-50)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _sfxGroupName   = "SFX";
    [SerializeField] private string _musicGroupName = "Music";
    [SerializeField][Range(0f, 1f)] private float _sfxVolume = 0.25f;

    private float _sceneVolumeMultiplier = 1f;
    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    private void Awake() // Initialise le singleton persistant et les sources audio
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildSources();
        SceneManager.sceneLoaded += OnSceneLoaded; // Coupe la musique à chaque changement de scène
    }

    private void OnDestroy() // Désabonne l'événement de chargement de scène
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Réinitialise le volume et stoppe la musique
    {
        if (mode == LoadSceneMode.Additive) return; // Ignore les chargements additifs

        _sceneVolumeMultiplier = 1f;
        StopMusic();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f) // Joue un SFX one-shot avec le volume configuré
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, volume * _sfxVolume * _sceneVolumeMultiplier);
    }

    public void SetSceneVolumeMultiplier(float multiplier) // Définit le multiplicateur de volume de la scène
    {
        _sceneVolumeMultiplier = Mathf.Clamp01(multiplier);
    }

    public void PlayMusic(AudioClip clip) // Démarre la musique de fond si pas déjà en cours
    {
        if (clip == null || _musicSource == null) return;
        if (_musicSource.clip == clip && _musicSource.isPlaying) return; // Évite le redémarrage

        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void StopMusic() // Stoppe et vide la musique de fond
    {
        if (_musicSource == null) return;
        _musicSource.Stop();
        _musicSource.clip = null;
    }

    private void BuildSources() // Crée les deux AudioSources avec leur groupe mixer
    {
        AudioMixerGroup[] sfxGroups   = _audioMixer != null ? _audioMixer.FindMatchingGroups(_sfxGroupName)   : null;
        AudioMixerGroup[] musicGroups = _audioMixer != null ? _audioMixer.FindMatchingGroups(_musicGroupName) : null;

        GameObject sfxGO   = new GameObject("AudioSource_SFX");
        sfxGO.transform.SetParent(transform);
        _sfxSource             = sfxGO.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop        = false;
        if (sfxGroups != null && sfxGroups.Length > 0)
            _sfxSource.outputAudioMixerGroup = sfxGroups[0];

        GameObject musicGO   = new GameObject("AudioSource_Music");
        musicGO.transform.SetParent(transform);
        _musicSource             = musicGO.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop        = true;
        if (musicGroups != null && musicGroups.Length > 0)
            _musicSource.outputAudioMixerGroup = musicGroups[0];
    }
}
