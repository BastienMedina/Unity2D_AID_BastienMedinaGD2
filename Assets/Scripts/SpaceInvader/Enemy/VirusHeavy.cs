using UnityEngine;

public class VirusHeavy : VirusBase
{
    [SerializeField] private int _heavyMaxHealth = 3;
    [SerializeField] private GameObject _miniVirusPrefab;
    [SerializeField] private int _splitCount = 2;
    [SerializeField] private float _splitSpreadX = 0.4f;
    [SerializeField] private float _spawnBoundaryLeft = -4.5f;
    [SerializeField] private float _spawnBoundaryRight = 4.5f;
    [SerializeField] private AudioClip _splitClip;

    private bool _isDead;

    protected override int GetInitialHealth() => _heavyMaxHealth; // Retourne la santé propre au lourd

    protected override void HandleDeath() // Divise le virus en minis puis se détruit
    {
        if (_isDead) return;
        _isDead = true;

        if (_miniVirusPrefab == null || _splitCount <= 0) { Destroy(gameObject); return; }

        AudioManager.Instance?.PlaySFX(_splitClip);

        for (int i = 0; i < _splitCount; i++) // Instancie chaque mini en éventail
        {
            float offsetX   = (i - (_splitCount - 1) * 0.5f) * _splitSpreadX;
            float clampedX  = Mathf.Clamp(transform.position.x + offsetX, _spawnBoundaryLeft, _spawnBoundaryRight);
            Instantiate(_miniVirusPrefab, new Vector2(clampedX, transform.position.y), Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void Inject(WaveManager waveManager) { } // Stub de compatibilité avec l'injecteur
}
