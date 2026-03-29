using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Gère uniquement le spawn des vagues, la formation et la difficulté
public class SI_WaveManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Prefabs des trois types de virus pouvant être spawnés
    // -------------------------------------------------------------------------

    // Prefab du virus rapide placé sur les lignes inférieures
    [SerializeField] private GameObject _virusFastPrefab;

    // Prefab du virus lourd placé sur la ligne supérieure de chaque vague
    [SerializeField] private GameObject _virusHeavyPrefab;

    // Prefab du virus tireur placé sur la ligne du milieu de chaque vague
    [SerializeField] private GameObject _virusShooterPrefab;

    // -------------------------------------------------------------------------
    // Paramètres de grille et de difficulté
    // -------------------------------------------------------------------------

    // Nombre de lignes de la grille de départ de la première vague
    [SerializeField] private int _baseRowCount = 3;

    // Nombre de colonnes de la grille de départ de la première vague
    [SerializeField] private int _baseColCount = 6;

    // Intervalle en secondes entre chaque déplacement de la formation
    [SerializeField] private float _formationMoveInterval = 1.0f;

    // Facteur de réduction de l'intervalle de déplacement par vague
    [SerializeField] private float _difficultyScalePerWave = 0.1f;

    // Limite verticale basse déclenchant l'événement d'urgence serveur
    [SerializeField] private float _formationBottomBoundary = -2.5f;

    // Intervalle minimum de déplacement pour éviter une vitesse infinie
    [SerializeField] private float _minMoveInterval = 0.1f;

    // Événement d'urgence déclenché si la formation atteint le serveur
    [SerializeField] private UnityEvent _onFormationReachedServer;

    // Limite maximale de colonnes autorisée quelle que soit la vague
    [SerializeField] private int _maxColCount = 10;

    // Décalage horizontal appliqué à chaque pas de déplacement latéral
    [SerializeField] private float _formationStepX = 0.5f;

    // Décalage vertical appliqué lors du rebond sur les bords latéraux
    [SerializeField] private float _formationStepDown = 0.4f;

    // Limite horizontale gauche avant inversion de direction de la formation
    [SerializeField] private float _formationBoundaryLeft = -4f;

    // Limite horizontale droite avant inversion de direction de la formation
    [SerializeField] private float _formationBoundaryRight = 4f;

    // Espacement horizontal entre chaque virus dans la grille de spawn
    [SerializeField] private float _cellSpacingX = 1.0f;

    // Espacement vertical entre chaque ligne dans la grille de spawn
    [SerializeField] private float _cellSpacingY = 0.8f;

    // Position Y de la première ligne en haut de la grille au spawn
    [SerializeField] private float _spawnOriginY = 2.5f;

    // -------------------------------------------------------------------------
    // Événements publics de progression des vagues
    // -------------------------------------------------------------------------

    // Événement déclenché au début de chaque nouvelle vague
    [SerializeField] private UnityEvent<int> _onWaveStarted;

    // Événement déclenché lorsque tous les virus d'une vague sont détruits
    [SerializeField] private UnityEvent<int> _onWaveCleared;

    // Événement déclenché lorsque toutes les vagues ont été complétées
    [SerializeField] private UnityEvent _onAllWavesComplete;

    // Événement déclenché à chaque spawn de virus pour l'injecteur
    [SerializeField] private UnityEvent<GameObject> _onVirusSpawned;

    /// <summary>Événement public accessible par SI_VirusInjector.</summary>
    // Expose l'événement de spawn de virus en lecture seule
    public UnityEvent<GameObject> OnVirusSpawned => _onVirusSpawned;

    // -------------------------------------------------------------------------
    // État interne de la formation et de la progression
    // -------------------------------------------------------------------------

    // Numéro de la vague courante, commence à 1
    private int _currentWave;

    // Offset de formation courant appliqué à tous les virus actifs
    private Vector2 _formationOffset;

    // Direction horizontale courante de la formation (+1 droite, -1 gauche)
    private float _formationDirection = 1f;

    // Nombre de lignes utilisées lors du spawn de la vague courante
    private int _currentRowCount;

    // Nombre de colonnes utilisées lors du spawn de la vague courante
    private int _currentColCount;

    // Intervalle de déplacement courant, réduit à chaque nouvelle vague
    private float _currentMoveInterval;

    // Ensemble des virus vivants de la vague courante
    private readonly HashSet<VirusBase> _aliveViruses = new HashSet<VirusBase>();

    // Table des positions X de base de chaque virus dans la grille
    private readonly Dictionary<VirusBase, float> _basePositionsX = new Dictionary<VirusBase, float>();

    // Table des positions Y de base de chaque virus dans la grille
    private readonly Dictionary<VirusBase, float> _basePositionsY = new Dictionary<VirusBase, float>();

    // Indique si une vague est actuellement en cours de jeu
    private bool _waveInProgress;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Surveille les gardes d'urgence à chaque frame
    private void Update()
    {
        // Vérifie si la formation a atteint la limite basse de l'écran
        if (_formationOffset.y <= _formationBottomBoundary)
        {
            // Déclenche l'événement d'urgence une seule fois si actif
            if (_waveInProgress)
            {
                // Notifie que la formation a atteint le serveur en urgence
                _onFormationReachedServer?.Invoke();

                // Arrête la vague pour éviter de répéter l'événement
                _waveInProgress = false;
            }
        }

        // Force la clôture si tous les virus sont morts sans flag levé
        if (_waveInProgress && _aliveViruses.Count == 0)
        {
            // Lève le flag manquant pour débloquer la coroutine WaitUntil
            _waveInProgress = false;

            // Déclenche la fin de vague en mode rattrapage d'urgence
            _onWaveCleared?.Invoke(_currentWave);

            // Lance la séquence de transition vers la prochaine vague
            StartCoroutine(BeginNextWaveOrComplete());
        }
    }

    // Initialise les paramètres et démarre la première vague
    private void Start()
    {
        // Confirme que Start() s'exécute bien
        Debug.Log("[SI] WaveManager.Start() exécuté");

        // Vérifie les références aux prefabs de virus
        Debug.Log($"[SI] WaveManager — VirusFast={_virusFastPrefab != null} | VirusHeavy={_virusHeavyPrefab != null} | VirusShooter={_virusShooterPrefab != null}");

        // Confirme le démarrage de la première vague
        Debug.Log($"[SI] WaveManager — démarrage vague 1 | rows={_baseRowCount} cols={_baseColCount}");

        // Vérifie si une coroutine ou InvokeRepeating lance le spawn
        Debug.Log("[SI] WaveManager — vérification déclenchement du spawn");

        // Initialise la vague courante à zéro avant le premier spawn
        _currentWave = 0;

        // Initialise les dimensions de grille aux valeurs de base
        _currentRowCount = _baseRowCount;
        _currentColCount = _baseColCount;

        // Initialise l'intervalle de déplacement à la valeur de base
        _currentMoveInterval = _formationMoveInterval;

        // Réinitialise l'offset de formation à l'origine
        _formationOffset = Vector2.zero;

        // Lance la première vague dès le démarrage du gestionnaire
        StartCoroutine(StartNextWave());
    }

    // -------------------------------------------------------------------------
    // Gestion des vagues
    // -------------------------------------------------------------------------

    // Incrémente les paramètres et lance le spawn de la vague suivante
    private IEnumerator StartNextWave()
    {
        // Incrémente le compteur de vague avant le spawn
        _currentWave++;

        // Augmente le nombre de colonnes sans dépasser le maximum autorisé
        if (_currentWave > 1)
        {
            // Ajoute une colonne supplémentaire à chaque nouvelle vague
            _currentColCount = Mathf.Min(_currentColCount + 1, _maxColCount);

            // Réduit l'intervalle de déplacement selon le facteur de difficulté
            _currentMoveInterval *= (1f - _difficultyScalePerWave);

            // Empêche l'intervalle de tomber sous le minimum configuré
            _currentMoveInterval = Mathf.Max(_currentMoveInterval, _minMoveInterval);

            // Ajoute une ligne VirusFast supplémentaire en bas de la grille
            _currentRowCount++;
        }

        // Réinitialise l'offset de formation au centre pour chaque vague
        _formationOffset = Vector2.zero;

        // Réinitialise la direction de déplacement vers la droite
        _formationDirection = 1f;

        // Vide les tables de données de la vague précédente
        _aliveViruses.Clear();
        _basePositionsX.Clear();
        _basePositionsY.Clear();

        // Marque la vague comme en cours avant de spawner les virus
        _waveInProgress = true;

        // Notifie les abonnés du démarrage de la nouvelle vague
        _onWaveStarted?.Invoke(_currentWave);

        // Confirme le début du spawn de la vague
        Debug.Log($"[SI] SpawnWave() appelé — wave={_currentWave}");

        // Spawn tous les virus dans la grille de la vague courante
        SpawnWaveGrid();
        // Démarre la coroutine de déplacement périodique de la formation
        StartCoroutine(FormationMoveRoutine());

        // Attend que tous les virus de la vague soient détruits
        yield return new WaitUntil(() => !_waveInProgress);
    }

    // Instancie tous les virus de la vague dans leur grille initiale
    private void SpawnWaveGrid()
    {
        // Calcule la largeur totale de la grille pour centrer horizontalement
        float totalWidth = (_currentColCount - 1) * _cellSpacingX;

        // Calcule la position X de départ pour centrer la grille à l'écran
        float startX = -totalWidth * 0.5f;

        // Parcourt chaque ligne de la grille pour spawner les virus
        for (int row = 0; row < _currentRowCount; row++)
        {
            // Calcule la position Y de la ligne courante depuis l'origine
            float posY = _spawnOriginY - row * _cellSpacingY;

            // Détermine le prefab à utiliser selon le numéro de ligne
            GameObject prefabForRow = GetPrefabForRow(row);

            // Ignore la ligne si aucun prefab valide n'est disponible
            if (prefabForRow == null)
            {
                continue;
            }

            // Parcourt chaque colonne de la ligne pour spawner les virus
            for (int col = 0; col < _currentColCount; col++)
            {
                // Calcule la position X du virus courant dans la grille
                float posX = startX + col * _cellSpacingX;

                // Construit la position monde du virus à instancier
                Vector2 spawnPos = new Vector2(posX, posY);

                // Affiche chaque virus instancié avec sa position
                Debug.Log($"[SI] Spawn virus row={row} col={col} pos={spawnPos}");

                // Instancie le virus à la position calculée sans rotation
                GameObject virusObject = Instantiate(prefabForRow, spawnPos, Quaternion.identity);

                // Notifie l'injecteur du nouveau virus pour câbler ses références
                _onVirusSpawned?.Invoke(virusObject);

                // Récupère le composant VirusBase sur le virus instancié
                VirusBase virus = virusObject.GetComponent<VirusBase>();

                // Enregistre le virus et sa position de base si valide
                if (virus != null)
                {
                    // Ajoute le virus à l'ensemble des virus vivants
                    _aliveViruses.Add(virus);

                    // Mémorise la position X de base du virus dans la grille
                    _basePositionsX[virus] = posX;

                    // Mémorise la position Y de base du virus dans la grille
                    _basePositionsY[virus] = posY;

                    // S'abonne à la mort du virus pour mettre à jour le compteur
                    virus.OnDeathEvent += () => OnVirusDied(virus);
                }
            }
        }
    }

    // Retourne le prefab correspondant au rôle de la ligne donnée
    private GameObject GetPrefabForRow(int row)
    {
        // Ligne 0 : virus lourd en haut de la formation
        if (row == 0)
        {
            // Retourne le prefab VirusHeavy pour la ligne supérieure
            return _virusHeavyPrefab;
        }

        // Ligne 1 : virus tireur au milieu de la formation
        if (row == 1)
        {
            // Retourne le prefab VirusShooter pour la ligne intermédiaire
            return _virusShooterPrefab;
        }

        // Lignes 2 et suivantes : virus rapide en bas de la formation
        return _virusFastPrefab;
    }

    // Retire le virus mort et vérifie si la vague est terminée
    private void OnVirusDied(VirusBase virus)
    {
        // Retire le virus détruit de l'ensemble des vivants
        _aliveViruses.Remove(virus);

        // Retire les entrées de position associées au virus mort
        _basePositionsX.Remove(virus);
        _basePositionsY.Remove(virus);

        // Vérifie si tous les virus de la vague ont été éliminés
        if (_aliveViruses.Count == 0 && _waveInProgress)
        {
            // Marque la vague comme terminée pour débloquer WaitUntil
            _waveInProgress = false;

            // Notifie les abonnés de la fin de la vague courante
            _onWaveCleared?.Invoke(_currentWave);

            // Lance la vague suivante ou signale la fin du jeu
            StartCoroutine(BeginNextWaveOrComplete());
        }
    }

    // Démarre la vague suivante ou notifie la fin de toutes les vagues
    private IEnumerator BeginNextWaveOrComplete()
    {
        // Attend une frame pour laisser les événements se propager
        yield return null;

        // Lance directement la prochaine vague sans condition de fin
        StartCoroutine(StartNextWave());
    }

    // -------------------------------------------------------------------------
    // Déplacement de la formation
    // -------------------------------------------------------------------------

    // Déplace la formation latéralement à intervalle régulier
    private IEnumerator FormationMoveRoutine()
    {
        // Répète le déplacement tant que la vague est en cours
        while (_waveInProgress)
        {
            // Attend l'intervalle courant avant le prochain déplacement
            yield return new WaitForSeconds(_currentMoveInterval);

            // Abandonne si la vague s'est terminée pendant l'attente
            if (!_waveInProgress)
            {
                break;
            }

            // Calcule la nouvelle position X candidate après le pas
            float nextOffsetX = _formationOffset.x + _formationDirection * _formationStepX;

            // Vérifie si la formation dépasse la limite droite de l'écran
            bool hitsRight = _formationDirection > 0f && nextOffsetX >= _formationBoundaryRight;

            // Vérifie si la formation dépasse la limite gauche de l'écran
            bool hitsLeft = _formationDirection < 0f && nextOffsetX <= _formationBoundaryLeft;

            // Traite le rebond si une limite latérale est franchie
            if (hitsRight || hitsLeft)
            {
                // Inverse la direction horizontale de la formation
                _formationDirection *= -1f;

                // Descend la formation d'un pas vertical lors du rebond
                _formationOffset.y -= _formationStepDown;
            }
            else
            {
                // Applique le pas horizontal dans la direction courante
                _formationOffset.x = nextOffsetX;
            }
        }
    }

    // -------------------------------------------------------------------------
    // API publique — compatible avec les scripts virus existants
    // -------------------------------------------------------------------------

    /// <summary>Retourne l'offset horizontal courant de la formation.</summary>
    // Expose la composante X de l'offset aux virus de la formation
    public float GetFormationOffsetX()
    {
        // Renvoie uniquement la composante horizontale de l'offset courant
        return _formationOffset.x;
    }

    /// <summary>Retourne l'offset complet de la formation en Vector2.</summary>
    // Expose l'offset complet pour les systèmes qui en ont besoin
    public Vector2 GetFormationOffset()
    {
        // Renvoie l'offset de formation complet avec ses deux composantes
        return _formationOffset;
    }

    /// <summary>Retourne la position X de base enregistrée pour ce virus.</summary>
    // Expose la position X initiale du virus dans la grille de spawn
    public float GetBasePositionX(VirusBase virus)
    {
        // Retourne la position enregistrée ou zéro si le virus est absent
        return _basePositionsX.TryGetValue(virus, out float baseX) ? baseX : 0f;
    }

    /// <summary>Retourne la position Y de base enregistrée pour ce virus.</summary>
    // Expose la position Y initiale du virus dans la grille de spawn
    public float GetBasePositionY(VirusBase virus)
    {
        // Retourne la position enregistrée ou zéro si le virus est absent
        return _basePositionsY.TryGetValue(virus, out float baseY) ? baseY : 0f;
    }

    /// <summary>Retourne le numéro de la vague actuellement en cours.</summary>
    // Expose le compteur de vague en lecture seule aux autres systèmes
    public int GetCurrentWave()
    {
        // Renvoie la valeur du compteur interne de vague courante
        return _currentWave;
    }

    // -------------------------------------------------------------------------
    // Accesseurs diagnostiques — lecture seule, cachés de l'inspecteur
    // -------------------------------------------------------------------------

    /// <summary>Accessor diagnostique — prefab VirusFast.</summary>
    // Expose le prefab VirusFast pour les scripts de diagnostic
    [HideInInspector] public GameObject VirusFastPrefab => _virusFastPrefab;

    /// <summary>Accessor diagnostique — prefab VirusHeavy.</summary>
    // Expose le prefab VirusHeavy pour les scripts de diagnostic
    [HideInInspector] public GameObject VirusHeavyPrefab => _virusHeavyPrefab;

    /// <summary>Accessor diagnostique — prefab VirusShooter.</summary>
    // Expose le prefab VirusShooter pour les scripts de diagnostic
    [HideInInspector] public GameObject VirusShooterPrefab => _virusShooterPrefab;
}
