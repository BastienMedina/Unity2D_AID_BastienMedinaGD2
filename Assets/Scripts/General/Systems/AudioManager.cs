using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Singleton persistant gérant la lecture des effets sonores et de la musique de fond.
[DefaultExecutionOrder(-50)]
public class AudioManager : MonoBehaviour
{
    // Instance globale accessible depuis n'importe quelle scène
    public static AudioManager Instance { get; private set; }

    // AudioMixer principal du projet — doit correspondre au même asset que OptionsController
    [SerializeField] private AudioMixer _audioMixer;

    // Nom du groupe SFX dans l'AudioMixer
    [SerializeField] private string _sfxGroupName = "SFX";

    // Nom du groupe Musique dans l'AudioMixer
    [SerializeField] private string _musicGroupName = "Music";

    // Volume global appliqué à tous les SFX (0 = muet, 1 = plein volume)
    [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.25f;

    // Source audio dédiée aux effets one-shot
    private AudioSource _sfxSource;

    // Source audio dédiée à la musique de fond en boucle
    private AudioSource _musicSource;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSources();

        // Arrête automatiquement la musique à chaque changement de scène.
        // AudioMusicPlayer de la nouvelle scène la redémarrera avec le bon clip.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Coupe la musique dès qu'une nouvelle scène est chargée.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ignore les chargements additifs (overlays, etc.)
        if (mode == LoadSceneMode.Additive)
            return;

        StopMusic();
    }

    // -------------------------------------------------------------------------
    // API publique — SFX
    // -------------------------------------------------------------------------

    /// <summary>Joue un son one-shot sur le groupe SFX.
    /// Le volume est multiplié par <see cref="_sfxVolume"/> pour un contrôle global centralisé.</summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _sfxSource == null)
            return;

        _sfxSource.PlayOneShot(clip, volume * _sfxVolume);
    }

    // -------------------------------------------------------------------------
    // API publique — Musique
    // -------------------------------------------------------------------------

    /// <summary>Démarre la lecture d'une musique de fond en boucle.
    /// Ne redémarre pas si la même piste est déjà en cours.</summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || _musicSource == null)
            return;

        // Évite le redémarrage si la piste est déjà active
        if (_musicSource.clip == clip && _musicSource.isPlaying)
            return;

        _musicSource.clip = clip;
        _musicSource.Play();
    }

    /// <summary>Arrête la musique de fond en cours.</summary>
    public void StopMusic()
    {
        if (_musicSource == null)
            return;

        _musicSource.Stop();
        _musicSource.clip = null;
    }

    // -------------------------------------------------------------------------
    // Construction des AudioSources
    // -------------------------------------------------------------------------

    // Crée et configure les deux AudioSources avec leur groupe respectif
    private void BuildSources()
    {
        // Récupère le groupe SFX depuis le mixer si disponible
        AudioMixerGroup[] sfxGroups  = _audioMixer != null ? _audioMixer.FindMatchingGroups(_sfxGroupName)   : null;
        AudioMixerGroup[] musicGroups = _audioMixer != null ? _audioMixer.FindMatchingGroups(_musicGroupName) : null;

        // Crée la source SFX sur un enfant dédié
        GameObject sfxGO = new GameObject("AudioSource_SFX");
        sfxGO.transform.SetParent(transform);
        _sfxSource = sfxGO.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop        = false;

        if (sfxGroups != null && sfxGroups.Length > 0)
            _sfxSource.outputAudioMixerGroup = sfxGroups[0];

        // Crée la source Musique sur un enfant dédié
        GameObject musicGO = new GameObject("AudioSource_Music");
        musicGO.transform.SetParent(transform);
        _musicSource = musicGO.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop        = true;

        if (musicGroups != null && musicGroups.Length > 0)
            _musicSource.outputAudioMixerGroup = musicGroups[0];
    }
}
