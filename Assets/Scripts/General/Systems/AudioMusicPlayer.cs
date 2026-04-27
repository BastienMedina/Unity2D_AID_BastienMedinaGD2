using UnityEngine;

// Démarre la musique de fond de la scène courante via l'AudioManager.
// Placer ce composant sur n'importe quel GameObject de la scène.
public class AudioMusicPlayer : MonoBehaviour
{
    // Clip audio joué en boucle dès le chargement de la scène
    [SerializeField] private AudioClip _musicClip;

    // Lance la musique de fond via l'AudioManager au démarrage de la scène.
    // Arrête toujours la piste précédente pour éviter les persistances inter-scènes.
    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[AudioMusicPlayer] AudioManager.Instance introuvable.", this);
            return;
        }

        // Coupe systématiquement la musique précédente avant d'en démarrer une nouvelle,
        // même si le clip est identique (changement de scène vers la même scène).
        AudioManager.Instance.StopMusic();

        if (_musicClip == null)
        {
            Debug.LogWarning("[AudioMusicPlayer] Aucun clip assigné — musique coupée.", this);
            return;
        }

        AudioManager.Instance.PlayMusic(_musicClip);
    }
}
