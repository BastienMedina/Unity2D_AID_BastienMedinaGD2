// Contrat obligatoire pour tout virus pouvant subir des dégâts
public interface IVirusDamageable
{
    // Applique un montant de dégâts au virus concerné
    void TakeDamage(int amount);

    // Retourne vrai si la santé du virus est épuisée
    bool IsDead();

    // Retourne la valeur de santé actuelle du virus
    int GetCurrentHealth();
}
