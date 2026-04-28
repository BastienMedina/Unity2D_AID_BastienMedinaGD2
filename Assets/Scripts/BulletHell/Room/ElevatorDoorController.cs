using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class ElevatorDoorController : MonoBehaviour
{
    [SerializeField] private float _waitDuration = 60f;
    [SerializeField] private float _doorPanelThickness = 0.15f;
    [SerializeField] private Color _doorColor = new Color(0.25f, 0.25f, 0.35f, 1f);
    [SerializeField] private int _doorSortingOrder = 2;

    public UnityEvent OnDoorsOpened = new UnityEvent();
    public UnityEvent<int> OnTimerTick = new UnityEvent<int>();

    private ElevatorDoor _doorA;
    private ElevatorDoor _doorB;
    private bool _sequenceStarted;
    private bool _isTimerDone;
    private Vector2 _entranceDirection;
    private float _doorwayWidth;

    public bool IsTimerDone => _isTimerDone; // Expose l'état du timer

    private void Start() // Démarre le timer automatiquement au chargement
    {
        StartDoorSequence();
    }

    public void Configure(Vector2 entranceDirection, float doorwayWidth) // Reçoit direction et largeur du passage
    {
        _entranceDirection = entranceDirection.normalized;
        _doorwayWidth      = doorwayWidth;
        BuildDoors();
    }

    public void StartDoorSequence() // Lance le timer si pas encore démarré
    {
        if (_sequenceStarted) return;
        _sequenceStarted = true;
        StartCoroutine(DoorSequenceCoroutine());
    }

    private IEnumerator DoorSequenceCoroutine() // Attend le timer puis ouvre les portes
    {
        float remaining = _waitDuration;
        OnTimerTick?.Invoke(Mathf.CeilToInt(remaining));

        while (remaining > 0f) // Décrémente seconde par seconde
        {
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
            OnTimerTick?.Invoke(Mathf.Max(0, Mathf.CeilToInt(remaining)));
        }

        _doorA?.Open();
        _doorB?.Open();
        yield return new WaitForSeconds(0.6f); // Attend la fin de l'animation

        _isTimerDone = true;
        OnDoorsOpened?.Invoke();
    }

    private void BuildDoors() // Crée deux panneaux à l'entrée du passage
    {
        if (_doorwayWidth <= 0f)
        {
            Debug.LogWarning("[ElevatorDoorController] Configure() non appelé — portes non construites.");
            return;
        }

        Vector3 worldScale   = transform.lossyScale;
        float halfW          = worldScale.x * 0.5f;
        float halfH          = worldScale.y * 0.5f;
        Vector3 entranceCenter = transform.position + new Vector3(_entranceDirection.x * halfW, _entranceDirection.y * halfH, 0f);
        Vector2 perp         = new Vector2(-_entranceDirection.y, _entranceDirection.x);

        float halfDoor     = _doorwayWidth * 0.5f;
        float closedOffset = halfDoor * 0.5f;
        float openOffset   = halfDoor + _doorPanelThickness * 0.5f + 0.05f;

        Vector3 posA_closed = entranceCenter + (Vector3)(perp *  closedOffset);
        Vector3 posA_open   = entranceCenter + (Vector3)(perp *  openOffset);
        Vector3 posB_closed = entranceCenter + (Vector3)(perp * -closedOffset);
        Vector3 posB_open   = entranceCenter + (Vector3)(perp * -openOffset);

        bool isHorizontalEntrance = Mathf.Abs(_entranceDirection.x) > 0.5f;
        Vector2 panelSize = isHorizontalEntrance // Horizontal ou vertical selon la direction
            ? new Vector2(_doorPanelThickness, halfDoor)
            : new Vector2(halfDoor, _doorPanelThickness);

        _doorA = CreateDoorPanel("ElevatorDoor_A", posA_closed, posA_open, panelSize);
        _doorB = CreateDoorPanel("ElevatorDoor_B", posB_closed, posB_open, panelSize);
    }

    private ElevatorDoor CreateDoorPanel(string goName, Vector3 worldClosed, Vector3 worldOpen, Vector2 size) // Instancie un panneau avec sprite et collider
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(transform.parent, false);
        go.transform.position   = worldClosed;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Texture2D tex     = new Texture2D(1, 1) { filterMode = FilterMode.Point };
        tex.SetPixel(0, 0, _doorColor);
        tex.Apply();

        sr.sprite         = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        sr.sortingOrder   = _doorSortingOrder;

        go.AddComponent<BoxCollider2D>(); // Bloque physiquement le joueur

        ElevatorDoor door = go.AddComponent<ElevatorDoor>();
        door.InitializeWorld(worldClosed, worldOpen);
        return door;
    }
}
