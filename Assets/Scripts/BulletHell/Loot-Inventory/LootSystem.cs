using UnityEngine;

public class LootSystem : MonoBehaviour
{
    [SerializeField] private GameObject[] _lootPrefabs;
    [SerializeField] private float _dropChance = 1f;

    public void SpawnLoot(Vector3 position) // Instancie un préfab de butin aléatoire
    {
        if (_lootPrefabs == null || _lootPrefabs.Length == 0)
            return;

        if (Random.value > _dropChance) // Abandonne si tirage raté
            return;

        Instantiate(_lootPrefabs[Random.Range(0, _lootPrefabs.Length)], position, Quaternion.identity);
    }
}
