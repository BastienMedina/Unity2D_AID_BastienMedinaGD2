using UnityEngine;
using UnityEngine.Events;

// Instancie un objet de butin à une position via une LootTable
public class LootDropper : MonoBehaviour
{
    // Table de butin utilisée pour tirer le préfab à instancier
    [SerializeField] private LootTable _lootTable;

    // Probabilité entre 0 et 1 qu'un butin soit effectivement généré
    [SerializeField] private float _dropChance = 0.75f;

    // Événement déclenché avec le GameObject instancié après le drop
    [SerializeField] private UnityEvent<GameObject> _onLootDropped;

    // Tente de dropper un butin à la position indiquée en paramètre
    public void DropLoot(Vector2 position)
    {
        // Abandonne le drop si la table de butin n'est pas assignée
        if (_lootTable == null)
        {
            return;
        }

        // Abandonne si le tirage aléatoire dépasse la chance configurée
        if (Random.value > _dropChance)
        {
            return;
        }

        // Tire un préfab pondéré depuis la table de butin configurée
        GameObject prefab = _lootTable.Roll();

        // Abandonne si la table n'a retourné aucun préfab valide
        if (prefab == null)
        {
            return;
        }

        // Instancie le préfab tiré à la position transmise en paramètre
        GameObject lootObject = Instantiate(prefab, position, Quaternion.identity);

        // Notifie les abonnés avec le GameObject instancié comme argument
        _onLootDropped?.Invoke(lootObject);
    }
}
