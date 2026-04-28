using UnityEngine;

[DefaultExecutionOrder(10)]
public class ProceduralMapGenerator : MonoBehaviour
{
    private const string URPShaderName  = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
    private const float  WallThickness  = 0.3f;
    private const float  PlayerSpawnOffset = 1.5f;

    public enum RoomType { Toilettes, SalleReunion, Vestiaire, SalleRepos }
    private enum WallSide { Top, Right, Bottom, Left }

    [SerializeField] private float _openSpaceWidth  = 20f;
    [SerializeField] private float _openSpaceHeight = 14f;
    [SerializeField] private float _smallRoomWidth  = 6f;
    [SerializeField] private float _smallRoomHeight = 5f;
    [SerializeField] private float _elevatorWidth   = 3f;
    [SerializeField] private float _elevatorHeight  = 3f;
    [SerializeField] private float _doorwayWidth    = 2f;

    [SerializeField] private Color _colorToilettes   = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color _colorSalleReunion = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color _colorVestiaire   = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color _colorSalleRepos  = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color _colorElevator    = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color _wallColor        = Color.white;

    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private int _baseEnemyCount = 3;
    [SerializeField] private GameObject _searchableObjectPrefab;
    [SerializeField] private SearchUIManager _searchUIManager;
    [SerializeField] private LootTable _defaultLootTable;
    [SerializeField] private PropLibrary _propLibrary;
    [SerializeField] private GameObject _enemyHiddenPrefab;
    [SerializeField] [Range(0f, 1f)] private float _infestedChance = 0.25f;

    [SerializeField] private int   _deskCount       = 6;
    [SerializeField] private int   _plantCount      = 3;
    [SerializeField] private Color _deskColor       = new Color(0.33f, 0.33f, 0.33f, 1f);
    [SerializeField] private Color _plantColor      = new Color(0.2f,  0.6f,  0.2f,  1f);
    [SerializeField] private float _openSpaceMargin = 1.5f;
    [SerializeField] private float _minPropSpacing  = 2.0f;
    [SerializeField] private Transform _mapRoot;

    private InventoryManager _inventoryManager;
    private LootSystem       _lootSystem;

    private void Start() // Génère la carte complète après tous les Awake
    {
        if (_mapRoot == null)
        {
            GameObject mapGO = GameObject.Find("Map");
            if (mapGO != null)
                _mapRoot = mapGO.transform;
            else
                Debug.LogError("[ProceduralMapGenerator] GameObject 'Map' introuvable dans la scène !");
        }

        if (_searchUIManager == null)
            _searchUIManager = FindFirstObjectByType<SearchUIManager>();

        _inventoryManager = FindFirstObjectByType<InventoryManager>();
        _lootSystem       = FindFirstObjectByType<LootSystem>();

        GenerateFloor();
    }

    private void GenerateFloor() // Orchestre la génération de toutes les salles
    {
        try { GenerateFloorInternal(); }
        catch (System.Exception e)
        {
            Debug.LogError($"[ProceduralMapGenerator] Exception dans GenerateFloor : {e}", this);
        }
    }

    private void GenerateFloorInternal() // Tire les types et murs puis construit l'étage
    {
        RoomType room1Type = (RoomType)Random.Range(0, 4);
        RoomType room2Type;
        do { room2Type = (RoomType)Random.Range(0, 4); } // Évite les doublons de type
        while (room2Type == room1Type);

        WallSide[] walls   = PickThreeDistinctWalls();
        WallSide room1Wall = walls[0];
        WallSide room2Wall = walls[1];
        WallSide elevWall  = walls[2];

        Vector2 pos1    = CalcSmallRoomPosition(room1Wall);
        Vector2 pos2    = CalcSmallRoomPosition(room2Wall);
        Vector2 posElev = CalcElevatorPosition(elevWall);

        BuildRoomVisual(pos1, new Vector2(_smallRoomWidth, _smallRoomHeight), GetRoomColor(room1Type), room1Type.ToString(), room1Wall);
        PierceOpenSpaceWall(room1Wall, pos1);

        BuildRoomVisual(pos2, new Vector2(_smallRoomWidth, _smallRoomHeight), GetRoomColor(room2Type), room2Type.ToString(), room2Wall);
        PierceOpenSpaceWall(room2Wall, pos2);

        BuildElevator(posElev, elevWall);
        PierceOpenSpaceWall(elevWall, posElev);

        PositionPlayerAtElevatorEntrance(posElev, elevWall);
        SpawnEnemies();
        SpawnOpenSpaceProps();
    }

