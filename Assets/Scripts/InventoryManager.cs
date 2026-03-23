using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Gère la liste des objets collectés par le joueur en jeu
public class InventoryManager : MonoBehaviour
{
    // Événement déclenché avec l'objet ajouté à l'inventaire
    [SerializeField] private UnityEvent<GameObject> _onItemAdded;

    // Liste interne des objets actuellement dans l'inventaire
    private List<GameObject> _items = new List<GameObject>();

    // Ajoute un objet à l'inventaire et notifie les abonnés
    public void AddItem(GameObject item)
    {
        // Ignore l'ajout si l'objet transmis est nul
        if (item == null)
        {
            return;
        }

        // Ajoute l'objet à la liste interne de l'inventaire
        _items.Add(item);

        // Notifie les abonnés de l'ajout avec l'objet concerné
        _onItemAdded?.Invoke(item);
    }

    // Retourne une copie de la liste des objets en inventaire
    public List<GameObject> GetItems()
    {
        // Retourne une nouvelle liste pour protéger l'état interne
        return new List<GameObject>(_items);
    }
}
