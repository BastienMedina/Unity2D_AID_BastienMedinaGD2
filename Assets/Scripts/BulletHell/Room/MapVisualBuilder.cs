using UnityEngine;

// Construit tous les visuels de la carte en Awake
[DefaultExecutionOrder(-10)]
public class MapVisualBuilder : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Constante shader URP pour tous les matériaux
    // -----------------------------------------------------------------------

    // Nom du shader URP 2D non-éclairé pour sprites
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // -----------------------------------------------------------------------
    // Paramètres configurables depuis l'Inspector
    // -----------------------------------------------------------------------

    // Couleur de sol de la salle principale (OpenSpace)
    [SerializeField] private Color floorColorOpenSpace = new Color(0.16f, 0.16f, 0.16f, 1f);

    // Couleur de sol de la salle Bureau (Office)
    [SerializeField] private Color floorColorOffice = new Color(0.2f, 0.2f, 0.2f, 1f);

    // Couleur de sol de la salle Pause (Break)
    [SerializeField] private Color floorColorBreak = new Color(0.19f, 0.19f, 0.19f, 1f);

    // Couleur blanche partagée par tous les murs
    [SerializeField] private Color wallColor = new Color(1f, 1f, 1f, 1f);

    // Couleur grise partagée par tous les bureaux
    [SerializeField] private Color deskColor = new Color(0.33f, 0.33f, 0.33f, 1f);

    // Couleur gris-bleu pour le casier de la salle Pause
    [SerializeField] private Color lockerColor = new Color(0.25f, 0.25f, 0.3f, 1f);

    // Dimension uniforme des bureaux (largeur, hauteur)
    [SerializeField] private Vector2 deskSize = new Vector2(1.5f, 0.8f);

    // -----------------------------------------------------------------------
    // Cycle de vie Unity
    // -----------------------------------------------------------------------

    // Construit uniquement l'Open Space — les salles secondaires sont générées procéduralement
    private void Awake()
    {
        BuildRoomOpenSpace();
        HideStaticRooms();
    }

    // -----------------------------------------------------------------------
    // Room A — Room_OpenSpace (centre-gauche, grande)
    // -----------------------------------------------------------------------

    // Construit sol, murs segmentés et bureaux de la salle principale
    private void BuildRoomOpenSpace()
    {
        // Récupère le parent de la salle OpenSpace dans la hiérarchie
        Transform roomRoot = FindChild(transform, "Room_OpenSpace");
        if (roomRoot == null)
        {
            Debug.LogError("[MapVisualBuilder] Room_OpenSpace introuvable sous Map.");
            return;
        }

        // Sol 20×14, centré en (0, 0) — plan z=1 sous les murs
        ApplyFloor(roomRoot, "Floor_OpenSpace", floorColorOpenSpace,
            new Vector3(20f, 14f, 1f), new Vector3(0f, 0f, 1f), -2);

        // Mur du haut — couvre toute la largeur du sol + demi-épaisseurs
        ApplyWall(roomRoot, "Wall_Top",
            new Vector3(20.3f, 0.3f, 1f), new Vector3(0f, 7.15f, 0f), wallColor);

        // Mur du bas — symétrique au mur du haut
        ApplyWall(roomRoot, "Wall_Bottom",
            new Vector3(20.3f, 0.3f, 1f), new Vector3(0f, -7.15f, 0f), wallColor);

        // Mur gauche — plein sur toute la hauteur
        ApplyWall(roomRoot, "Wall_Left",
            new Vector3(0.3f, 14.3f, 1f), new Vector3(-10.15f, 0f, 0f), wallColor);

        // Mur droit — plein ; les ouvertures procédurales seront percées par ProceduralMapGenerator
        ApplyWall(roomRoot, "Wall_Right",
            new Vector3(0.3f, 14.3f, 1f), new Vector3(10.15f, 0f, 0f), wallColor);

        // Désactive les anciens segments de mur droit devenus obsolètes
        DisableObsoleteChild(roomRoot, "Wall_Right_A");
        DisableObsoleteChild(roomRoot, "Wall_Right_B");
        DisableObsoleteChild(roomRoot, "Wall_Right_C");
        DisableObsoleteChild(roomRoot, "Wall_Right_Top");
        DisableObsoleteChild(roomRoot, "Wall_Right_Mid");
        DisableObsoleteChild(roomRoot, "Wall_Right_Bottom");

        // Échelle vectorielle des bureaux de la salle principale
        Vector3 deskScale = new Vector3(deskSize.x, deskSize.y, 1f);

        // Positions des six bureaux de la salle ouverte
        Vector3[] deskPositions = new Vector3[]
        {
            new Vector3(-6f,  4f, 0f),
            new Vector3(-6f, -4f, 0f),
            new Vector3(-2f,  5f, 0f),
            new Vector3(-2f, -5f, 0f),
            new Vector3( 2f,  2f, 0f),
            new Vector3( 2f, -3f, 0f),
        };

        // Noms des six bureaux enfants dans Room_OpenSpace
        string[] deskNames = new string[]
        {
            "Desk_01", "Desk_02", "Desk_03", "Desk_04", "Desk_05", "Desk_06"
        };

        // Applique chaque bureau à sa position cible
        for (int i = 0; i < deskNames.Length; i++)
        {
            ApplyDesk(roomRoot, deskNames[i], deskColor, deskScale, deskPositions[i]);
        }
    }

    // -----------------------------------------------------------------------
    // Room B/C — Salles statiques remplacées par la génération procédurale
    // -----------------------------------------------------------------------

    // Désactive Room_Office et Room_Break présents dans la hiérarchie de scène
    private void HideStaticRooms()
    {
        HideChildIfExists("Room_Office");
        HideChildIfExists("Room_Break");
    }

    // Désactive un enfant direct du Map par nom s'il existe
    private void HideChildIfExists(string goName)
    {
        Transform t = FindChild(transform, goName);
        if (t != null) t.gameObject.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Méthodes d'application visuelle
    // -----------------------------------------------------------------------

    // Applique le sol : position, échelle et sprite coloré
    private void ApplyFloor(Transform roomRoot, string goName, Color color,
        Vector3 scale, Vector3 worldPos, int sortingOrder)
    {
        // Trouve ou crée l'enfant sol dans la salle donnée
        Transform t = GetOrCreateChild(roomRoot, goName);

        // Positionne et redimensionne le sol selon les paramètres
        t.position = worldPos;
        t.localScale = scale;

        // Récupère ou crée le SpriteRenderer du sol
        SpriteRenderer sr = GetOrAddSpriteRenderer(t.gameObject);
        sr.sprite = CreateColorSprite(color);
        SetURPMaterial(sr);
        sr.sortingOrder = sortingOrder;
    }

    // Applique le mur : position, échelle, sprite et collider
    private void ApplyWall(Transform roomRoot, string goName, Vector3 scale,
        Vector3 worldPos, Color color)
    {
        // Trouve ou crée l'enfant mur dans la salle donnée
        Transform t = GetOrCreateChild(roomRoot, goName);

        // Positionne et redimensionne le mur selon les paramètres
        t.position = worldPos;
        t.localScale = scale;

        // Récupère ou crée le SpriteRenderer du mur
        SpriteRenderer sr = GetOrAddSpriteRenderer(t.gameObject);
        sr.sprite = CreateColorSprite(color);
        SetURPMaterial(sr);
        sr.sortingOrder = 0;

        // Ajoute un BoxCollider2D solide si absent
        if (t.GetComponent<BoxCollider2D>() == null)
            t.gameObject.AddComponent<BoxCollider2D>();
    }

    // Applique le bureau ou casier : position, échelle, sprite et collider
    private void ApplyDesk(Transform roomRoot, string goName, Color color,
        Vector3 scale, Vector3 worldPos)
    {
        // Trouve ou crée l'enfant bureau dans la salle donnée
        Transform t = GetOrCreateChild(roomRoot, goName);

        // Positionne et redimensionne le bureau selon les paramètres
        t.position = worldPos;
        t.localScale = scale;

        // Récupère ou crée le SpriteRenderer du bureau
        SpriteRenderer sr = GetOrAddSpriteRenderer(t.gameObject);
        sr.sprite = CreateColorSprite(color);
        SetURPMaterial(sr);
        sr.sortingOrder = 1;

        // Ajoute un BoxCollider2D solide si absent
        if (t.GetComponent<BoxCollider2D>() == null)
            t.gameObject.AddComponent<BoxCollider2D>();
    }

    // Crée ou repositionne un trigger de passage entre deux salles
    private void ApplyTrigger(Transform roomRoot, string goName,
        Vector3 scale, Vector3 worldPos)
    {
        // Trouve ou crée l'enfant trigger dans la salle donnée
        Transform t = GetOrCreateChild(roomRoot, goName);

        // Positionne et redimensionne le trigger selon les paramètres
        t.position = worldPos;
        t.localScale = scale;

        // Configure le BoxCollider2D en mode trigger uniquement
        BoxCollider2D col = t.GetComponent<BoxCollider2D>();
        if (col == null)
            col = t.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    // Désactive un enfant obsolète remplacé par un nouveau GO
    private void DisableObsoleteChild(Transform parent, string goName)
    {
        // Cherche l'enfant obsolète et le désactive si trouvé
        Transform t = FindChild(parent, goName);
        if (t != null)
            t.gameObject.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Helpers de recherche et création dans la hiérarchie
    // -----------------------------------------------------------------------

    // Cherche un enfant direct par nom, le crée s'il est absent
    private Transform GetOrCreateChild(Transform parent, string goName)
    {
        // Tente d'abord de trouver l'enfant existant par nom
        Transform existing = FindChild(parent, goName);
        if (existing != null) return existing;

        // Crée un nouveau GameObject enfant si introuvable
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    // Cherche un enfant direct par nom exact parmi les enfants directs
    private Transform FindChild(Transform parent, string name)
    {
        // Parcourt tous les enfants directs pour trouver le nom
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
                return parent.GetChild(i);
        }
        return null;
    }

    // -----------------------------------------------------------------------
    // Helpers sprite et matériau
    // -----------------------------------------------------------------------

    // Récupère ou ajoute un SpriteRenderer sur le GameObject cible
    private SpriteRenderer GetOrAddSpriteRenderer(GameObject go)
    {
        // Retourne l'existant ou en crée un nouveau si absent
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = go.AddComponent<SpriteRenderer>();
        return sr;
    }

    // Crée un sprite de couleur unie depuis une texture d'un pixel
    private Sprite CreateColorSprite(Color color)
    {
        // Génère une texture 1×1 avec le filtre Point pour éviter le flou
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, color);
        tex.Apply();

        // Retourne le sprite créé à partir de la texture générée
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    // Assigne le matériau shader URP au SpriteRenderer donné
    private void SetURPMaterial(SpriteRenderer sr)
    {
        // Crée un nouveau matériau depuis le shader URP 2D non-éclairé
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
    }
}
