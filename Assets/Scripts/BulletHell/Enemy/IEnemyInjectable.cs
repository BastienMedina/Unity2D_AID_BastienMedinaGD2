// Contrat permettant à ProceduralMapGenerator d'injecter les dépendances
// runtime dans tout ennemi instancié dynamiquement
public interface IEnemyInjectable
{
    /// <summary>Injecte les références runtime requises après l'Instantiate.</summary>
    void InjectDependencies(UnityEngine.Transform playerTransform, LivesManager livesManager, LootSystem lootSystem);
}
