using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private string _masterParam     = "MasterVolume";
    [SerializeField] private string _musicParam      = "MusicVolume";
    [SerializeField] private float _defaultVolume    = 0.75f;
    [SerializeField] private float _silenceThreshold = 0.001f;
    [SerializeField] private float _silenceDb        = -80f;

    private void Start() // Charge les préférences et applique les volumes
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", _defaultVolume);
        float music  = PlayerPrefs.GetFloat("MusicVolume",  _defaultVolume);
        _masterSlider.value = master;
        _musicSlider.value  = music;
        ApplyMasterVolume(master);
        ApplyMusicVolume(music);
    }

    public void OnMasterVolumeChanged(float value) => ApplyMasterVolume(value); // Applique le volume master en temps réel

    public void OnMusicVolumeChanged(float value)  => ApplyMusicVolume(value);  // Applique le volume musique en temps réel

    public void OnApplyAndExit() // Persiste les valeurs et retourne au menu
    {
        PlayerPrefs.SetFloat("MasterVolume", _masterSlider.value);
        PlayerPrefs.SetFloat("MusicVolume",  _musicSlider.value);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
        _menuPanel.SetActive(true);
    }

    private void ApplyMasterVolume(float value) // Convertit 0-1 en dB et envoie au mixer
    {
        float db = value > _silenceThreshold ? Mathf.Log10(value) * 20f : _silenceDb;
        _audioMixer.SetFloat(_masterParam, db);
    }

    private void ApplyMusicVolume(float value) // Convertit 0-1 en dB et envoie au mixer
    {
        float db = value > _silenceThreshold ? Mathf.Log10(value) * 20f : _silenceDb;
        _audioMixer.SetFloat(_musicParam, db);
    }
}
