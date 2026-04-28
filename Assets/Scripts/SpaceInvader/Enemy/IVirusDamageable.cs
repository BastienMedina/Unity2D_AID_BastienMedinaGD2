public interface IVirusDamageable
{
    void TakeDamage(int amount); // Applique les dégâts au virus
    bool IsDead();               // Retourne vrai si la santé est épuisée
    int GetCurrentHealth();      // Retourne la santé actuelle du virus
}
