using UnityEngine;

// Applique le visuel de la balle joueur : utilise le sprite assigné sur le SpriteRenderer
// sans générer de texture procédurale, et ajuste l'ordre de tri uniquement.
public class SI_BulletVisual : MonoBehaviour
{
    // Ordre de tri dans le layer de rendu par défaut
    [SerializeField] private int _sortingOrder = 3;

    // Initialise le visuel de la balle joueur
    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Applique uniquement l'ordre de tri — le sprite reste celui assigné dans l'Inspector
        sr.sortingOrder = _sortingOrder;
    }
}
