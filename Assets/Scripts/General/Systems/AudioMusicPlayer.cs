using UnityEngine;

// Démarre la musique de fond de la scène courante via l'AudioManager.
// Permet aussi de surcharger le volume SFX global pour cette scène uniquement.
// Placer ce composant sur n'importe quel GameObject de la scène.
public class AudioMusicPlayer : MonoBehaviour
{
    // Clip audio joué en boucle dès le chargement de la scène
    [SerializeField] private AudioClip _musicClip;

    // Multiplicateur de volume SFX propre à cette scène (1 = volume global, 0.5 = moitié)
    [SerializeField] [Range(0f, 1f)] private float _sfxVolumeOverride = 1f;

    // Lance la musique de fond et applique le volume SFX de scène au démarrage.
    // Arrête toujours la piste précédente pour éviter les persistances inter-scènes.
    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[AudioMusicPlayer] AudioManager.Instance introuvable.", this);
            return;
        }

        // Applique le multiplicateur de volume SFX défini pour cette scène.
        AudioManager.Instance.SetSceneVolumeMultiplier(_sfxVolumeOverride);

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
