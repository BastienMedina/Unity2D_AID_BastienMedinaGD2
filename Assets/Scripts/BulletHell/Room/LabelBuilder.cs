using UnityEngine;

// Crée les labels texte pour tous les objets clés
public class LabelBuilder : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Paramètres configurables depuis l'Inspector
    // -----------------------------------------------------------------------

    // Taille de police du texte rendu en espace monde
    [SerializeField] private int fontSize = 14;

    // Taille du caractère en unités monde pour TextMesh
    [SerializeField] private float characterSize = 0.1f;

    // Décalage vertical du label au-dessus de la position cible
    [SerializeField] private float verticalOffset = 0.6f;

    // -----------------------------------------------------------------------
    // Méthode de création d'un label
    // -----------------------------------------------------------------------

    // Crée un label texte au-dessus d'un objet
    private void CreateLabel(string text, Vector3 worldPos, Color color)
    {
        // Crée le GameObject portant le label
        GameObject go = new GameObject($"Label_{text}");

        // Positionne le label légèrement au-dessus de l'objet
        go.transform.position = worldPos + Vector3.up * verticalOffset;

        // Ajoute le composant TextMesh pour l'affichage monde
        TextMesh tm = go.AddComponent<TextMesh>();

        // Configure le texte affiché par ce label
        tm.text = text;

        // Définit la taille de police du label
        tm.fontSize = fontSize;

        // Définit la taille des caractères en unités monde
        tm.characterSize = characterSize;

        // Centre le pivot horizontal et vertical du texte
        tm.anchor = TextAnchor.MiddleCenter;

        // Aligne le contenu du texte au centre
        tm.alignment = TextAlignment.Center;

        // Applique la couleur fournie en paramètre
        tm.color = color;

        // Attache le label à ce GameObject comme parent
        go.transform.SetParent(transform, true);
    }

    // -----------------------------------------------------------------------
    // Initialisation de tous les labels au démarrage
    // -----------------------------------------------------------------------

    // Initialise tous les labels au démarrage de la scène
    private void Awake()
    {
        // Label du joueur en blanc
        CreateLabel("PLAYER",
            new Vector3(-5f, 0f, 0f), Color.white);

        // Label de l'ennemi chargeur en rouge clair
        CreateLabel("CHARGER",
            new Vector3(-3f, 3f, 0f), new Color(1f, 0.3f, 0.3f, 1f));

        // Label de l'ennemi tireur en violet
        CreateLabel("SHOOTER",
            new Vector3(3f, -3f, 0f), new Color(0.6f, 0.2f, 1f, 1f));

        // Label de l'ennemi caché dans le bureau en orange
        CreateLabel("HIDDEN\n(desk)",
            new Vector3(13f, 6f, 0f), new Color(1f, 0.5f, 0f, 1f));

        // Label du spawner réseau en cyan
        CreateLabel("NETWORK\nSPAWNER",
            new Vector3(13f, -5f, 0f), new Color(0f, 1f, 1f, 1f));

        // Label de la salle principale en gris clair
        CreateLabel("OPEN SPACE",
            new Vector3(-7f, 6.5f, 0f), new Color(0.8f, 0.8f, 0.8f, 1f));

        // Label de la salle Bureau en gris clair
        CreateLabel("OFFICE",
            new Vector3(14f, 9.5f, 0f), new Color(0.8f, 0.8f, 0.8f, 1f));

        // Label de la salle Pause en gris clair
        CreateLabel("BREAK ROOM",
            new Vector3(14f, -1.5f, 0f), new Color(0.8f, 0.8f, 0.8f, 1f));

        // Label de la porte AB vers Room_Office en jaune
        CreateLabel("DOOR \u2192",
            new Vector3(9.5f, 2.0f, 0f), Color.yellow);

        // Label de la porte AC vers Room_Break en jaune
        CreateLabel("DOOR \u2192",
            new Vector3(9.5f, -5.0f, 0f), Color.yellow);

        // Labels des bureaux de la salle principale en gris moyen
        CreateLabel("DESK", new Vector3(-6f,  4f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));
        CreateLabel("DESK", new Vector3(-6f, -4f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));
        CreateLabel("DESK", new Vector3(-2f,  5f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));
        CreateLabel("DESK", new Vector3(-2f, -5f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));
        CreateLabel("DESK", new Vector3( 2f,  2f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));
        CreateLabel("DESK", new Vector3( 2f, -3f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));

        // Labels des bureaux de Room_Office en gris moyen
        CreateLabel("DESK", new Vector3(13f, 7f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));
        CreateLabel("DESK", new Vector3(15f, 5f, 0f), new Color(0.5f, 0.5f, 0.5f, 1f));

        // Label du casier de Room_Break en gris-bleu
        CreateLabel("LOCKER",
            new Vector3(16f, -5f, 0f), new Color(0.4f, 0.4f, 0.5f, 1f));
    }
}
