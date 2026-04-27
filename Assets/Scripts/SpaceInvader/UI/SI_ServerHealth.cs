using UnityEngine;
using UnityEngine.Events;

// Suit l'intégrité du serveur et déclenche le game over
public class SI_ServerHealth : MonoBehaviour
{
    // Valeur maximale de l'intégrité du serveur au démarrage
    [SerializeField] private int _maxIntegrity = 5;

    // Événement déclenché après chaque dégât avec l'intégrité restante
    [SerializeField] private UnityEvent<int> _onServerDamaged;

    // Événement déclenché une fois quand l'intégrité atteint zéro
    [SerializeField] private UnityEvent _onServerDestroyed;

    // Son joué à chaque fois que le serveur subit des dégâts
    [SerializeField] private AudioClip _damagedClip;

    // Son joué quand le serveur est complètement détruit
    [SerializeField] private AudioClip _destroyedClip;

    // Valeur courante de l'intégrité décrémentée à chaque impact
    private int _currentIntegrity;

    // Initialise l'intégrité courante à la valeur maximale configurée
    private void Awake()
    {
        // Clamp _maxIntegrity à 1 minimum si une valeur invalide est saisie
        if (_maxIntegrity <= 0)
        {
            // Avertit que la valeur configurée est invalide et corrige à 1
            Debug.LogWarning("[SERVER] _maxIntegrity invalide, corrigé à 1");

            // Force la valeur minimale acceptable pour l'intégrité du serveur
            _maxIntegrity = 1;
        }

        // Copie la valeur maximale dans le compteur d'intégrité courant
        _currentIntegrity = _maxIntegrity;
    }

    /// <summary>Réduit l'intégrité du serveur et déclenche les événements.</summary>
    // Décrémente l'intégrité et notifie les abonnés selon l'état
    public void TakeDamage(int amount)
    {
        // Ignore les dégâts si le serveur est déjà détruit
        if (_currentIntegrity <= 0)
        {
            return;
        }

        // Réduit l'intégrité courante du montant de dégâts reçu
        _currentIntegrity -= amount;

        // Clamp l'intégrité à zéro pour éviter les valeurs négatives
        _currentIntegrity = Mathf.Max(_currentIntegrity, 0);

        // Notifie les abonnés avec la valeur d'intégrité restante
        _onServerDamaged?.Invoke(_currentIntegrity);

        // Déclenche le game over si l'intégrité est épuisée
        if (_currentIntegrity <= 0)
        {
            AudioManager.Instance?.PlaySFX(_destroyedClip);
            // Notifie les abonnés que le serveur vient d'être détruit
            _onServerDestroyed?.Invoke();
        }
        else
        {
            AudioManager.Instance?.PlaySFX(_damagedClip);
        }
    }

    /// <summary>Retourne la valeur d'intégrité actuelle du serveur.</summary>
    // Expose l'intégrité courante en lecture seule aux autres systèmes
    public int GetCurrentIntegrity()
    {
        // Renvoie la valeur courante sans la modifier
        return _currentIntegrity;
    }
}
