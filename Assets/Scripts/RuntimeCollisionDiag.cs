using UnityEngine;

// Diagnostique les collisions au démarrage de la scène
public class RuntimeCollisionDiag : MonoBehaviour
{
    // Inspecte tous les Rigidbody2D et Collider2D présents dans la scène
    private void Start()
    {
        // Trouve tous les Rigidbody2D dans la scène
        Rigidbody2D[] allRBs = FindObjectsOfType<Rigidbody2D>();

        // Affiche chaque Rigidbody2D trouvé dans la scène
        foreach (var rb in allRBs)
            Debug.Log($"[COL] RB2D found: {rb.gameObject.name} | layer={LayerMask.LayerToName(rb.gameObject.layer)} | bodyType={rb.bodyType} | isKinematic={rb.isKinematic}");

        // Trouve tous les Collider2D dans la scène
        Collider2D[] allCols = FindObjectsOfType<Collider2D>();

        // Affiche chaque collider trouvé avec ses propriétés
        foreach (var col in allCols)
            Debug.Log($"[COL] COL2D found: {col.gameObject.name} | layer={LayerMask.LayerToName(col.gameObject.layer)} | isTrigger={col.isTrigger} | enabled={col.enabled}");

        // Vérifie la matrice de collision entre les layers Default
        Debug.Log($"[COL] Layer Default vs Default collision = {!Physics2D.GetIgnoreLayerCollision(0, 0)}");
    }
}
