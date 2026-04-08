using UnityEngine;

// Génère la disposition de l'étage procéduralement au chargement
[DefaultExecutionOrder(10)]
public class ProceduralMapGenerator : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constante shader URP pour tous les matériaux créés
    // -------------------------------------------------------------------------

    // Nom du shader URP 2D non-éclairé pour sprites
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Épaisseur uniforme de tous les murs générés procéduralement
    private const float WallThickness = 0.3f;

    // -------------------------------------------------------------------------
    // Types de salles secondaires disponibles
    // -------------------------------------------------------------------------

    // Types de salles secondaires disponibles
    public enum RoomType { Toilettes, SalleReunion, Vestiaire, SalleRepos }

    // -------------------------------------------------------------------------
    // Murs disponibles pour le placement des salles
    // -------------------------------------------------------------------------

    // Énumère les quatre murs de l'Open Space pour le placement
    private enum WallSide { Top, Right, Bottom, Left }

    // -------------------------------------------------------------------------
    // Dimensions de l'Open Space
    // -------------------------------------------------------------------------

    // Largeur de la salle principale Open Space en unités
    [SerializeField] private float _openSpaceWidth = 20f;

    // Hauteur de la salle principale Open Space en unités
    [SerializeField] private float _openSpaceHeight = 14f;

    // -------------------------------------------------------------------------
    // Dimensions des petites salles secondaires
    // -------------------------------------------------------------------------

    // Largeur des petites salles secondaires en unités
    [SerializeField] private float _smallRoomWidth = 6f;

    // Hauteur des petites salles secondaires en unités
    [SerializeField] private float _smallRoomHeight = 5f;

    // -------------------------------------------------------------------------
    // Dimensions de la salle ascenseur
    // -------------------------------------------------------------------------

    // Largeur de la salle ascenseur en unités
    [SerializeField] private float _elevatorWidth = 3f;

    // Hauteur de la salle ascenseur en unités
    [SerializeField] private float _elevatorHeight = 3f;

    // -------------------------------------------------------------------------
    // Largeur du passage entre les salles
    // -------------------------------------------------------------------------

    // Largeur du passage entre l'Open Space et les salles
    [SerializeField] private float _doorwayWidth = 2f;

    // -------------------------------------------------------------------------
    // Couleurs des types de salles secondaires
    // -------------------------------------------------------------------------

    // Couleur bleu clair pour la salle Toilettes
    [SerializeField] private Color _colorToilettes = new Color(0.6f, 0.8f, 1.0f);

    // Couleur vert clair pour la salle de Réunion
    [SerializeField] private Color _colorSalleReunion = new Color(0.8f, 0.9f, 0.6f);

    // Couleur orange clair pour le Vestiaire
    [SerializeField] private Color _colorVestiaire = new Color(1.0f, 0.85f, 0.6f);

    // Couleur violet clair pour la salle de Repos
    [SerializeField] private Color _colorSalleRepos = new Color(0.9f, 0.7f, 0.9f);

    // Couleur gris foncé pour la salle ascenseur
    [SerializeField] private Color _colorElevator = new Color(0.3f, 0.3f, 0.3f);

    // Couleur blanche partagée par tous les murs générés
    [SerializeField] private Color _wallColor = Color.white;

    // -------------------------------------------------------------------------
    // Paramètres de spawn d'ennemis
    // -------------------------------------------------------------------------

    // Tableau des prefabs ennemis à instancier dans l'Open Space
    [SerializeField] private GameObject[] _enemyPrefabs;

    // Nombre de base d'ennemis au premier étage
    [SerializeField] private int _baseEnemyCount = 3;

    // Prefab d'objet fouillable à instancier dans les salles
    [SerializeField] private GameObject _searchableObjectPrefab;

    // Référence au gestionnaire d'UI de fouille pour câbler les SearchableObject
    [SerializeField] private SearchUIManager _searchUIManager;

    // Table de loot par défaut assignée à chaque SearchableObject spawné en runtime
    [SerializeField] private LootTable _defaultLootTable;

    // Référence à l'InventoryManager de la scène pour câbler les LootDropper spawned
    private InventoryManager _inventoryManager;

    // Référence au LootSystem de la scène pour l'injecter dans les ennemis spawned
    private LootSystem _lootSystem;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Référence vers le GO Map qui contient MapVisualBuilder et Room_OpenSpace
    [SerializeField] private Transform _mapRoot;

    // Génère la carte complète après que tous les Awake() de la scène sont terminés.
    private void Start()
    {
        Debug.Log("[ProceduralMapGenerator] Start() appelé.");

        // Cherche Map dans la scène si non assigné en Inspector.
        if (_mapRoot == null)
        {
            GameObject mapGO = GameObject.Find("Map");
            if (mapGO != null)
            {
                _mapRoot = mapGO.transform;
                Debug.Log("[ProceduralMapGenerator] _mapRoot trouvé : " + _mapRoot.name);
            }
            else
            {
                Debug.LogError("[ProceduralMapGenerator] GameObject 'Map' introuvable dans la scène !");
            }
        }

        // Cherche le SearchUIManager dans la scène si non assigné en Inspector.
        // Appelé dans Start (et non Awake) pour garantir que tous les Awake()
        // sont terminés et que SearchUIManager est bien initialisé.
        if (_searchUIManager == null)
            _searchUIManager = FindFirstObjectByType<SearchUIManager>();

        if (_searchUIManager == null)
            Debug.LogWarning("[ProceduralMapGenerator] SearchUIManager introuvable — les objets fouillables ne seront pas câblés.", this);

        // Cherche l'InventoryManager pour le câbler aux LootDropper spawned.
        _inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (_inventoryManager == null)
            Debug.LogWarning("[ProceduralMapGenerator] InventoryManager introuvable — le loot ne sera pas ajouté à l'inventaire.", this);

        // Cherche le LootSystem pour l'injecter dans les ennemis spawned.
        _lootSystem = FindFirstObjectByType<LootSystem>();

        if (_lootSystem == null)
            Debug.LogWarning("[ProceduralMapGenerator] LootSystem introuvable — le loot ennemi ne sera pas spawné.", this);

        GenerateFloor();
    }

    // -------------------------------------------------------------------------
    // Génération principale
    // -------------------------------------------------------------------------

    // Orchestre la génération de toutes les salles et ennemis
    private void GenerateFloor()
    {
        try
        {
            GenerateFloorInternal();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ProceduralMapGenerator] Exception dans GenerateFloor : {e}", this);
        }
    }

    private void GenerateFloorInternal()
    {
        // Tire deux types de salles distincts sans doublons
        RoomType room1Type = (RoomType)Random.Range(0, 4);
        RoomType room2Type;

        do
        {
            room2Type = (RoomType)Random.Range(0, 4);
        }
        while (room2Type == room1Type);

        // Tire les murs pour les salles et l'ascenseur sans répétition
        WallSide[] walls = PickThreeDistinctWalls();
        WallSide room1Wall   = walls[0];
        WallSide room2Wall   = walls[1];
        WallSide elevWall    = walls[2];

        // Calcule les centres selon le mur assigné pour la salle 1
        Vector2 pos1 = CalcSmallRoomPosition(room1Wall);

        // Calcule les centres selon le mur assigné pour la salle 2
        Vector2 pos2 = CalcSmallRoomPosition(room2Wall);

        // Calcule le centre selon le mur assigné pour l'ascenseur
        Vector2 posElev = CalcElevatorPosition(elevWall);

        // Construit les visuels et objets fouillables de la salle 1
        BuildRoomVisual(pos1, new Vector2(_smallRoomWidth, _smallRoomHeight),
            GetRoomColor(room1Type), room1Type.ToString(), GetSearchableCount(room1Type), room1Wall);
        PierceOpenSpaceWall(room1Wall, pos1);

        // Construit les visuels et objets fouillables de la salle 2
        BuildRoomVisual(pos2, new Vector2(_smallRoomWidth, _smallRoomHeight),
            GetRoomColor(room2Type), room2Type.ToString(), GetSearchableCount(room2Type), room2Wall);
        PierceOpenSpaceWall(room2Wall, pos2);

        // Construit la salle ascenseur avec son trigger
        BuildElevator(posElev, elevWall);
        PierceOpenSpaceWall(elevWall, posElev);

        // Instancie les ennemis dans l'Open Space selon l'étage
        SpawnEnemies();
    }

    // -------------------------------------------------------------------------
    // Sélection des murs
    // -------------------------------------------------------------------------

    // Tire trois murs distincts dans un ordre aléatoire
    private WallSide[] PickThreeDistinctWalls()
    {
        WallSide[] all = { WallSide.Top, WallSide.Right, WallSide.Bottom, WallSide.Left };

        // Mélange Fisher-Yates pour sélectionner trois murs aléatoires
        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            WallSide tmp = all[i];
            all[i] = all[j];
            all[j] = tmp;
        }

        // Retourne les trois premiers murs du tableau mélangé
        return new WallSide[] { all[0], all[1], all[2] };
    }

    // -------------------------------------------------------------------------
    // Calcul des positions selon les murs
    // -------------------------------------------------------------------------

    // Calcule la position centrale d'une petite salle selon son mur
    private Vector2 CalcSmallRoomPosition(WallSide wall)
    {
        // Demi-dimensions de l'Open Space pour les calculs de bord
        float halfW = _openSpaceWidth  * 0.5f;
        float halfH = _openSpaceHeight * 0.5f;

        // Demi-dimensions de la petite salle pour le décalage
        float halfRW = _smallRoomWidth  * 0.5f;
        float halfRH = _smallRoomHeight * 0.5f;

        return wall switch
        {
            WallSide.Top    => new Vector2(0f,  halfH + halfRH),
            WallSide.Bottom => new Vector2(0f, -halfH - halfRH),
            WallSide.Right  => new Vector2( halfW + halfRW, 0f),
            WallSide.Left   => new Vector2(-halfW - halfRW, 0f),
            _               => Vector2.zero
        };
    }

    // Calcule la position centrale de la salle ascenseur selon son mur
    private Vector2 CalcElevatorPosition(WallSide wall)
    {
        // Demi-dimensions de l'Open Space pour les calculs de bord
        float halfW = _openSpaceWidth  * 0.5f;
        float halfH = _openSpaceHeight * 0.5f;

        // Demi-dimensions de l'ascenseur pour le décalage
        float halfEW = _elevatorWidth  * 0.5f;
        float halfEH = _elevatorHeight * 0.5f;

        return wall switch
        {
            WallSide.Top    => new Vector2(0f,  halfH + halfEH),
            WallSide.Bottom => new Vector2(0f, -halfH - halfEH),
            WallSide.Right  => new Vector2( halfW + halfEW, 0f),
            WallSide.Left   => new Vector2(-halfW - halfEW, 0f),
            _               => Vector2.zero
        };
    }

    // -------------------------------------------------------------------------
    // Données par type de salle
    // -------------------------------------------------------------------------

    // Retourne la couleur associée au type de salle donné
    private Color GetRoomColor(RoomType type)
    {
        return type switch
        {
            RoomType.Toilettes    => _colorToilettes,
            RoomType.SalleReunion => _colorSalleReunion,
            RoomType.Vestiaire    => _colorVestiaire,
            RoomType.SalleRepos   => _colorSalleRepos,
            _                     => Color.grey
        };
    }

    // Retourne le nombre d'objets fouillables selon le type de salle
    private int GetSearchableCount(RoomType type)
    {
        return type switch
        {
            RoomType.Toilettes    => 1,
            RoomType.SalleReunion => 3,
            RoomType.Vestiaire    => 2,
            RoomType.SalleRepos   => 2,
            _                     => 0
        };
    }

    // -------------------------------------------------------------------------
    // Construction visuelle des salles secondaires
    // -------------------------------------------------------------------------

    // Construit le visuel, le label et les objets fouillables d'une salle
    private void BuildRoomVisual(Vector2 pos, Vector2 size, Color color,
                                  string label, int searchableCount, WallSide attachedWall)
    {
        // Détermine le parent : Map si disponible, sinon ce transform
        Transform parent = _mapRoot != null ? _mapRoot : transform;

        // Crée le fond de la salle
        GameObject room = new GameObject("Room_" + label);
        room.transform.SetParent(parent, false);

        SpriteRenderer sr = room.AddComponent<SpriteRenderer>();
        sr.sprite        = CreateColorSprite(color);
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder  = -1;

        room.transform.position   = new Vector3(pos.x, pos.y, 0f);
        room.transform.localScale = new Vector3(size.x, size.y, 1f);

        // Ajoute le label de la salle au centre
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(room.transform, false);
        labelGO.transform.localPosition = Vector3.zero;

        TextMesh tm    = labelGO.AddComponent<TextMesh>();
        tm.text        = label;
        tm.fontSize    = 8;
        tm.anchor      = TextAnchor.MiddleCenter;
        tm.color       = Color.white;

        // Corrige l'échelle du label pour annuler celle de la salle
        labelGO.transform.localScale = new Vector3(
            1f / size.x, 1f / size.y, 1f);

        // Instancie les objets fouillables en grille dans la salle
        for (int i = 0; i < searchableCount; i++)
        {
            if (_searchableObjectPrefab == null) break;

            // Répartit les objets horizontalement au centre de la salle
            float xOff    = (i - searchableCount / 2f + 0.5f) * 1.5f;
            Vector3 objPos = new Vector3(pos.x + xOff, pos.y - 0.5f, 0f);
            Instantiate(_searchableObjectPrefab, objPos, Quaternion.identity);
        }

        // Génère les props thématiques de la salle selon son type
        RoomType parsedType = GetRoomTypeFromLabel(label);
        SpawnRoomProps(pos, size, parsedType, parent);

        // Génère les quatre murs avec passage vers l'Open Space
        BuildRoomWalls(pos, size, attachedWall, parent, "Room_" + label);
    }

    // -------------------------------------------------------------------------
    // Construction de l'ascenseur
    // -------------------------------------------------------------------------

    // Construit la mini-pièce ascenseur avec son trigger et composant
    private void BuildElevator(Vector2 pos, WallSide attachedWall)
    {
        // Détermine le parent : Map si disponible, sinon ce transform
        Transform parent = _mapRoot != null ? _mapRoot : transform;

        // Crée le fond de la salle ascenseur en gris foncé
        GameObject elev = new GameObject("Room_Ascenseur");
        elev.transform.SetParent(parent, false);

        SpriteRenderer sr = elev.AddComponent<SpriteRenderer>();
        sr.sprite         = CreateColorSprite(_colorElevator);
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder   = -1;

        elev.transform.position   = new Vector3(pos.x, pos.y, 0f);
        elev.transform.localScale = new Vector3(_elevatorWidth, _elevatorHeight, 1f);

        // Ajoute le label Ascenseur centré dans la salle
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(elev.transform, false);
        labelGO.transform.localPosition = Vector3.zero;

        TextMesh tm = labelGO.AddComponent<TextMesh>();
        tm.text     = "ASCENSEUR";
        tm.fontSize = 8;
        tm.anchor   = TextAnchor.MiddleCenter;
        tm.color    = Color.white;

        // Corrige l'échelle du label pour annuler celle de la salle
        labelGO.transform.localScale = new Vector3(
            1f / _elevatorWidth, 1f / _elevatorHeight, 1f);

        // Ajoute un BoxCollider2D trigger pour détecter le joueur
        BoxCollider2D col = elev.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(
            (_elevatorWidth  - 0.4f) / _elevatorWidth,
            (_elevatorHeight - 0.4f) / _elevatorHeight);

        // Ajoute le composant ElevatorTrigger pour la transition
        elev.AddComponent<ElevatorTrigger>();

        // Génère les quatre murs de l'ascenseur avec passage vers l'Open Space
        BuildRoomWalls(pos, new Vector2(_elevatorWidth, _elevatorHeight),
            attachedWall, parent, "Room_Ascenseur");
    }

    // -------------------------------------------------------------------------
    // Spawn d'ennemis dans l'Open Space
    // -------------------------------------------------------------------------

    // Calcule le nombre d'ennemis selon l'étage actuel
    private int GetEnemyCount()
    {
        int floor = GameProgress.Instance != null ? GameProgress.Instance.CurrentFloor : 1;
        return _baseEnemyCount + (floor - 1) * 2;
    }

    // Instancie les ennemis aléatoirement dans les bornes de l'Open Space
    private void SpawnEnemies()
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[SpawnEnemies] _enemyPrefabs est null ou vide — aucun ennemi spawné.", this);
            return;
        }

        int count   = GetEnemyCount();
        float halfW = _openSpaceWidth  * 0.5f - 1f;
        float halfH = _openSpaceHeight * 0.5f - 1f;

        Debug.Log($"[SpawnEnemies] Tentative de spawn de {count} ennemis dans bounds ±{halfW} x ±{halfH}");

        // Cherche le joueur pour l'injection de dépendances et pour éviter de spawner sur sa position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform playerTransform = player != null ? player.transform : null;
        Vector2 playerPos = playerTransform != null
            ? (Vector2)playerTransform.position
            : new Vector2(float.MaxValue, float.MaxValue);

        // Cherche le LivesManager pour l'injection dans les ennemis
        LivesManager livesManager = FindFirstObjectByType<LivesManager>();

        if (playerTransform == null)
            Debug.LogWarning("[SpawnEnemies] Joueur introuvable — les ennemis seront inertes.", this);

        if (livesManager == null)
            Debug.LogWarning("[SpawnEnemies] LivesManager introuvable — les ennemis ne pourront pas infliger de dégâts.", this);

        for (int i = 0; i < count; i++)
        {
            // Choisit un prefab ennemi aléatoire dans le tableau
            GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            if (prefab == null)
            {
                Debug.LogWarning($"[SpawnEnemies] prefab null à l'index {i} — ignoré.", this);
                continue;
            }

            // Cherche une position qui ne chevauche pas le joueur
            Vector3 spawnPos;
            int maxAttempts = 10;

            do
            {
                float x = Random.Range(-halfW, halfW);
                float y = Random.Range(-halfH, halfH);
                spawnPos = new Vector3(x, y, 0f);
                maxAttempts--;
            }
            while (Vector2.Distance(spawnPos, playerPos) < 2f && maxAttempts > 0);

            Debug.Log($"[SpawnEnemies] Spawn {prefab.name} à {spawnPos}");
            GameObject enemyGO = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Injecte les dépendances runtime dans l'ennemi instancié
            IEnemyInjectable injectable = enemyGO.GetComponent<IEnemyInjectable>();
            if (injectable != null)
            {
                injectable.InjectDependencies(playerTransform, livesManager, _lootSystem);
            }
            else
            {
                Debug.LogWarning($"[SpawnEnemies] {prefab.name} n'implémente pas IEnemyInjectable — dépendances non injectées.", this);
            }

            // Applique les visuels selon le type d'ennemi instancié
            if (enemyGO.GetComponent<EnemyHidden>() != null)
                EnemyVisualBuilder.ApplyHiddenVisual(enemyGO);
            else if (enemyGO.GetComponent<EnemyShooter>() != null)
                EnemyVisualBuilder.ApplyShooterVisual(enemyGO);
            else if (enemyGO.GetComponent<EnemyCharger>() != null)
                EnemyVisualBuilder.ApplyChargerVisual(enemyGO);
        }
    }

    // -------------------------------------------------------------------------
    // Ouverture du mur de l'Open Space face à une salle procédurale
    // -------------------------------------------------------------------------

    // Perce une ouverture dans le mur de l'Open Space sur le côté d'une salle
    private void PierceOpenSpaceWall(WallSide wall, Vector2 roomCenter)
    {
        if (_mapRoot == null) return;

        // Cherche Room_OpenSpace dans les enfants du Map
        Transform openSpaceRoot = null;
        for (int i = 0; i < _mapRoot.childCount; i++)
        {
            if (_mapRoot.GetChild(i).name == "Room_OpenSpace")
            {
                openSpaceRoot = _mapRoot.GetChild(i);
                break;
            }
        }
        if (openSpaceRoot == null) return;

        // Identifie le nom du mur plein correspondant au côté demandé
        string wallName = wall switch
        {
            WallSide.Top    => "Wall_Top",
            WallSide.Bottom => "Wall_Bottom",
            WallSide.Left   => "Wall_Left",
            WallSide.Right  => "Wall_Right",
            _               => "Wall_Right"
        };

        // Cherche le mur à percer dans les enfants de Room_OpenSpace
        Transform wallTransform = null;
        for (int i = 0; i < openSpaceRoot.childCount; i++)
        {
            if (openSpaceRoot.GetChild(i).name == wallName)
            {
                wallTransform = openSpaceRoot.GetChild(i);
                break;
            }
        }
        if (wallTransform == null) return;

        // Désactive le mur plein et le remplace par deux segments
        wallTransform.gameObject.SetActive(false);

        float halfT = WallThickness * 0.5f;
        float halfD = _doorwayWidth * 0.5f;
        float halfOS_W = _openSpaceWidth  * 0.5f;
        float halfOS_H = _openSpaceHeight * 0.5f;

        Transform pieceParent = openSpaceRoot;

        // Les murs horizontaux (Top/Bottom) sont percés en deux segments X
        if (wall == WallSide.Top || wall == WallSide.Bottom)
        {
            float wallY    = wall == WallSide.Top ? halfOS_H + halfT : -(halfOS_H + halfT);
            float fullHalf = halfOS_W + halfT;    // demi-largeur du mur plein original
            float doorCX   = roomCenter.x;        // centre X de la porte = centre de la salle

            // Segment gauche — de -fullHalf à doorCX - halfD
            float leftW  = (doorCX - halfD) - (-fullHalf);
            float leftCX = -fullHalf + leftW * 0.5f;
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_L",
                new Vector2(leftW, WallThickness), new Vector2(leftCX, wallY));

            // Segment droit — de doorCX + halfD à +fullHalf
            float rightW  = fullHalf - (doorCX + halfD);
            float rightCX = doorCX + halfD + rightW * 0.5f;
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_R",
                new Vector2(rightW, WallThickness), new Vector2(rightCX, wallY));
        }
        else
        {
            // Les murs verticaux (Left/Right) sont percés en deux segments Y
            float wallX    = wall == WallSide.Right ? halfOS_W + halfT : -(halfOS_W + halfT);
            float fullHalf = halfOS_H + halfT;
            float doorCY   = roomCenter.y;

            // Segment haut — de doorCY + halfD à +fullHalf
            float topH  = fullHalf - (doorCY + halfD);
            float topCY = doorCY + halfD + topH * 0.5f;
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_T",
                new Vector2(WallThickness, topH), new Vector2(wallX, topCY));

            // Segment bas — de -fullHalf à doorCY - halfD
            float botH  = (doorCY - halfD) - (-fullHalf);
            float botCY = -fullHalf + botH * 0.5f;
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_B",
                new Vector2(WallThickness, botH), new Vector2(wallX, botCY));
        }
    }

    // Instancie un segment de mur de l'Open Space avec SpriteRenderer et BoxCollider2D
    private void SpawnOpenSpaceWallSegment(Transform parent, string goName,
                                            Vector2 size, Vector2 worldPos)
    {
        GameObject seg = new GameObject(goName);
        seg.transform.SetParent(parent, false);
        seg.transform.position   = new Vector3(worldPos.x, worldPos.y, 0f);
        seg.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = seg.AddComponent<SpriteRenderer>();
        sr.sprite         = CreateColorSprite(_wallColor);
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder   = 0;

        seg.AddComponent<BoxCollider2D>();
    }

    // -------------------------------------------------------------------------
    // Props thématiques par salle
    // -------------------------------------------------------------------------

    // Convertit le label de salle en RoomType
    private RoomType GetRoomTypeFromLabel(string label)
    {
        if (System.Enum.TryParse(label, out RoomType result)) return result;
        return RoomType.Toilettes;
    }

    // Génère les props intérieurs selon le type de salle
    private void SpawnRoomProps(Vector2 pos, Vector2 size, RoomType type, Transform parent)
    {
        GameObject propsRoot = new GameObject("Props_" + type);
        propsRoot.transform.SetParent(parent, false);
        Transform propsT = propsRoot.transform;

        switch (type)
        {
            case RoomType.Toilettes:
                SpawnToilettesProps(pos, size, propsT);
                break;
            case RoomType.SalleReunion:
                SpawnSalleReunionProps(pos, size, propsT);
                break;
            case RoomType.Vestiaire:
                SpawnVestiaireProps(pos, size, propsT);
                break;
            case RoomType.SalleRepos:
                SpawnSalleReposProps(pos, size, propsT);
                break;
        }
    }

    // Toilettes : 2-3 cabines WC + 1-2 lavabos
    private void SpawnToilettesProps(Vector2 pos, Vector2 size, Transform parent)
    {
        // Couleurs
        Color cabineColor = new Color(0.85f, 0.85f, 0.85f);
        Color wcColor     = new Color(0.95f, 0.95f, 1.0f);
        Color lavaboColor = new Color(0.75f, 0.88f, 1.0f);

        int cabineCount = Random.Range(2, 4);
        float startX    = pos.x - (cabineCount - 1) * 1.1f * 0.5f;

        for (int i = 0; i < cabineCount; i++)
        {
            float cx = startX + i * 1.1f;
            float cy = pos.y + size.y * 0.25f;

            // Cloison de la cabine — fouillable
            SpawnProp(parent, "Cabine_" + i, cabineColor,
                new Vector2(1.0f, 1.4f), new Vector2(cx, cy), 1,
                isSearchable: true, searchLabel: "Cabine WC");

            // Cuvette à l'intérieur (déco uniquement)
            SpawnProp(parent, "WC_" + i, wcColor,
                new Vector2(0.45f, 0.45f), new Vector2(cx, cy - 0.1f), 2);
        }

        // Lavabos sur le mur opposé — fouillables
        int lavaboCount = Random.Range(1, 3);
        for (int i = 0; i < lavaboCount; i++)
        {
            float lx = pos.x + (i - lavaboCount * 0.5f + 0.5f) * 1.2f;
            SpawnProp(parent, "Lavabo_" + i, lavaboColor,
                new Vector2(0.7f, 0.4f), new Vector2(lx, pos.y - size.y * 0.3f), 2,
                isSearchable: true, searchLabel: "Lavabo");
        }
    }

    // Salle de réunion : grande table centrale + chaises
    private void SpawnSalleReunionProps(Vector2 pos, Vector2 size, Transform parent)
    {
        Color tableColor  = new Color(0.45f, 0.3f, 0.15f);
        Color chaiseColor = new Color(0.3f, 0.3f, 0.35f);

        // Table centrale — fouillable
        SpawnProp(parent, "Table", tableColor,
            new Vector2(3.0f, 1.2f), pos, 1,
            isSearchable: true, searchLabel: "Table de réunion");

        // Chaises autour de la table (déco uniquement)
        int chairsPerSide = Random.Range(2, 4);
        for (int i = 0; i < chairsPerSide; i++)
        {
            float xOff = (i - chairsPerSide * 0.5f + 0.5f) * 0.9f;

            SpawnProp(parent, "Chaise_Top_" + i, chaiseColor,
                new Vector2(0.5f, 0.4f), new Vector2(pos.x + xOff, pos.y + 0.9f), 2);
            SpawnProp(parent, "Chaise_Bot_" + i, chaiseColor,
                new Vector2(0.5f, 0.4f), new Vector2(pos.x + xOff, pos.y - 0.9f), 2);
        }
    }

    // Vestiaire : rangée de casiers + banc
    private void SpawnVestiaireProps(Vector2 pos, Vector2 size, Transform parent)
    {
        Color casierColor = new Color(0.3f, 0.35f, 0.5f);
        Color bancColor   = new Color(0.5f, 0.35f, 0.2f);

        int casierCount = Random.Range(3, 6);
        float startX    = pos.x - (casierCount - 1) * 0.7f * 0.5f;

        for (int i = 0; i < casierCount; i++)
        {
            float cx = startX + i * 0.7f;
            // Casiers — fouillables
            SpawnProp(parent, "Casier_" + i, casierColor,
                new Vector2(0.6f, 1.4f), new Vector2(cx, pos.y + size.y * 0.2f), 1,
                isSearchable: true, searchLabel: "Casier");
        }

        // Banc devant les casiers (déco uniquement)
        SpawnProp(parent, "Banc", bancColor,
            new Vector2(casierCount * 0.7f * 0.8f, 0.3f),
            new Vector2(pos.x, pos.y - 0.2f), 1);
    }

    // Salle de repos : canapé, table basse, machine à café
    private void SpawnSalleReposProps(Vector2 pos, Vector2 size, Transform parent)
    {
        Color canapeColor  = new Color(0.55f, 0.35f, 0.55f);
        Color tableColor   = new Color(0.35f, 0.25f, 0.15f);
        Color machineColor = new Color(0.25f, 0.25f, 0.3f);
        Color plantColor   = new Color(0.2f, 0.55f, 0.2f);

        // Canapé (déco uniquement)
        SpawnProp(parent, "Canape", canapeColor,
            new Vector2(2.0f, 0.7f), new Vector2(pos.x, pos.y + 0.5f), 1);

        // Table basse — fouillable
        SpawnProp(parent, "TableBasse", tableColor,
            new Vector2(1.0f, 0.5f), new Vector2(pos.x, pos.y - 0.3f), 1,
            isSearchable: true, searchLabel: "Table basse");

        // Machine à café — fouillable, position légèrement aléatoire
        float machineX = pos.x + Random.Range(-size.x * 0.3f, size.x * 0.3f);
        SpawnProp(parent, "MachineCafe", machineColor,
            new Vector2(0.5f, 0.6f), new Vector2(machineX, pos.y - size.y * 0.3f), 2,
            isSearchable: true, searchLabel: "Machine à café");

        // Plante décorative dans un coin
        float plantX = pos.x + (Random.value > 0.5f ? size.x * 0.35f : -size.x * 0.35f);
        SpawnProp(parent, "Plante", plantColor,
            new Vector2(0.4f, 0.6f), new Vector2(plantX, pos.y + size.y * 0.3f), 2);
    }

    // Instancie un prop visuel simple avec SpriteRenderer et BoxCollider2D
    // Si isSearchable est vrai, ajoute un SearchableObject câblé au SearchUIManager
    private void SpawnProp(Transform parent, string goName, Color color,
                            Vector2 size, Vector2 worldPos, int sortingOrder,
                            bool isSearchable = false, string searchLabel = "")
    {
        GameObject prop = new GameObject("Prop_" + goName);
        prop.transform.SetParent(parent, false);
        prop.transform.position   = new Vector3(worldPos.x, worldPos.y, 0f);
        prop.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = prop.AddComponent<SpriteRenderer>();
        sr.sprite         = CreateColorSprite(color);
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder   = sortingOrder;

        BoxCollider2D col = prop.AddComponent<BoxCollider2D>();
        col.size          = Vector2.one;

        // Ajoute et câble un SearchableObject si le prop est fouillable
        if (isSearchable && _searchUIManager != null)
        {
            SearchableObject searchable = prop.AddComponent<SearchableObject>();
            string label = string.IsNullOrEmpty(searchLabel) ? goName : searchLabel;
            searchable.SetLabel(label);

            // Ajoute un LootDropper et le câble à l'InventoryManager et à la LootTable par défaut.
            LootDropper dropper = prop.AddComponent<LootDropper>();
            dropper.SetInventoryManager(_inventoryManager);
            dropper.SetLootTable(_defaultLootTable);
            searchable.SetLootDropper(dropper);

            searchable.OnPlayerEnterRange.AddListener(_searchUIManager.OnPlayerEnterRange);
            searchable.OnPlayerExitRange.AddListener(_searchUIManager.OnPlayerExitRange);
        }
    }

    // -------------------------------------------------------------------------

    // Génère les quatre murs d'une salle avec une ouverture vers l'Open Space
    private void BuildRoomWalls(Vector2 pos, Vector2 size, WallSide attachedWall,
                                 Transform parent, string roomName)
    {
        // Le passage est sur le côté de la salle qui fait face à l'Open Space
        WallSide doorwaySide = GetOppositeWall(attachedWall);

        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;
        float halfT = WallThickness * 0.5f;
        float halfD = _doorwayWidth * 0.5f;

        // Conteneur parent pour garder la hiérarchie propre
        GameObject wallsRoot = new GameObject(roomName + "_Walls");
        wallsRoot.transform.SetParent(parent, false);
        Transform walls = wallsRoot.transform;

        // ── Mur du haut ──────────────────────────────────────────────────────
        if (doorwaySide == WallSide.Top)
        {
            // Passage centré : deux segments gauche et droit
            float segW = halfW - halfD;
            SpawnWall(walls, "Wall_Top_L",
                new Vector2(segW, WallThickness),
                new Vector2(pos.x - halfD - segW * 0.5f, pos.y + halfH + halfT));
            SpawnWall(walls, "Wall_Top_R",
                new Vector2(segW, WallThickness),
                new Vector2(pos.x + halfD + segW * 0.5f, pos.y + halfH + halfT));
        }
        else
        {
            // Mur plein : largeur totale avec coins
            SpawnWall(walls, "Wall_Top",
                new Vector2(size.x + WallThickness, WallThickness),
                new Vector2(pos.x, pos.y + halfH + halfT));
        }

        // ── Mur du bas ───────────────────────────────────────────────────────
        if (doorwaySide == WallSide.Bottom)
        {
            float segW = halfW - halfD;
            SpawnWall(walls, "Wall_Bottom_L",
                new Vector2(segW, WallThickness),
                new Vector2(pos.x - halfD - segW * 0.5f, pos.y - halfH - halfT));
            SpawnWall(walls, "Wall_Bottom_R",
                new Vector2(segW, WallThickness),
                new Vector2(pos.x + halfD + segW * 0.5f, pos.y - halfH - halfT));
        }
        else
        {
            SpawnWall(walls, "Wall_Bottom",
                new Vector2(size.x + WallThickness, WallThickness),
                new Vector2(pos.x, pos.y - halfH - halfT));
        }

        // ── Mur droit ────────────────────────────────────────────────────────
        if (doorwaySide == WallSide.Right)
        {
            float segH = halfH - halfD;
            SpawnWall(walls, "Wall_Right_T",
                new Vector2(WallThickness, segH),
                new Vector2(pos.x + halfW + halfT, pos.y + halfD + segH * 0.5f));
            SpawnWall(walls, "Wall_Right_B",
                new Vector2(WallThickness, segH),
                new Vector2(pos.x + halfW + halfT, pos.y - halfD - segH * 0.5f));
        }
        else
        {
            SpawnWall(walls, "Wall_Right",
                new Vector2(WallThickness, size.y),
                new Vector2(pos.x + halfW + halfT, pos.y));
        }

        // ── Mur gauche ───────────────────────────────────────────────────────
        if (doorwaySide == WallSide.Left)
        {
            float segH = halfH - halfD;
            SpawnWall(walls, "Wall_Left_T",
                new Vector2(WallThickness, segH),
                new Vector2(pos.x - halfW - halfT, pos.y + halfD + segH * 0.5f));
            SpawnWall(walls, "Wall_Left_B",
                new Vector2(WallThickness, segH),
                new Vector2(pos.x - halfW - halfT, pos.y - halfD - segH * 0.5f));
        }
        else
        {
            SpawnWall(walls, "Wall_Left",
                new Vector2(WallThickness, size.y),
                new Vector2(pos.x - halfW - halfT, pos.y));
        }
    }

    // Instancie un mur avec SpriteRenderer coloré et BoxCollider2D
    private void SpawnWall(Transform parent, string goName, Vector2 size, Vector2 worldPos)
    {
        GameObject wall = new GameObject(goName);
        wall.transform.SetParent(parent, false);
        wall.transform.position  = new Vector3(worldPos.x, worldPos.y, 0f);
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite         = CreateColorSprite(_wallColor);
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder   = 0;

        wall.AddComponent<BoxCollider2D>();
    }

    // Retourne le côté opposé d'un mur de l'Open Space
    private WallSide GetOppositeWall(WallSide wall)
    {
        return wall switch
        {
            WallSide.Top    => WallSide.Bottom,
            WallSide.Bottom => WallSide.Top,
            WallSide.Right  => WallSide.Left,
            WallSide.Left   => WallSide.Right,
            _               => WallSide.Top
        };
    }

    // -------------------------------------------------------------------------
    // Utilitaire sprite
    // -------------------------------------------------------------------------

    // Crée un sprite de couleur unie depuis une texture d'un pixel
    private Sprite CreateColorSprite(Color color)
    {
        // Génère une texture 1×1 avec filtre Point pour éviter le flou
        Texture2D tex    = new Texture2D(1, 1);
        tex.filterMode   = FilterMode.Point;
        tex.SetPixel(0, 0, color);
        tex.Apply();

        // Retourne le sprite créé à partir de la texture générée
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
