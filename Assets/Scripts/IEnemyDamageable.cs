// Contrat obligatoire pour tout ennemi pouvant subir des dégâts
public interface IEnemyDamageable
{
    // Applique un montant de dégâts à l'ennemi concerné
    void TakeDamage(int amount);

    // Retourne vrai si la santé de l'ennemi est épuisée
    bool IsDead();
}
