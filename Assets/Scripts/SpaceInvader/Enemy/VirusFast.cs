using UnityEngine;

// Gère uniquement la descente verticale du virus rapide
public class VirusFast : VirusBase
{
    // Indique si le virus est mort pour bloquer toute action résiduelle
    private bool _isDead;

    // Détruit le GameObject à la mort du virus rapide
    protected override void HandleDeath()
    {
        // Empêche un double appel si la mort est déjà traitée
        if (_isDead) return;

        // Marque le virus comme mort pour bloquer tout traitement résiduel
        _isDead = true;

        // Supprime ce virus de la scène après sa mort
        Destroy(gameObject);
    }

    /// <summary>Injecte les références scène (compatibilité avec SI_VirusInjector).</summary>
    // Conservé pour compatibilité avec l'injecteur existant
    public void Inject(WaveManager waveManager, SI_ServerHealth serverHealth)
    {
        // Aucune dépendance externe dans la nouvelle version
    }
}
