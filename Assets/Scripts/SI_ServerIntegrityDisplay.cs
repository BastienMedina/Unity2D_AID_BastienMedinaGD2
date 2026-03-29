using System.Collections;
using UnityEngine;

// Affiche l'intégrité restante du serveur défendu
public class SI_ServerIntegrityDisplay : MonoBehaviour
{
    // Référence à la santé du serveur pour le polling
    [SerializeField] private SI_ServerHealth _serverHealth;

    // Nombre d'indicateurs d'intégrité affichés au-dessus du serveur
    [SerializeField] private int _indicatorCount = 5;

    // Espacement horizontal entre les indicateurs d'intégrité
    [SerializeField] private float _indicatorSpacingX = 0.4f;

    // Position Y locale des indicateurs au-dessus du serveur
    [SerializeField] private float _indicatorLocalY = 0.4f;

    // Échelle locale des indicateurs circulaires verts
    [SerializeField] private Vector3 _indicatorScale = new Vector3(0.2f, 0.2f, 1f);

    // Couleur verte des indicateurs d'intégrité actifs
    [SerializeField] private Color _activeColor = new Color(0.2f, 1f, 0.4f, 1f);

    // Couleur rouge affichée lors du flash de destruction
    [SerializeField] private Color _destroyedColor = new Color(1f, 0.1f, 0.1f, 1f);

    // Durée totale du flash rouge à la destruction du serveur
    [SerializeField] private float _flashDuration = 0.8f;

    // Nombre de clignotements lors du flash de destruction
    [SerializeField] private int _flashCount = 4;

    // Nom du shader URP 2D Sprite-Unlit-Default
    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Tableau des SpriteRenderers des indicateurs instanciés
    private SpriteRenderer[] _indicators;

    // Dernière valeur d'intégrité connue pour détecter les changements
    private int _lastKnownIntegrity = -1;

    // Indique si la destruction a déjà été traitée
    private bool _destroyedHandled;

    // Initialise les indicateurs et mémorise l'intégrité initiale
    private void Awake()
    {
        // Vérifie que la référence au serveur est bien assignée
        if (_serverHealth == null)
        {
            Debug.LogError("[SI_ServerIntegrityDisplay] _serverHealth non assigné.");
            return;
        }

        // Crée les indicateurs d'intégrité au-dessus du serveur
        BuildIndicators();
    }

    // Démarre le polling après que SI_ServerHealth a initialisé sa santé
    private void Start()
    {
        // Ignore si la référence est absente
        if (_serverHealth == null) return;

        // Mémorise la valeur initiale d'intégrité du serveur
        _lastKnownIntegrity = _serverHealth.GetCurrentIntegrity();
    }

    // Détecte les changements d'intégrité via polling chaque frame
    private void Update()
    {
        // Ignore si la référence est absente ou si déjà détruit
        if (_serverHealth == null || _destroyedHandled) return;

        // Lit l'intégrité courante depuis le composant serveur
        int current = _serverHealth.GetCurrentIntegrity();

        // Réagit uniquement si la valeur a changé depuis le dernier poll
        if (current == _lastKnownIntegrity) return;

        // Met à jour la valeur mémorisée avec la nouvelle intégrité
        _lastKnownIntegrity = current;

        // Traite la destruction si l'intégrité atteint zéro
        if (current <= 0)
        {
            // Marque la destruction pour stopper le polling ultérieur
            _destroyedHandled = true;
            HandleDestroyed();
        }
        else
        {
            // Masque l'indicateur correspondant à l'intégrité perdue
            HandleDamaged(current);
        }
    }

    // Construit les indicateurs circulaires au-dessus du serveur
    private void BuildIndicators()
    {
        // Alloue le tableau selon le nombre d'indicateurs configuré
        _indicators = new SpriteRenderer[_indicatorCount];

        // Calcule la position X du premier indicateur (centré)
        float startX = -(_indicatorCount - 1) * _indicatorSpacingX * 0.5f;

        // Itère sur chaque emplacement d'indicateur à créer
        for (int i = 0; i < _indicatorCount; i++)
        {
            // Crée un enfant nommé selon son index (base 1)
            GameObject go = new GameObject($"Integrity_{(i + 1):D2}");
            go.transform.SetParent(transform, false);

            // Positionne l'indicateur uniformément au-dessus du serveur
            go.transform.localPosition = new Vector3(
                startX + i * _indicatorSpacingX,
                _indicatorLocalY,
                0f);

            go.transform.localScale = _indicatorScale;

            // Ajoute le SpriteRenderer avec la couleur active par défaut
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, _activeColor);
            tex.Apply();

            // Crée le sprite à partir de la texture d'un pixel
            sr.sprite = Sprite.Create(tex,
                new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

            // Assigne le shader URP 2D Sprite-Unlit-Default
            sr.sharedMaterial = new Material(Shader.Find(ShaderName));
            sr.sortingOrder   = 3;

            // Stocke le SpriteRenderer dans le tableau pour modification
            _indicators[i] = sr;
        }
    }

    // Cache l'indicateur le plus à droite encore visible
    private void HandleDamaged(int remainingIntegrity)
    {
        // Calcule l'index de l'indicateur à masquer (rightmost first)
        int indexToHide = remainingIntegrity;

        // Vérifie que l'index est dans les bornes du tableau
        if (indexToHide >= 0 && indexToHide < _indicatorCount)
        {
            // Désactive l'indicateur correspondant à l'intégrité perdue
            _indicators[indexToHide].gameObject.SetActive(false);
        }
    }

    // Cache tous les indicateurs et lance le flash rouge de destruction
    private void HandleDestroyed()
    {
        // Masque tous les indicateurs restants immédiatement
        for (int i = 0; i < _indicators.Length; i++)
        {
            // Désactive chaque indicateur encore visible
            _indicators[i].gameObject.SetActive(false);
        }

        // Lance la coroutine de flash rouge sur le SpriteRenderer du serveur
        StartCoroutine(FlashRed());
    }

    // Fait clignoter le serveur en rouge après sa destruction
    private IEnumerator FlashRed()
    {
        // Récupère le SpriteRenderer du serveur parent
        SpriteRenderer serverSr = GetComponent<SpriteRenderer>();

        // Abandonne si le SpriteRenderer est absent sur ce GameObject
        if (serverSr == null) yield break;

        // Mémorise la couleur d'origine pour la restaurer après le flash
        Color originalColor = serverSr.color;

        // Durée d'un demi-cycle de clignotement calculée depuis la config
        float halfInterval = _flashDuration / (_flashCount * 2f);

        // Boucle sur le nombre de clignotements configuré
        for (int i = 0; i < _flashCount; i++)
        {
            // Passe la couleur en rouge pendant le premier demi-cycle
            serverSr.color = _destroyedColor;
            yield return new WaitForSeconds(halfInterval);

            // Rétablit la couleur d'origine pendant le second demi-cycle
            serverSr.color = originalColor;
            yield return new WaitForSeconds(halfInterval);
        }
    }
}