    private WallSide[] PickThreeDistinctWalls() // Tire trois murs distincts aléatoirement
    {
        WallSide[] all = { WallSide.Top, WallSide.Right, WallSide.Bottom, WallSide.Left };

        for (int i = all.Length - 1; i > 0; i--) // Mélange Fisher-Yates
        {
            int j = Random.Range(0, i + 1);
            WallSide tmp = all[i]; all[i] = all[j]; all[j] = tmp;
        }

        return new WallSide[] { all[0], all[1], all[2] };
    }

    private Vector2 CalcSmallRoomPosition(WallSide wall) // Calcule la position d'une petite salle
    {
        float halfW  = _openSpaceWidth  * 0.5f;
        float halfH  = _openSpaceHeight * 0.5f;
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

    private Vector2 CalcElevatorPosition(WallSide wall) // Calcule la position de l'ascenseur
    {
        float halfW  = _openSpaceWidth  * 0.5f;
        float halfH  = _openSpaceHeight * 0.5f;
        float halfEW = _elevatorWidth   * 0.5f;
        float halfEH = _elevatorHeight  * 0.5f;

        return wall switch
        {
            WallSide.Top    => new Vector2(0f,  halfH + halfEH),
            WallSide.Bottom => new Vector2(0f, -halfH - halfEH),
            WallSide.Right  => new Vector2( halfW + halfEW, 0f),
            WallSide.Left   => new Vector2(-halfW - halfEW, 0f),
            _               => Vector2.zero
        };
    }

    private Color GetRoomColor(RoomType type) => Color.black; // Retourne la couleur selon le type

    private void BuildRoomVisual(Vector2 pos, Vector2 size, Color color, string label, WallSide attachedWall) // Construit visuels, props et murs d'une salle
    {
        Transform parent  = _mapRoot != null ? _mapRoot : transform;
        GameObject room   = new GameObject("Room_" + label);
        room.transform.SetParent(parent, false);

        SpriteRenderer sr  = room.AddComponent<SpriteRenderer>();
        sr.sprite          = CreateColorSprite(color);
        sr.sharedMaterial  = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder    = -1;
        room.transform.position   = new Vector3(pos.x, pos.y, 0f);
        room.transform.localScale = new Vector3(size.x, size.y, 1f);

        RoomType parsedType = GetRoomTypeFromLabel(label);
        SpawnRoomProps(pos, size, parsedType, parent);
        BuildRoomWalls(pos, size, attachedWall, parent, "Room_" + label);
    }

    private void BuildElevator(Vector2 pos, WallSide attachedWall) // Construit la salle ascenseur avec trigger
    {
        Transform parent  = _mapRoot != null ? _mapRoot : transform;
        GameObject elev   = new GameObject("Room_Ascenseur");
        elev.transform.SetParent(parent, false);

        SpriteRenderer sr  = elev.AddComponent<SpriteRenderer>();
        sr.sprite          = CreateColorSprite(_colorElevator);
        sr.sharedMaterial  = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder    = -1;
        elev.transform.position   = new Vector3(pos.x, pos.y, 0f);
        elev.transform.localScale = new Vector3(_elevatorWidth, _elevatorHeight, 1f);

        BoxCollider2D col = elev.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2((_elevatorWidth - 0.4f) / _elevatorWidth, (_elevatorHeight - 0.4f) / _elevatorHeight);

        ElevatorDoorController doorCtrl = elev.AddComponent<ElevatorDoorController>();
        elev.AddComponent<ElevatorCountdownUI>();
        elev.AddComponent<ElevatorTrigger>();

        WallSide doorwaySide = GetOppositeWall(attachedWall);
        doorCtrl.Configure(WallSideToDirection(doorwaySide), _doorwayWidth);

        BuildRoomWalls(pos, new Vector2(_elevatorWidth, _elevatorHeight), attachedWall, parent, "Room_Ascenseur");
    }

    private int GetEnemyCount() // Calcule le nombre d'ennemis selon l'étage
    {
        int floor = GameProgress.Instance != null ? GameProgress.Instance.CurrentFloor : 1;
        return _baseEnemyCount + (floor - 1);
    }

    private void SpawnEnemies() // Instancie les ennemis dans l'Open Space
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Length == 0) return;

