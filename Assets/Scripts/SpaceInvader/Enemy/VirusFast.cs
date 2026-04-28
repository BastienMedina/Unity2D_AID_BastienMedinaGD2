using UnityEngine;

public class VirusFast : VirusBase
{
    private bool _isDead;

    protected override void HandleDeath() // Détruit le virus rapide à la mort
    {
        if (_isDead) return;
        _isDead = true;
        Destroy(gameObject);
    }

    public void Inject(WaveManager waveManager, SI_ServerHealth serverHealth) { } // Stub de compatibilité avec l'injecteur
}
