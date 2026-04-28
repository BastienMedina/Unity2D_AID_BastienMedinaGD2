public interface IEnemyDamageable
{
    void TakeDamage(int amount); // Applique les dégâts à l'ennemi
    bool IsDead(); // Retourne vrai si la santé est épuisée
}