        int count   = GetEnemyCount();
        float halfW = _openSpaceWidth  * 0.5f - 1f;
        float halfH = _openSpaceHeight * 0.5f - 1f;

        GameObject player        = GameObject.FindGameObjectWithTag("Player");
        Transform playerTransform = player != null ? player.transform : null;

        if (playerTransform == null)
            Debug.LogWarning("[ProceduralMapGenerator] Aucun GameObject taggé 'Player' trouvé. Les ennemis seront inertes.");

        Vector2 playerPos     = playerTransform != null ? (Vector2)playerTransform.position : new Vector2(float.MaxValue, float.MaxValue);
        LivesManager livesMgr = FindFirstObjectByType<LivesManager>();

        for (int i = 0; i < count; i++) // Instancie chaque ennemi à une position aléatoire
        {
            GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            if (prefab == null) continue;

            Vector3 spawnPos;
            int maxAttempts = 10;
            do // Cherche une position éloignée du joueur
            {
                spawnPos = new Vector3(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH), 0f);
                maxAttempts--;
            }
            while (Vector2.Distance(spawnPos, playerPos) < 2f && maxAttempts > 0);

            GameObject enemyGO = Instantiate(prefab, spawnPos, Quaternion.identity);

            IEnemyInjectable injectable = enemyGO.GetComponent<IEnemyInjectable>();
            if (injectable != null)
                injectable.InjectDependencies(playerTransform, livesMgr, _lootSystem);

