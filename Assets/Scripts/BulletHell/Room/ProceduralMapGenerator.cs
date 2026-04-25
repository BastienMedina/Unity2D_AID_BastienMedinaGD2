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
    [SerializeField] private Color _colorToilettes = new Color(0f, 0f, 0f, 1f);

    // Couleur vert clair pour la salle de Réunion
    [SerializeField] private Color _colorSalleReunion = new Color(0f, 0f, 0f, 1f);

    // Couleur orange clair pour le Vestiaire
    [SerializeField] private Color _colorVestiaire = new Color(0f, 0f, 0f, 1f);

    // Couleur violet clair pour la salle de Repos
    [SerializeField] private Color _colorSalleRepos = new Color(0f, 0f, 0f, 1f);

    // Couleur gris foncé pour la salle ascenseur
    [SerializeField] private Color _colorElevator = new Color(0f, 0f, 0f, 1f);

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

    // Bibliothèque de prefabs de props d'environnement — optionnelle, fallback couleur si non assignée
    [SerializeField] private PropLibrary _propLibrary;

    // Prefab EnemyHidden instancié depuis les bureaux infestés
    [SerializeField] private GameObject _enemyHiddenPrefab;

    // Probabilité (0-1) qu'un bureau fouillable soit infesté par un EnemyHidden
    [SerializeField] [Range(0f, 1f)] private float _infestedChance = 0.25f;

    // -------------------------------------------------------------------------
    // Paramètres de génération procédurale de l'Open Space
    // -------------------------------------------------------------------------

    // Nombre de bureaux à placer dans l'Open Space
    [SerializeField] private int _deskCount = 6;

    // Nombre de plantes décoratives dans l'Open Space
    [SerializeField] private int _plantCount = 3;

    // Couleur de fallback pour les bureaux de l'Open Space
    [SerializeField] private Color _deskColor = new Color(0.33f, 0.33f, 0.33f, 1f);

    // Couleur de fallback pour les plantes de l'Open Space
    [SerializeField] private Color _plantColor = new Color(0.2f, 0.6f, 0.2f, 1f);

    // Marge par rapport aux bords de l'Open Space (évite les overlaps avec les murs)
    [SerializeField] private float _openSpaceMargin = 1.5f;

    // Espacement minimal entre deux props de l'Open Space
    [SerializeField] private float _minPropSpacing = 2.0f;

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
        // Cherche Map dans la scène si non assigné en Inspector.
        if (_mapRoot == null)
        {
            GameObject mapGO = GameObject.Find("Map");
            if (mapGO != null)
                _mapRoot = mapGO.transform;
            else
                Debug.LogError("[ProceduralMapGenerator] GameObject 'Map' introuvable dans la scène !");
        }

        // Cherche le SearchUIManager dans la scène si non assigné en Inspector.
        // Appelé dans Start (et non Awake) pour garantir que tous les Awake()
        // sont terminés et que SearchUIManager est bien initialisé.
        if (_searchUIManager == null)
            _searchUIManager = FindFirstObjectByType<SearchUIManager>();

        // Cherche l'InventoryManager pour le câbler aux LootDropper spawned.
        _inventoryManager = FindFirstObjectByType<InventoryManager>();

        // Cherche le LootSystem pour l'injecter dans les ennemis spawned.
        _lootSystem = FindFirstObjectByType<LootSystem>();

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
            GetRoomColor(room1Type), room1Type.ToString(), room1Wall);
        PierceOpenSpaceWall(room1Wall, pos1);

        // Construit les visuels et objets fouillables de la salle 2
        BuildRoomVisual(pos2, new Vector2(_smallRoomWidth, _smallRoomHeight),
            GetRoomColor(room2Type), room2Type.ToString(), room2Wall);
        PierceOpenSpaceWall(room2Wall, pos2);

        // Construit la salle ascenseur avec son trigger
        BuildElevator(posElev, elevWall);
        PierceOpenSpaceWall(elevWall, posElev);

        // Repositionne le joueur devant l'entrée de l'ascenseur
        PositionPlayerAtElevatorEntrance(posElev, elevWall);

        // Instancie les ennemis dans l'Open Space selon l'étage
        SpawnEnemies();

        // Place les props procéduraux dans l'Open Space (bureaux, plantes)
        SpawnOpenSpaceProps();
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
        return Color.black;
    }

    // -------------------------------------------------------------------------
    // Construction visuelle des salles secondaires
    // -------------------------------------------------------------------------

    // Construit le visuel, le label et les objets fouillables d'une salle
    private void BuildRoomVisual(Vector2 pos, Vector2 size, Color color,
                                  string label, WallSide attachedWall)
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
    // Floor 1 → _baseEnemyCount (3), puis +1 par étage supplémentaire
    private int GetEnemyCount()
    {
        int floor = GameProgress.Instance != null ? GameProgress.Instance.CurrentFloor : 1;
        return _baseEnemyCount + (floor - 1);
    }

    // Instancie les ennemis aléatoirement dans les bornes de l'Open Space
    private void SpawnEnemies()
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Length == 0)
            return;

        int count   = GetEnemyCount();
        float halfW = _openSpaceWidth  * 0.5f - 1f;
        float halfH = _openSpaceHeight * 0.5f - 1f;

        // Cherche le joueur pour l'injection de dépendances et pour éviter de spawner sur sa position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform playerTransform = player != null ? player.transform : null;
        Vector2 playerPos = playerTransform != null
            ? (Vector2)playerTransform.position
            : new Vector2(float.MaxValue, float.MaxValue);

        // Cherche le LivesManager pour l'injection dans les ennemis
        LivesManager livesManager = FindFirstObjectByType<LivesManager>();

        for (int i = 0; i < count; i++)
        {
            // Choisit un prefab ennemi aléatoire dans le tableau
            GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            if (prefab == null)
                continue;

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

            GameObject enemyGO = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Injecte les dépendances runtime dans l'ennemi instancié
            IEnemyInjectable injectable = enemyGO.GetComponent<IEnemyInjectable>();
            if (injectable != null)
                injectable.InjectDependencies(playerTransform, livesManager, _lootSystem);

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
    // Spawn du joueur devant l'entrée de l'ascenseur
    // -------------------------------------------------------------------------

    // Distance en unités entre le bord de l'Open Space et le point de spawn du joueur
    private const float PlayerSpawnOffset = 1.5f;

    // Repositionne le joueur juste devant l'ouverture de l'ascenseur côté Open Space.
    // La position est calculée à partir du mur de l'ascenseur pour rester cohérente
    // quelle que soit la disposition procédurale de l'étage.
    private void PositionPlayerAtElevatorEntrance(Vector2 elevCenter, WallSide elevWall)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        float halfW = _openSpaceWidth  * 0.5f;
        float halfH = _openSpaceHeight * 0.5f;

        // Calcule la position à l'intérieur de l'Open Space, face à l'ouverture de l'ascenseur
        Vector2 spawnPos = elevWall switch
        {
            WallSide.Top    => new Vector2(elevCenter.x,  halfH - PlayerSpawnOffset),
            WallSide.Bottom => new Vector2(elevCenter.x, -halfH + PlayerSpawnOffset),
            WallSide.Right  => new Vector2( halfW - PlayerSpawnOffset, elevCenter.y),
            WallSide.Left   => new Vector2(-halfW + PlayerSpawnOffset, elevCenter.y),
            _               => Vector2.zero
        };

        // Repositionne le Rigidbody2D si présent pour éviter les artefacts physiques
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = spawnPos;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            player.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        }
    }

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
    // Props procéduraux de l'Open Space
    // -------------------------------------------------------------------------

    // Place bureaux en grille régulière et plantes dans les coins de l'Open Space
    private void SpawnOpenSpaceProps()
    {
        if (_mapRoot == null) return;

        Transform roomRoot = null;
        for (int i = 0; i < _mapRoot.childCount; i++)
        {
            if (_mapRoot.GetChild(i).name == "Room_OpenSpace")
            {
                roomRoot = _mapRoot.GetChild(i);
                break;
            }
        }

        if (roomRoot == null)
            return;

        GameObject propsRoot = new GameObject("Props_OpenSpace");
        // Parent direct sur _mapRoot (scale 1,1,1) et non sur Room_OpenSpace (scale 20×14)
        // pour que les BoxCollider2D et transforms des props restent en espace monde cohérent.
        propsRoot.transform.SetParent(_mapRoot, false);
        Transform parent = propsRoot.transform;

        // ── Dimensions utiles après marge ─────────────────────────────────────
        float usableW = _openSpaceWidth  - 2f * _openSpaceMargin;   // 17u
        float usableH = _openSpaceHeight - 2f * _openSpaceMargin;   // 11u

        // ── Grille de bureaux ─────────────────────────────────────────────────
        // cols/rows calculés pour remplir uniformément la zone utile
        int cols = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(_deskCount * (usableW / usableH))));
        int rows = Mathf.CeilToInt((float)_deskCount / cols);

        // Pas entre chaque cellule de grille
        float colStep = usableW / cols;
        float rowStep = usableH / rows;

        // Jitter max = 15% du pas pour casser la monotonie sans désorganiser la grille
        float jitterX = colStep * 0.15f;
        float jitterY = rowStep * 0.15f;

        // Coin haut-gauche de la première cellule, centré sur la salle (origine = 0,0)
        float startX = -(usableW * 0.5f) + (colStep * 0.5f);
        float startY = -(usableH * 0.5f) + (rowStep * 0.5f);

        int deskIdx = 0;
        for (int row = 0; row < rows && deskIdx < _deskCount; row++)
        {
            for (int col = 0; col < cols && deskIdx < _deskCount; col++)
            {
                float x = startX + col * colStep + Random.Range(-jitterX, jitterX);
                float y = startY + row * rowStep + Random.Range(-jitterY, jitterY);

                SpawnProp(parent, "Desk_" + (deskIdx + 1),
                    _propLibrary?.deskPrefab, _deskColor,
                    new Vector2(3.0f, 2.0f), new Vector2(x, y), 1,
                    isSearchable: true, searchLabel: "Bureau");
                deskIdx++;
            }
        }

        // ── Plantes dans les quatre coins ─────────────────────────────────────
        float halfW = usableW * 0.5f;
        float halfH = usableH * 0.5f;

        // Décalage depuis le coin pour garder la plante dans la salle
        float cornerOffset = 0.5f;

        Vector2[] cornerPositions = new Vector2[]
        {
            new Vector2(-halfW + cornerOffset,  halfH - cornerOffset),  // coin haut-gauche
            new Vector2( halfW - cornerOffset,  halfH - cornerOffset),  // coin haut-droit
            new Vector2(-halfW + cornerOffset, -halfH + cornerOffset),  // coin bas-gauche
            new Vector2( halfW - cornerOffset, -halfH + cornerOffset),  // coin bas-droit
        };

        int plantCount = Mathf.Min(_plantCount, cornerPositions.Length);
        for (int i = 0; i < plantCount; i++)
        {
            // Léger jitter sur la position de coin pour varier entre les runs
            Vector2 pos = cornerPositions[i] + new Vector2(
                Random.Range(-cornerOffset * 0.3f, cornerOffset * 0.3f),
                Random.Range(-cornerOffset * 0.3f, cornerOffset * 0.3f));

            SpawnProp(parent, "Plante_" + (i + 1),
                _propLibrary?.plantePrefab, _plantColor,
                new Vector2(1.2f, 1.2f), pos, 2);
        }
    }

    // Retourne les bounds d'un GameObject instancié à localScale=1 en lisant son SpriteRenderer.
    // Utilisé pour calculer le scale compensé indépendamment du PPU du sprite.
    private Bounds CalculatePrefabBounds(GameObject go)
    {
        // Sauvegarde le scale courant et réinitialise à 1 pour lire les bounds natifs
        Vector3 savedScale = go.transform.localScale;
        go.transform.localScale = Vector3.one;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        Bounds b = sr != null ? sr.bounds : new Bounds(Vector3.zero, Vector3.zero);

        // Restaure le scale original
        go.transform.localScale = savedScale;
        return b;
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
            SpawnProp(parent, "Cabine_" + i, _propLibrary?.cabinePrefab, cabineColor,
                new Vector2(1.0f, 1.4f), new Vector2(cx, cy), 1,
                isSearchable: true, searchLabel: "Cabine WC");

            // Cuvette à l'intérieur (déco uniquement)
            SpawnProp(parent, "WC_" + i, _propLibrary?.wcPrefab, wcColor,
                new Vector2(0.45f, 0.45f), new Vector2(cx, cy - 0.1f), 2);
        }

        // Lavabos sur le mur opposé — fouillables
        int lavaboCount = Random.Range(1, 3);
        for (int i = 0; i < lavaboCount; i++)
        {
            float lx = pos.x + (i - lavaboCount * 0.5f + 0.5f) * 1.2f;
            SpawnProp(parent, "Lavabo_" + i, _propLibrary?.lavaboPrefab, lavaboColor,
                new Vector2(0.7f, 0.4f), new Vector2(lx, pos.y - size.y * 0.3f), 2,
                isSearchable: true, searchLabel: "Lavabo");
        }
    }

    // Salle de réunion : grande table centrale (les chaises sont incluses dans le sprite)
    private void SpawnSalleReunionProps(Vector2 pos, Vector2 size, Transform parent)
    {
        Color tableColor = new Color(0.45f, 0.3f, 0.15f);

        // Table centrale — fouillable (sprite inclut déjà les chaises)
        SpawnProp(parent, "Table", _propLibrary?.tableReunionPrefab, tableColor,
            new Vector2(3.0f, 1.2f), pos, 1,
            isSearchable: true, searchLabel: "Table de réunion");
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
            SpawnProp(parent, "Casier_" + i, _propLibrary?.casierPrefab, casierColor,
                new Vector2(0.6f, 1.4f), new Vector2(cx, pos.y + size.y * 0.2f), 1,
                isSearchable: true, searchLabel: "Casier");
        }

        // Banc devant les casiers (déco uniquement)
        SpawnProp(parent, "Banc", _propLibrary?.bancPrefab, bancColor,
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
        SpawnProp(parent, "Canape", _propLibrary?.canapePrefab, canapeColor,
            new Vector2(2.0f, 0.7f), new Vector2(pos.x, pos.y + 0.5f), 1);

        // Table basse — fouillable
        SpawnProp(parent, "TableBasse", _propLibrary?.tableBassePrefab, tableColor,
            new Vector2(1.0f, 0.5f), new Vector2(pos.x, pos.y - 0.3f), 1,
            isSearchable: true, searchLabel: "Table basse");

        // Machine à café — fouillable, position légèrement aléatoire
        float machineX = pos.x + Random.Range(-size.x * 0.3f, size.x * 0.3f);
        SpawnProp(parent, "MachineCafe", _propLibrary?.machineCafePrefab, machineColor,
            new Vector2(0.5f, 0.6f), new Vector2(machineX, pos.y - size.y * 0.3f), 2,
            isSearchable: true, searchLabel: "Machine à café");

        // Plante décorative dans un coin
        float plantX = pos.x + (Random.value > 0.5f ? size.x * 0.35f : -size.x * 0.35f);
        SpawnProp(parent, "Plante", _propLibrary?.plantePrefab, plantColor,
            new Vector2(0.4f, 0.6f), new Vector2(plantX, pos.y + size.y * 0.3f), 2);
    }

    // Instancie un prop depuis un prefab (si fourni) ou via un sprite couleur de fallback.
    // Si isSearchable est vrai, ajoute un SearchableObject câblé au SearchUIManager.
    private void SpawnProp(Transform parent, string goName, GameObject prefab, Color fallbackColor,
                            Vector2 size, Vector2 worldPos, int sortingOrder,
                            bool isSearchable = false, string searchLabel = "")
    {
        GameObject prop;

        if (prefab != null)
        {
            prop = Instantiate(prefab, new Vector3(worldPos.x, worldPos.y, 0f),
                               Quaternion.identity, parent);
            prop.name = "Prop_" + goName;

            SpriteRenderer sr = prop.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = sortingOrder;

            if (sr != null && sr.sprite != null)
            {
                // Adapte le scale pour que le sprite occupe exactement la taille monde demandée
                float nativeW = sr.sprite.rect.width  / sr.sprite.pixelsPerUnit;
                float nativeH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                prop.transform.localScale = new Vector3(size.x / nativeW, size.y / nativeH, 1f);
            }
            else
            {
                // Le prefab existe mais son sprite est null : applique le fallback couleur
                // pour éviter qu'un SpriteRenderer vide rende un rectangle gris visible.
                if (sr == null)
                    sr = prop.AddComponent<SpriteRenderer>();

                sr.sprite        = CreateColorSprite(fallbackColor);
                sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
                sr.sortingOrder  = sortingOrder;
                prop.transform.localScale = new Vector3(size.x, size.y, 1f);
            }
        }
        else
        {
            prop = new GameObject("Prop_" + goName);
            prop.transform.SetParent(parent, false);
            prop.transform.position   = new Vector3(worldPos.x, worldPos.y, 0f);
            prop.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer sr  = prop.AddComponent<SpriteRenderer>();
            sr.sprite          = CreateColorSprite(fallbackColor);
            sr.sharedMaterial  = new Material(Shader.Find(URPShaderName));
            sr.sortingOrder    = sortingOrder;
        }

        // Tous les props sont des obstacles solides — le BoxCollider2D n'est jamais un trigger.
        // La détection de fouille est gérée par SearchableObject.CheckPlayerProximity()
        // via Vector2.Distance, indépendamment du système physique.
        BoxCollider2D col = prop.GetComponent<BoxCollider2D>()
                         ?? prop.AddComponent<BoxCollider2D>();

        // La size du BoxCollider2D s'exprime en espace local — on divise par le scale
        // appliqué pour que le collider corresponde exactement aux dimensions monde du prop.
        Vector3 s = prop.transform.localScale;
        float colW = s.x != 0 ? size.x / s.x : 1f;
        float colH = s.y != 0 ? size.y / s.y : 1f;
        col.size      = new Vector2(colW, colH);
        col.isTrigger = false;

        // Ajoute et câble un SearchableObject si le prop est fouillable
        if (isSearchable && _searchUIManager != null)
        {
            SearchableObject searchable = prop.GetComponent<SearchableObject>()
                                       ?? prop.AddComponent<SearchableObject>();
            string label = string.IsNullOrEmpty(searchLabel) ? goName : searchLabel;
            searchable.SetLabel(label);

            LootDropper dropper = prop.GetComponent<LootDropper>()
                                ?? prop.AddComponent<LootDropper>();
            dropper.SetInventoryManager(_inventoryManager);
            dropper.SetLootTable(_defaultLootTable);
            searchable.SetLootDropper(dropper);

            searchable.OnPlayerEnterRange.AddListener(_searchUIManager.OnPlayerEnterRange);
            searchable.OnPlayerExitRange.AddListener(_searchUIManager.OnPlayerExitRange);

            // Configure la probabilité d'infestation et le prefab ennemi caché
            searchable.SetInfested(_infestedChance, _enemyHiddenPrefab);
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
