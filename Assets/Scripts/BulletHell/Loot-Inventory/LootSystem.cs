using UnityEngine;

// Instancie des objets de butin à une position donnée dans le monde
public class LootSystem : MonoBehaviour
{
    // Tableau des préfabs de butin pouvant être instanciés aléatoirement
    [SerializeField] private GameObject[] _lootPrefabs;

    // Probabilité entre 0 et 1 qu'un butin soit effectivement spawné
    [SerializeField] private float _dropChance = 1f;

    // Instancie un préfab de butin aléatoire à la position indiquée
    public void SpawnLoot(Vector3 position)
    {
        // N'instancie rien si le tableau de préfabs est vide ou nul
        if (_lootPrefabs == null || _lootPrefabs.Length == 0)
        {
            return;
        }

        // Abandonne le spawn si le tirage aléatoire dépasse la chance
        if (Random.value > _dropChance)
        {
            return;
        }

        // Choisit un préfab au hasard parmi ceux configurés
        int randomIndex = Random.Range(0, _lootPrefabs.Length);

        // Instancie le préfab choisi à la position de mort de l'ennemi
        Instantiate(_lootPrefabs[randomIndex], position, Quaternion.identity);
    }
}
