using UnityEngine;

public class AudioMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _musicClip;
    [SerializeField][Range(0f, 1f)] private float _sfxVolumeOverride = 1f;

    private void Start() // Applique le volume de scène et démarre la musique
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.SetSceneVolumeMultiplier(_sfxVolumeOverride);
        AudioManager.Instance.StopMusic(); // Coupe la musique précédente avant la nouvelle

        if (_musicClip == null) return;

        AudioManager.Instance.PlayMusic(_musicClip);
    }
}
