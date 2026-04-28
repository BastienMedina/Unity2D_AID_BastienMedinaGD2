using UnityEngine;
using UnityEngine.Events;

public class SI_ServerHealth : MonoBehaviour
{
    [SerializeField] private int _maxIntegrity = 3;
    [SerializeField] public UnityEvent<int> _onServerDamaged;
    [SerializeField] private UnityEvent _onServerDestroyed;
    [SerializeField] private AudioClip _damagedClip;
    [SerializeField] private AudioClip _destroyedClip;

    private int _currentIntegrity;

    private void Awake() // Initialise l'intégrité à la valeur maximale configurée
    {
        if (_maxIntegrity <= 0) { Debug.LogWarning("[SERVER] _maxIntegrity invalide, corrigé à 1"); _maxIntegrity = 1; }
        _currentIntegrity = _maxIntegrity;
    }

    public void TakeDamage(int amount) // Décrémente l'intégrité et notifie les abonnés
    {
        if (_currentIntegrity <= 0) return;

        _currentIntegrity = Mathf.Max(_currentIntegrity - amount, 0);
        _onServerDamaged?.Invoke(_currentIntegrity);

        if (_currentIntegrity <= 0) // Déclenche la destruction si intégrité épuisée
        {
            AudioManager.Instance?.PlaySFX(_destroyedClip);
            _onServerDestroyed?.Invoke();
        }
        else AudioManager.Instance?.PlaySFX(_damagedClip);
    }

    public int GetCurrentIntegrity() => _currentIntegrity; // Expose l'intégrité courante en lecture
    public int GetMaxIntegrity()     => _maxIntegrity;     // Expose le maximum pour construire l'UI
}
