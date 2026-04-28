using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Gère les portes d'ascenseur positionnées à l'entrée de la salle.
// Au démarrage de la scène, lance un timer configurable.
// À la fin du timer, ouvre les portes et déverrouille l'ascenseur.
// La transition vers l'étage suivant est déclenchée par le joueur, pas automatiquement.
[RequireComponent(typeof(SpriteRenderer))]
public class ElevatorDoorController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Durée du timer avant ouverture des portes (secondes)
    [SerializeField] private float _waitDuration = 60f;

    // Épaisseur de chaque panneau de porte en unités monde
    [SerializeField] private float _doorPanelThickness = 0.15f;

    // Couleur des panneaux de porte
    [SerializeField] private Color _doorColor = new Color(0.25f, 0.25f, 0.35f, 1f);

    // Ordre de tri des panneaux de porte
    [SerializeField] private int _doorSortingOrder = 2;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché quand les portes s'ouvrent (fin du timer) — ascenseur déverrouillé
    public UnityEvent OnDoorsOpened = new UnityEvent();

    // Déclenché chaque seconde pendant le timer avec le temps restant (entier)
    public UnityEvent<int> OnTimerTick = new UnityEvent<int>();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Référence au panneau A (gauche ou haut)
    private ElevatorDoor _doorA;

    // Référence au panneau B (droite ou bas)
    private ElevatorDoor _doorB;

    // Indique si la séquence a déjà été lancée (anti double-trigger)
    private bool _sequenceStarted;

    // Indique si le timer est terminé et les portes ouvertes
    private bool _isTimerDone;

    // Direction normalisée pointant vers l'extérieur de la salle (côté passage)
    private Vector2 _entranceDirection;

    // Largeur du passage en unités monde
    private float _doorwayWidth;

    // -------------------------------------------------------------------------
    // Propriétés publiques
    // -------------------------------------------------------------------------

    /// <summary>Vrai quand le timer est écoulé et les portes sont ouvertes.</summary>
    public bool IsTimerDone => _isTimerDone;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Démarre automatiquement le timer dès le chargement de la scène
    private void Start()
    {
        StartDoorSequence();
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Configure la direction et la largeur du passage avant construction des portes.
    /// Doit être appelé par ProceduralMapGenerator juste après AddComponent.</summary>
    public void Configure(Vector2 entranceDirection, float doorwayWidth)
    {
        _entranceDirection = entranceDirection.normalized;
        _doorwayWidth      = doorwayWidth;
        BuildDoors();
    }

    /// <summary>Démarre la séquence : timer → ouverture des portes.
    /// Protégé par _sequenceStarted pour éviter tout double déclenchement.</summary>
    public void StartDoorSequence()
    {
        if (_sequenceStarted) return;
        _sequenceStarted = true;

        StartCoroutine(DoorSequenceCoroutine());
    }

    // -------------------------------------------------------------------------
    // Séquence
    // -------------------------------------------------------------------------

    // Attend le timer (avec tick par seconde), puis ouvre les portes
    private IEnumerator DoorSequenceCoroutine()
    {
        Debug.Log($"[ElevatorDoorController] Timer démarré — {_waitDuration}s avant ouverture.");

        float remaining = _waitDuration;
        OnTimerTick?.Invoke(Mathf.CeilToInt(remaining));

        while (remaining > 0f)
        {
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
            OnTimerTick?.Invoke(Mathf.Max(0, Mathf.CeilToInt(remaining)));
        }

        // Ouverture des portes — ascenseur maintenant accessible
        _doorA?.Open();
        _doorB?.Open();

        const float animDuration = 0.6f;
        yield return new WaitForSeconds(animDuration);

        _isTimerDone = true;

        Debug.Log("[ElevatorDoorController] Portes ouvertes — ascenseur déverrouillé.");

        OnDoorsOpened?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Construction des panneaux à l'entrée du passage
    // -------------------------------------------------------------------------

    // Crée deux panneaux fins à l'entrée du passage de l'ascenseur
    private void BuildDoors()
    {
        if (_doorwayWidth <= 0f)
        {
            Debug.LogWarning("[ElevatorDoorController] Configure() non appelé — portes non construites.");
            return;
        }

        // Dimensions de la salle en world space (le GO a un localScale = taille de la salle)
        Vector3 worldScale = transform.lossyScale;
        float halfW = worldScale.x * 0.5f;
        float halfH = worldScale.y * 0.5f;

        // Centre du bord d'entrée (en world space)
        Vector3 entranceCenter = transform.position
            + new Vector3(_entranceDirection.x * halfW, _entranceDirection.y * halfH, 0f);

        // Axe perpendiculaire au sens d'entrée — direction de glissement des panneaux
        Vector2 perp = new Vector2(-_entranceDirection.y, _entranceDirection.x);

        float halfDoor = _doorwayWidth * 0.5f;

        // Panneaux fermés centrés sur leur demi-passage, ouverts glissés au-delà
        float closedOffset = halfDoor * 0.5f;
        float openOffset   = halfDoor + _doorPanelThickness * 0.5f + 0.05f;

        Vector3 posA_closed = entranceCenter + (Vector3)(perp *  closedOffset);
        Vector3 posA_open   = entranceCenter + (Vector3)(perp *  openOffset);
        Vector3 posB_closed = entranceCenter + (Vector3)(perp * -closedOffset);
        Vector3 posB_open   = entranceCenter + (Vector3)(perp * -openOffset);

        // Taille des panneaux : fin dans la direction d'entrée, demi-passage dans la direction perp
        bool isHorizontalEntrance = Mathf.Abs(_entranceDirection.x) > 0.5f;
        Vector2 panelSize = isHorizontalEntrance
            ? new Vector2(_doorPanelThickness, halfDoor)
            : new Vector2(halfDoor, _doorPanelThickness);

        _doorA = CreateDoorPanel("ElevatorDoor_A", posA_closed, posA_open, panelSize);
        _doorB = CreateDoorPanel("ElevatorDoor_B", posB_closed, posB_open, panelSize);
    }

    // Instancie un panneau de porte en world space et retourne son ElevatorDoor
    private ElevatorDoor CreateDoorPanel(string goName,
        Vector3 worldClosed, Vector3 worldOpen, Vector2 size)
    {
        GameObject go = new GameObject(goName);
        // Enfant du parent de l'ascenseur pour éviter le scale composé
        go.transform.SetParent(transform.parent, false);
        go.transform.position   = worldClosed;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, _doorColor);
        tex.Apply();

        sr.sprite         = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        sr.sortingOrder   = _doorSortingOrder;

        // BoxCollider2D solide pour bloquer le joueur tant que les portes sont fermées
        go.AddComponent<BoxCollider2D>();

        ElevatorDoor door = go.AddComponent<ElevatorDoor>();
        door.InitializeWorld(worldClosed, worldOpen);

        return door;
    }
}
