using UnityEngine;

// Démarre la musique de fond de la scène courante via l'AudioManager.
// Placer ce composant sur n'importe quel GameObject de la scène.
public class AudioMusicPlayer : MonoBehaviour
{
    // Clip audio joué en boucle dès le chargement de la scène
    [SerializeField] private AudioClip _musicClip;

    // Lance la musique de fond via l'AudioManager au démarrage de la scène
    private void Start()
    {
        if (_musicClip == null)
        {
            Debug.LogWarning("[AudioMusicPlayer] Aucun clip assigné.", this);
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[AudioMusicPlayer] AudioManager.Instance introuvable.", this);
            return;
        }

        AudioManager.Instance.PlayMusic(_musicClip);
    }
}
