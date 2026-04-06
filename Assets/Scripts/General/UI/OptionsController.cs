using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Gère les sliders audio et la persistance des préférences
public class OptionsController : MonoBehaviour
{
    // Slider contrôlant le volume master global
    [SerializeField] private Slider _masterSlider;

    // Slider contrôlant le volume de la musique
    [SerializeField] private Slider _musicSlider;

    // AudioMixer principal du projet
    [SerializeField] private AudioMixer _audioMixer;

    // Panneau menu principal à réafficher à la sortie
    [SerializeField] private GameObject _menuPanel;

    // Nom du paramètre exposé master dans le mixer
    [SerializeField] private string _masterParam = "MasterVolume";

    // Nom du paramètre exposé musique dans le mixer
    [SerializeField] private string _musicParam = "MusicVolume";

    // Valeur de volume par défaut si aucune préférence
    [SerializeField] private float _defaultVolume = 0.75f;

    // Seuil minimal pour éviter un log10 de zéro
    [SerializeField] private float _silenceThreshold = 0.001f;

    // Valeur décibels correspondant au silence complet
    [SerializeField] private float _silenceDb = -80f;

    // Charge et applique les préférences sauvegardées
    private void Start()
    {
        // Lit les préférences ou applique la valeur par défaut
        float master = PlayerPrefs.GetFloat("MasterVolume", _defaultVolume);
        float music  = PlayerPrefs.GetFloat("MusicVolume",  _defaultVolume);

        // Synchronise les sliders avec les valeurs chargées
        _masterSlider.value = master;
        _musicSlider.value  = music;

        // Applique immédiatement les volumes au mixer
        ApplyMasterVolume(master);
        ApplyMusicVolume(music);
    }

    /// <summary>Applique le volume master en temps réel depuis le slider.</summary>
    // Reçoit la valeur du slider et met à jour le mixer
    public void OnMasterVolumeChanged(float value)
    {
        ApplyMasterVolume(value);
    }

    /// <summary>Applique le volume musique en temps réel depuis le slider.</summary>
    // Reçoit la valeur du slider et met à jour le mixer
    public void OnMusicVolumeChanged(float value)
    {
        ApplyMusicVolume(value);
    }

    /// <summary>Sauvegarde les préférences et retourne au menu.</summary>
    // Écrit dans PlayerPrefs et réaffiche le menu principal
    public void OnApplyAndExit()
    {
        PlayerPrefs.SetFloat("MasterVolume", _masterSlider.value);
        PlayerPrefs.SetFloat("MusicVolume",  _musicSlider.value);
        PlayerPrefs.Save();

        // Cache le panneau options et réaffiche le menu
        gameObject.SetActive(false);
        _menuPanel.SetActive(true);
    }

    // Convertit une valeur linéaire en dB et l'envoie au mixer
    private void ApplyMasterVolume(float value)
    {
        // Convertit la valeur 0-1 en décibels pour l'AudioMixer
        float db = value > _silenceThreshold ? Mathf.Log10(value) * 20f : _silenceDb;
        _audioMixer.SetFloat(_masterParam, db);
    }

    // Convertit une valeur linéaire en dB et l'envoie au mixer
    private void ApplyMusicVolume(float value)
    {
        // Convertit la valeur 0-1 en décibels pour l'AudioMixer
        float db = value > _silenceThreshold ? Mathf.Log10(value) * 20f : _silenceDb;
        _audioMixer.SetFloat(_musicParam, db);
    }
}
