using UnityEngine;

public class LabelBuilder : MonoBehaviour
{
    [SerializeField] private int fontSize = 14;
    [SerializeField] private float characterSize = 0.1f;
    [SerializeField] private float verticalOffset = 0.6f;

    private void Awake() // Crée tous les labels de la map au démarrage
    {
        CreateLabel("PLAYER",        new Vector3(-5f,  0f, 0f), Color.white);
        CreateLabel("CHARGER",       new Vector3(-3f,  3f, 0f), new Color(1f, 0.3f, 0.3f, 1f));
        CreateLabel("SHOOTER",       new Vector3( 3f, -3f, 0f), new Color(0.6f, 0.2f, 1f, 1f));
        CreateLabel("HIDDEN\n(desk)",new Vector3(13f,  6f, 0f), new Color(1f, 0.5f, 0f, 1f));
        CreateLabel("NETWORK\nSPAWNER", new Vector3(13f, -5f, 0f), new Color(0f, 1f, 1f, 1f));
        CreateLabel("OPEN SPACE",    new Vector3(-7f,  6.5f, 0f), new Color(0.8f, 0.8f, 0.8f, 1f));
        CreateLabel("OFFICE",        new Vector3(14f,  9.5f, 0f), new Color(0.8f, 0.8f, 0.8f, 1f));
        CreateLabel("BREAK ROOM",    new Vector3(14f, -1.5f, 0f), new Color(0.8f, 0.8f, 0.8f, 1f));
        CreateLabel("DOOR \u2192",   new Vector3( 9.5f,  2.0f, 0f), Color.yellow);
        CreateLabel("DOOR \u2192",   new Vector3( 9.5f, -5.0f, 0f), Color.yellow);

        Color deskColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        CreateLabel("DESK", new Vector3(-6f,  4f, 0f), deskColor);
        CreateLabel("DESK", new Vector3(-6f, -4f, 0f), deskColor);
        CreateLabel("DESK", new Vector3(-2f,  5f, 0f), deskColor);
        CreateLabel("DESK", new Vector3(-2f, -5f, 0f), deskColor);
        CreateLabel("DESK", new Vector3( 2f,  2f, 0f), deskColor);
        CreateLabel("DESK", new Vector3( 2f, -3f, 0f), deskColor);
        CreateLabel("DESK", new Vector3(13f,  7f, 0f), deskColor);
        CreateLabel("DESK", new Vector3(15f,  5f, 0f), deskColor);
        CreateLabel("LOCKER", new Vector3(16f, -5f, 0f), new Color(0.4f, 0.4f, 0.5f, 1f));
    }

    private void CreateLabel(string text, Vector3 worldPos, Color color) // Instancie un TextMesh au-dessus de la position
    {
        GameObject go = new GameObject($"Label_{text}");
        go.transform.SetParent(transform, true);
        go.transform.position = worldPos + Vector3.up * verticalOffset;

        TextMesh tm       = go.AddComponent<TextMesh>();
        tm.text           = text;
        tm.fontSize       = fontSize;
        tm.characterSize  = characterSize;
        tm.anchor         = TextAnchor.MiddleCenter;
        tm.alignment      = TextAlignment.Center;
        tm.color          = color;
    }
}