            if      (enemyGO.GetComponent<EnemyHidden>()  != null) EnemyVisualBuilder.ApplyHiddenVisual(enemyGO);  // Applique visuel caché
            else if (enemyGO.GetComponent<EnemyShooter>() != null) EnemyVisualBuilder.ApplyShooterVisual(enemyGO); // Applique visuel tireur
            else if (enemyGO.GetComponent<EnemyCharger>() != null) EnemyVisualBuilder.ApplyChargerVisual(enemyGO); // Applique visuel chargeur
        }
    }

    private void PositionPlayerAtElevatorEntrance(Vector2 elevCenter, WallSide elevWall) // Repositionne le joueur devant l'ascenseur
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float halfW = _openSpaceWidth  * 0.5f;
        float halfH = _openSpaceHeight * 0.5f;

        Vector2 spawnPos = elevWall switch // Calcule la position face à l'ouverture
        {
            WallSide.Top    => new Vector2(elevCenter.x,  halfH - PlayerSpawnOffset),
            WallSide.Bottom => new Vector2(elevCenter.x, -halfH + PlayerSpawnOffset),
            WallSide.Right  => new Vector2( halfW - PlayerSpawnOffset, elevCenter.y),
            WallSide.Left   => new Vector2(-halfW + PlayerSpawnOffset, elevCenter.y),
            _               => Vector2.zero
        };

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = spawnPos; rb.linearVelocity = Vector2.zero; }
        else player.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
    }

    private void PierceOpenSpaceWall(WallSide wall, Vector2 roomCenter) // Perce une ouverture dans le mur de l'Open Space
    {
        if (_mapRoot == null) return;

        Transform openSpaceRoot = null;
        for (int i = 0; i < _mapRoot.childCount; i++) // Cherche Room_OpenSpace parmi les enfants
        {
            if (_mapRoot.GetChild(i).name == "Room_OpenSpace") { openSpaceRoot = _mapRoot.GetChild(i); break; }
        }
        if (openSpaceRoot == null) return;

        string wallName = wall switch
        {
            WallSide.Top    => "Wall_Top",
            WallSide.Bottom => "Wall_Bottom",
            WallSide.Left   => "Wall_Left",
            WallSide.Right  => "Wall_Right",
            _               => "Wall_Right"
        };

        Transform wallTransform = null;
        for (int i = 0; i < openSpaceRoot.childCount; i++) // Cherche le mur à percer
        {
            if (openSpaceRoot.GetChild(i).name == wallName) { wallTransform = openSpaceRoot.GetChild(i); break; }
        }
        if (wallTransform == null) return;

        wallTransform.gameObject.SetActive(false); // Désactive le mur plein

        float halfT    = WallThickness * 0.5f;
        float halfD    = _doorwayWidth * 0.5f;
        float halfOS_W = _openSpaceWidth  * 0.5f;
        float halfOS_H = _openSpaceHeight * 0.5f;
        Transform pieceParent = openSpaceRoot;

        if (wall == WallSide.Top || wall == WallSide.Bottom) // Perce un mur horizontal
        {
            float wallY    = wall == WallSide.Top ? halfOS_H + halfT : -(halfOS_H + halfT);
            float fullHalf = halfOS_W + halfT;
            float doorCX   = roomCenter.x;

            float leftW  = (doorCX - halfD) - (-fullHalf);
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_L", new Vector2(leftW, WallThickness), new Vector2(-fullHalf + leftW * 0.5f, wallY));

            float rightW = fullHalf - (doorCX + halfD);
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_R", new Vector2(rightW, WallThickness), new Vector2(doorCX + halfD + rightW * 0.5f, wallY));
        }
        else // Perce un mur vertical
        {
            float wallX    = wall == WallSide.Right ? halfOS_W + halfT : -(halfOS_W + halfT);
            float fullHalf = halfOS_H + halfT;
            float doorCY   = roomCenter.y;

            float topH  = fullHalf - (doorCY + halfD);
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_T", new Vector2(WallThickness, topH), new Vector2(wallX, doorCY + halfD + topH * 0.5f));

            float botH  = (doorCY - halfD) - (-fullHalf);
            SpawnOpenSpaceWallSegment(pieceParent, wallName + "_Seg_B", new Vector2(WallThickness, botH), new Vector2(wallX, -fullHalf + botH * 0.5f));
        }
    }

    private void SpawnOpenSpaceWallSegment(Transform parent, string goName, Vector2 size, Vector2 worldPos) // Instancie un segment de mur avec sprite et collider
    {
        GameObject seg        = new GameObject(goName);
        seg.transform.SetParent(parent, false);
        seg.transform.position   = new Vector3(worldPos.x, worldPos.y, 0f);
        seg.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr  = seg.AddComponent<SpriteRenderer>();
        sr.sprite          = CreateColorSprite(_wallColor);
        sr.sharedMaterial  = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder    = 0;

        seg.AddComponent<BoxCollider2D>();
    }

    private void SpawnOpenSpaceProps() // Place bureaux et plantes dans l'Open Space
    {
        if (_mapRoot == null) return;

        Transform roomRoot = null;
        for (int i = 0; i < _mapRoot.childCount; i++) // Cherche Room_OpenSpace
        {
            if (_mapRoot.GetChild(i).name == "Room_OpenSpace") { roomRoot = _mapRoot.GetChild(i); break; }
        }
        if (roomRoot == null) return;

        GameObject propsRoot = new GameObject("Props_OpenSpace");
        propsRoot.transform.SetParent(_mapRoot, false);
        Transform parent = propsRoot.transform;

        float usableW = _openSpaceWidth  - 2f * _openSpaceMargin;
        float usableH = _openSpaceHeight - 2f * _openSpaceMargin;

        int cols    = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(_deskCount * (usableW / usableH))));
        int rows    = Mathf.CeilToInt((float)_deskCount / cols);
        float colStep = usableW / cols;
        float rowStep = usableH / rows;
        float jitterX = colStep * 0.15f;
        float jitterY = rowStep * 0.15f;
        float startX  = -(usableW * 0.5f) + (colStep * 0.5f);
        float startY  = -(usableH * 0.5f) + (rowStep * 0.5f);

        int deskIdx = 0;
        for (int row = 0; row < rows && deskIdx < _deskCount; row++)
        {
            for (int col = 0; col < cols && deskIdx < _deskCount; col++) // Instancie chaque bureau sur la grille
            {
                float x = startX + col * colStep + Random.Range(-jitterX, jitterX);
                float y = startY + row * rowStep + Random.Range(-jitterY, jitterY);
                SpawnProp(parent, "Desk_" + (deskIdx + 1), _propLibrary?.deskPrefab, _deskColor, new Vector2(3.0f, 2.0f), new Vector2(x, y), 1, isSearchable: true, searchLabel: "Bureau");
                deskIdx++;
            }
        }

        float halfW        = usableW * 0.5f;
        float halfH        = usableH * 0.5f;
        float cornerOffset = 0.5f;

        Vector2[] cornerPositions = new Vector2[]
        {
            new Vector2(-halfW + cornerOffset,  halfH - cornerOffset),
            new Vector2( halfW - cornerOffset,  halfH - cornerOffset),
            new Vector2(-halfW + cornerOffset, -halfH + cornerOffset),
            new Vector2( halfW - cornerOffset, -halfH + cornerOffset),
        };

        int plantCount = Mathf.Min(_plantCount, cornerPositions.Length);
        for (int i = 0; i < plantCount; i++) // Instancie chaque plante dans un coin
        {
            Vector2 pos = cornerPositions[i] + new Vector2(Random.Range(-cornerOffset * 0.3f, cornerOffset * 0.3f), Random.Range(-cornerOffset * 0.3f, cornerOffset * 0.3f));
            SpawnProp(parent, "Plante_" + (i + 1), _propLibrary?.plantePrefab, _plantColor, new Vector2(1.2f, 1.2f), pos, 2);
        }
    }

    private Bounds CalculatePrefabBounds(GameObject go) // Retourne les bounds natifs du prefab à scale 1
    {
        Vector3 savedScale      = go.transform.localScale;
        go.transform.localScale = Vector3.one;
        SpriteRenderer sr       = go.GetComponent<SpriteRenderer>();
        Bounds b                = sr != null ? sr.bounds : new Bounds(Vector3.zero, Vector3.zero);
        go.transform.localScale = savedScale;
        return b;
    }

    private RoomType GetRoomTypeFromLabel(string label) // Convertit le label en RoomType
    {
        if (System.Enum.TryParse(label, out RoomType result)) return result;
        return RoomType.Toilettes;
    }

    private void SpawnRoomProps(Vector2 pos, Vector2 size, RoomType type, Transform parent) // Génère les props intérieurs selon le type
    {
        GameObject propsRoot = new GameObject("Props_" + type);
        propsRoot.transform.SetParent(parent, false);
        Transform propsT = propsRoot.transform;

        switch (type) // Redirige vers le spawner de props adapté
        {
            case RoomType.Toilettes:    SpawnToilettesProps(pos, size, propsT);    break;
            case RoomType.SalleReunion: SpawnSalleReunionProps(pos, size, propsT); break;
            case RoomType.Vestiaire:    SpawnVestiaireProps(pos, size, propsT);    break;
            case RoomType.SalleRepos:   SpawnSalleReposProps(pos, size, propsT);   break;
        }
    }

    private void SpawnToilettesProps(Vector2 pos, Vector2 size, Transform parent) // Cabines WC et lavabos
    {
        Color cabineColor = new Color(0.85f, 0.85f, 0.85f);
        Color wcColor     = new Color(0.95f, 0.95f, 1.0f);
        Color lavaboColor = new Color(0.75f, 0.88f, 1.0f);

        int cabineCount = Random.Range(2, 4);
        float startX    = pos.x - (cabineCount - 1) * 1.1f * 0.5f;

        for (int i = 0; i < cabineCount; i++) // Instancie cabines et cuvettes
        {
            float cx = startX + i * 1.1f;
            float cy = pos.y + size.y * 0.25f;
            SpawnProp(parent, "Cabine_" + i, _propLibrary?.cabinePrefab, cabineColor, new Vector2(1.0f, 1.4f), new Vector2(cx, cy), 1, isSearchable: true, searchLabel: "Cabine WC");
            SpawnProp(parent, "WC_" + i,     _propLibrary?.wcPrefab,     wcColor,     new Vector2(0.45f, 0.45f), new Vector2(cx, cy - 0.1f), 2);
        }

        int lavaboCount = Random.Range(1, 3);
        for (int i = 0; i < lavaboCount; i++) // Instancie les lavabos sur le mur opposé
        {
            float lx = pos.x + (i - lavaboCount * 0.5f + 0.5f) * 1.2f;
            SpawnProp(parent, "Lavabo_" + i, _propLibrary?.lavaboPrefab, lavaboColor, new Vector2(0.7f, 0.4f), new Vector2(lx, pos.y - size.y * 0.3f), 2, isSearchable: true, searchLabel: "Lavabo");
        }
    }

    private void SpawnSalleReunionProps(Vector2 pos, Vector2 size, Transform parent) // Table centrale fouillable
    {
        SpawnProp(parent, "Table", _propLibrary?.tableReunionPrefab, new Color(0.45f, 0.3f, 0.15f), new Vector2(3.0f, 1.2f), pos, 1, isSearchable: true, searchLabel: "Table de réunion");
    }

    private void SpawnVestiaireProps(Vector2 pos, Vector2 size, Transform parent) // Rangée de casiers et banc
    {
        Color casierColor = new Color(0.3f, 0.35f, 0.5f);
        Color bancColor   = new Color(0.5f, 0.35f, 0.2f);

        int casierCount = Random.Range(3, 6);
        float startX    = pos.x - (casierCount - 1) * 0.7f * 0.5f;

        for (int i = 0; i < casierCount; i++) // Instancie les casiers fouillables
        {
            float cx = startX + i * 0.7f;
            SpawnProp(parent, "Casier_" + i, _propLibrary?.casierPrefab, casierColor, new Vector2(0.6f, 1.4f), new Vector2(cx, pos.y + size.y * 0.2f), 1, isSearchable: true, searchLabel: "Casier");
        }

        SpawnProp(parent, "Banc", _propLibrary?.bancPrefab, bancColor, new Vector2(casierCount * 0.7f * 0.8f, 0.3f), new Vector2(pos.x, pos.y - 0.2f), 1);
    }

    private void SpawnSalleReposProps(Vector2 pos, Vector2 size, Transform parent) // Canapé, table basse, machine à café
    {
        SpawnProp(parent, "Canape",     _propLibrary?.canapePrefab,      new Color(0.55f, 0.35f, 0.55f), new Vector2(2.0f, 0.7f), new Vector2(pos.x, pos.y + 0.5f), 1);
        SpawnProp(parent, "TableBasse", _propLibrary?.tableBassePrefab,   new Color(0.35f, 0.25f, 0.15f), new Vector2(1.0f, 0.5f), new Vector2(pos.x, pos.y - 0.3f), 1, isSearchable: true, searchLabel: "Table basse");

        float machineX = pos.x + Random.Range(-size.x * 0.3f, size.x * 0.3f);
        SpawnProp(parent, "MachineCafe", _propLibrary?.machineCafePrefab, new Color(0.25f, 0.25f, 0.3f), new Vector2(0.5f, 0.6f), new Vector2(machineX, pos.y - size.y * 0.3f), 2, isSearchable: true, searchLabel: "Machine à café");

        float plantX = pos.x + (Random.value > 0.5f ? size.x * 0.35f : -size.x * 0.35f);
        SpawnProp(parent, "Plante",      _propLibrary?.plantePrefab,      new Color(0.2f,  0.55f, 0.2f),  new Vector2(0.4f, 0.6f), new Vector2(plantX, pos.y + size.y * 0.3f), 2);
    }

    private void SpawnProp(Transform parent, string goName, GameObject prefab, Color fallbackColor, Vector2 size, Vector2 worldPos, int sortingOrder, bool isSearchable = false, string searchLabel = "") // Instancie un prop depuis prefab ou couleur de fallback
    {
        GameObject prop;

        if (prefab != null)
        {
            prop      = Instantiate(prefab, new Vector3(worldPos.x, worldPos.y, 0f), Quaternion.identity, parent);
            prop.name = "Prop_" + goName;

            SpriteRenderer sr = prop.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = sortingOrder;

            if (sr != null && sr.sprite != null) // Adapte le scale pour couvrir la taille demandée
            {
                float nativeW = sr.sprite.rect.width  / sr.sprite.pixelsPerUnit;
                float nativeH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                prop.transform.localScale = new Vector3(size.x / nativeW, size.y / nativeH, 1f);
            }
            else // Fallback couleur si sprite absent
            {
                if (sr == null) sr = prop.AddComponent<SpriteRenderer>();
                sr.sprite         = CreateColorSprite(fallbackColor);
                sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
                sr.sortingOrder   = sortingOrder;
                prop.transform.localScale = new Vector3(size.x, size.y, 1f);
            }
        }
        else // Crée un GO simple avec sprite couleur
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

        BoxCollider2D col = prop.GetComponent<BoxCollider2D>() ?? prop.AddComponent<BoxCollider2D>();
        Vector3 s = prop.transform.localScale;
        col.size      = new Vector2(s.x != 0 ? size.x / s.x : 1f, s.y != 0 ? size.y / s.y : 1f); // Taille locale compensée
        col.isTrigger = false;

        if (isSearchable && _searchUIManager != null) // Configure le SearchableObject si fouillable
        {
            SearchableObject searchable = prop.GetComponent<SearchableObject>() ?? prop.AddComponent<SearchableObject>();
            searchable.SetLabel(string.IsNullOrEmpty(searchLabel) ? goName : searchLabel);

            LootDropper dropper = prop.GetComponent<LootDropper>() ?? prop.AddComponent<LootDropper>();
            dropper.SetInventoryManager(_inventoryManager);
            dropper.SetLootTable(_defaultLootTable);
            searchable.SetLootDropper(dropper);

            searchable.OnPlayerEnterRange.AddListener(_searchUIManager.OnPlayerEnterRange);
            searchable.OnPlayerExitRange.AddListener(_searchUIManager.OnPlayerExitRange);
            searchable.SetInfested(_infestedChance, _enemyHiddenPrefab);
        }
    }

    private void BuildRoomWalls(Vector2 pos, Vector2 size, WallSide attachedWall, Transform parent, string roomName) // Génère les quatre murs avec ouverture vers l'Open Space
    {
        WallSide doorwaySide = GetOppositeWall(attachedWall);

        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;
        float halfT = WallThickness * 0.5f;
        float halfD = _doorwayWidth * 0.5f;

        GameObject wallsRoot = new GameObject(roomName + "_Walls");
        wallsRoot.transform.SetParent(parent, false);
        Transform walls = wallsRoot.transform;

        if (doorwaySide == WallSide.Top) // Mur haut avec passage centré
        {
            float segW = halfW - halfD;
            SpawnWall(walls, "Wall_Top_L", new Vector2(segW, WallThickness), new Vector2(pos.x - halfD - segW * 0.5f, pos.y + halfH + halfT));
            SpawnWall(walls, "Wall_Top_R", new Vector2(segW, WallThickness), new Vector2(pos.x + halfD + segW * 0.5f, pos.y + halfH + halfT));
        }
        else SpawnWall(walls, "Wall_Top", new Vector2(size.x + WallThickness, WallThickness), new Vector2(pos.x, pos.y + halfH + halfT));

        if (doorwaySide == WallSide.Bottom) // Mur bas avec passage centré
        {
            float segW = halfW - halfD;
            SpawnWall(walls, "Wall_Bottom_L", new Vector2(segW, WallThickness), new Vector2(pos.x - halfD - segW * 0.5f, pos.y - halfH - halfT));
            SpawnWall(walls, "Wall_Bottom_R", new Vector2(segW, WallThickness), new Vector2(pos.x + halfD + segW * 0.5f, pos.y - halfH - halfT));
        }
        else SpawnWall(walls, "Wall_Bottom", new Vector2(size.x + WallThickness, WallThickness), new Vector2(pos.x, pos.y - halfH - halfT));

        if (doorwaySide == WallSide.Right) // Mur droit avec passage centré
        {
            float segH = halfH - halfD;
            SpawnWall(walls, "Wall_Right_T", new Vector2(WallThickness, segH), new Vector2(pos.x + halfW + halfT, pos.y + halfD + segH * 0.5f));
            SpawnWall(walls, "Wall_Right_B", new Vector2(WallThickness, segH), new Vector2(pos.x + halfW + halfT, pos.y - halfD - segH * 0.5f));
        }
        else SpawnWall(walls, "Wall_Right", new Vector2(WallThickness, size.y), new Vector2(pos.x + halfW + halfT, pos.y));

        if (doorwaySide == WallSide.Left) // Mur gauche avec passage centré
        {
            float segH = halfH - halfD;
            SpawnWall(walls, "Wall_Left_T", new Vector2(WallThickness, segH), new Vector2(pos.x - halfW - halfT, pos.y + halfD + segH * 0.5f));
            SpawnWall(walls, "Wall_Left_B", new Vector2(WallThickness, segH), new Vector2(pos.x - halfW - halfT, pos.y - halfD - segH * 0.5f));
        }
        else SpawnWall(walls, "Wall_Left", new Vector2(WallThickness, size.y), new Vector2(pos.x - halfW - halfT, pos.y));
    }

    private void SpawnWall(Transform parent, string goName, Vector2 size, Vector2 worldPos) // Instancie un mur avec sprite et collider
    {
        GameObject wall        = new GameObject(goName);
        wall.transform.SetParent(parent, false);
        wall.transform.position   = new Vector3(worldPos.x, worldPos.y, 0f);
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr  = wall.AddComponent<SpriteRenderer>();
        sr.sprite          = CreateColorSprite(_wallColor);
        sr.sharedMaterial  = new Material(Shader.Find(URPShaderName));
        sr.sortingOrder    = 0;

        wall.AddComponent<BoxCollider2D>();
    }

    private WallSide GetOppositeWall(WallSide wall) // Retourne le côté opposé du mur
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

    private Vector2 WallSideToDirection(WallSide side) // Convertit un mur en direction normalisée
    {
        return side switch
        {
            WallSide.Top    => Vector2.up,
            WallSide.Bottom => Vector2.down,
            WallSide.Right  => Vector2.right,
            WallSide.Left   => Vector2.left,
            _               => Vector2.down
        };
    }

    private Sprite CreateColorSprite(Color color) // Génère un sprite d'un pixel de la couleur donnée
    {
        Texture2D tex  = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
