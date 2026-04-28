using UnityEngine;

[DefaultExecutionOrder(-10)]
public class MapVisualBuilder : MonoBehaviour
{
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    [SerializeField] private Color floorColorOpenSpace = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color floorColorOffice    = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color floorColorBreak     = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color wallColor           = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private PropLibrary _propLibrary;

    private void Awake() // Construit l'Open Space et masque les salles statiques
    {
        BuildRoomOpenSpace();
        HideStaticRooms();
    }

    private void BuildRoomOpenSpace() // Construit sol et murs de l'Open Space
    {
        Transform roomRoot = FindChild(transform, "Room_OpenSpace");
        if (roomRoot == null) { Debug.LogError("[MapVisualBuilder] Room_OpenSpace introuvable."); return; }

        ApplyFloor(roomRoot, "Floor_OpenSpace", Color.black, new Vector3(20f, 14f, 1f), new Vector3(0f, 0f, 1f), -2);
        ApplyWall(roomRoot, "Wall_Top",    new Vector3(20.3f, 0.3f, 1f), new Vector3(0f,     7.15f, 0f), wallColor);
        ApplyWall(roomRoot, "Wall_Bottom", new Vector3(20.3f, 0.3f, 1f), new Vector3(0f,    -7.15f, 0f), wallColor);
        ApplyWall(roomRoot, "Wall_Left",   new Vector3(0.3f, 14.3f, 1f), new Vector3(-10.15f, 0f,   0f), wallColor);
        ApplyWall(roomRoot, "Wall_Right",  new Vector3(0.3f, 14.3f, 1f), new Vector3(10.15f,  0f,   0f), wallColor);

        string[] obsoleteWalls = { "Wall_Right_A", "Wall_Right_B", "Wall_Right_C", "Wall_Right_Top", "Wall_Right_Mid", "Wall_Right_Bottom" };
        foreach (string name in obsoleteWalls) // Désactive les anciens segments de mur
            DisableObsoleteChild(roomRoot, name);

        for (int i = 1; i <= 6; i++) // Désactive les bureaux statiques remplacés
            DisableObsoleteChild(roomRoot, "Desk_0" + i);
    }

    private void HideStaticRooms() // Désactive les salles gérées procéduralement
    {
        HideChildIfExists("Room_Office");
        HideChildIfExists("Room_Break");
    }

    private void HideChildIfExists(string goName) // Désactive un enfant direct par nom
    {
        Transform t = FindChild(transform, goName);
        if (t != null) t.gameObject.SetActive(false);
    }

    private void ApplyFloor(Transform roomRoot, string goName, Color color, Vector3 scale, Vector3 worldPos, int sortingOrder) // Crée ou configure le sol de la salle
    {
        Transform t    = GetOrCreateChild(roomRoot, goName);
        t.position     = worldPos;
        t.localScale   = scale;
        SpriteRenderer sr = GetOrAddSpriteRenderer(t.gameObject);
        sr.sprite      = CreateColorSprite(color);
        SetURPMaterial(sr);
        sr.sortingOrder = sortingOrder;
    }

    private void ApplyWall(Transform roomRoot, string goName, Vector3 scale, Vector3 worldPos, Color color) // Crée ou configure un mur avec collider
    {
        Transform t    = GetOrCreateChild(roomRoot, goName);
        t.position     = worldPos;
        t.localScale   = scale;
        SpriteRenderer sr = GetOrAddSpriteRenderer(t.gameObject);
        sr.sprite      = CreateColorSprite(color);
        SetURPMaterial(sr);
        sr.sortingOrder = 0;

        if (t.GetComponent<BoxCollider2D>() == null) // Ajoute collider si absent
            t.gameObject.AddComponent<BoxCollider2D>();
    }

    private void DisableObsoleteChild(Transform parent, string goName) // Désactive un enfant obsolète si trouvé
    {
        Transform t = FindChild(parent, goName);
        if (t != null) t.gameObject.SetActive(false);
    }

    private Transform GetOrCreateChild(Transform parent, string goName) // Cherche ou crée un enfant par nom
    {
        Transform existing = FindChild(parent, goName);
        if (existing != null) return existing;

        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private Transform FindChild(Transform parent, string name) // Parcourt les enfants directs par nom
    {
        for (int i = 0; i < parent.childCount; i++)
            if (parent.GetChild(i).name == name) return parent.GetChild(i);
        return null;
    }

    private SpriteRenderer GetOrAddSpriteRenderer(GameObject go) // Récupère ou crée un SpriteRenderer
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        return sr != null ? sr : go.AddComponent<SpriteRenderer>();
    }

    private Sprite CreateColorSprite(Color color) // Génère un sprite de couleur unie 1px
    {
        Texture2D tex   = new Texture2D(1, 1) { filterMode = FilterMode.Point };
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void SetURPMaterial(SpriteRenderer sr) // Assigne le shader URP non-éclairé
    {
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
    }
}
