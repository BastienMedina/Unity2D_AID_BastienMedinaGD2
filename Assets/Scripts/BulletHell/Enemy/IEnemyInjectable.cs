public interface IEnemyInjectable
{
    void InjectDependencies(UnityEngine.Transform playerTransform, LivesManager livesManager, LootSystem lootSystem); // Injecte les refs runtime après Instantiate
}
