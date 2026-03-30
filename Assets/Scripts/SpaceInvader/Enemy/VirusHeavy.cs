using UnityEngine;

// Gère uniquement la descente et la division à la mort
public class VirusHeavy : VirusBase
{
    // Santé initiale propre au virus lourd
    [SerializeField] private int _heavyMaxHealth = 3;

    // Prefab des mini-virus instanciés lors de la mort
    [SerializeField] private GameObject _miniVirusPrefab;

    // Nombre de mini-virus créés lors de la division à la mort
    [SerializeField] private int _splitCount = 2;

    // Décalage horizontal entre chaque mini-virus spawné
    [SerializeField] private float _splitSpreadX = 0.4f;

    // Limite horizontale gauche pour le clamp des positions de spawn
    [SerializeField] private float _spawnBoundaryLeft = -4.5f;

    // Limite horizontale droite pour le clamp des positions de spawn
    [SerializeField] private float _spawnBoundaryRight = 4.5f;

    // Indique si le virus est mort pour bloquer toute action résiduelle
    private bool _isDead;

    // Fournit la santé initiale issue du champ propre à VirusHeavy
    protected override int GetInitialHealth()
    {
        // Retourne la valeur configurée dans l'inspecteur pour ce virus
        return _heavyMaxHealth;
    }

    // Spawne les mini-virus en éventail, puis détruit ce GameObject
    protected override void HandleDeath()
    {
        // Empêche un double appel si la mort est déjà traitée
        if (_isDead) return;

        // Marque le virus comme mort pour bloquer tout traitement résiduel
        _isDead = true;

        // Ignore le split si le prefab est absent, sans lever d'erreur
        if (_miniVirusPrefab == null || _splitCount <= 0)
        {
            // Détruit directement le virus sans tenter le split
            Destroy(gameObject);
            return;
        }

        // Boucle sur le nombre de mini-virus à instancier à la mort
        for (int i = 0; i < _splitCount; i++)
        {
            // Calcule le décalage X centré autour de la position courante
            float offsetX = (i - (_splitCount - 1) * 0.5f) * _splitSpreadX;

            // Calcule la position X brute avant le clamp aux limites d'écran
            float rawX = transform.position.x + offsetX;

            // Clamp la position X dans les limites horizontales de l'écran
            float clampedX = Mathf.Clamp(rawX, _spawnBoundaryLeft, _spawnBoundaryRight);

            // Construit la position de spawn corrigée avec le X clampé
            Vector2 spawnPosition = new Vector2(clampedX, transform.position.y);

            // Instancie le mini-virus à la position calculée sans rotation
            Instantiate(_miniVirusPrefab, spawnPosition, Quaternion.identity);
        }

        // Supprime ce virus lourd de la scène après la division
        Destroy(gameObject);
    }

    /// <summary>Injecte le WaveManager depuis l'injecteur de scène (compatibilité).</summary>
    // Conservé pour compatibilité avec SI_VirusInjector existant
    public void Inject(WaveManager waveManager)
    {
        // Aucune dépendance à WaveManager dans la nouvelle version
    }
}
