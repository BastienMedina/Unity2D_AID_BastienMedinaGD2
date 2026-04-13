using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Affiche une barre de coeurs synchronisée avec le LivesManager.
// Instancie un prefab par coeur max et active/désactive les coeurs selon les vies restantes.
public class HeartsDisplay : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références configurables
    // -------------------------------------------------------------------------

    // Gestionnaire de vies à écouter — auto-résolu si non assigné.
    [SerializeField] private LivesManager _livesManager;

    // Prefab d'un coeur UI (doit contenir un composant Image).
    [SerializeField] private GameObject _heartPrefab;

    // Container horizontal où les coeurs sont instanciés.
    [SerializeField] private RectTransform _heartsContainer;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Liste des images de coeurs instanciés, dans l'ordre gauche-droite.
    private readonly List<Image> _heartImages = new List<Image>();

    // Couleur d'un coeur plein (vie restante).
    private static readonly Color ColorFull  = Color.white;

    // Couleur d'un coeur vide (vie perdue) — transparent.
    private static readonly Color ColorEmpty = new Color(1f, 1f, 1f, 0.15f);

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Résout les dépendances et construit la barre de coeurs initiale.
    private void Awake()
    {
        if (_livesManager == null)
            _livesManager = FindFirstObjectByType<LivesManager>();

        if (_livesManager == null)
        {
            Debug.LogError("[HeartsDisplay] LivesManager introuvable dans la scène.", this);
            return;
        }

        BuildHearts(_livesManager.GetCurrentLives());
    }

    // Abonne la mise à jour à l'événement de changement de vies.
    private void OnEnable()
    {
        if (_livesManager == null)
            return;

        _livesManager.OnLivesChanged.AddListener(RefreshHearts);
    }

    // Désabonne pour éviter les fuites mémoire.
    private void OnDisable()
    {
        if (_livesManager == null)
            return;

        _livesManager.OnLivesChanged.RemoveListener(RefreshHearts);
    }

    // -------------------------------------------------------------------------
    // Méthodes privées
    // -------------------------------------------------------------------------

    // Instancie autant de coeurs que le maximum de vies et les affiche tous pleins.
    private void BuildHearts(int maxLives)
    {
        if (_heartPrefab == null)
        {
            Debug.LogError("[HeartsDisplay] _heartPrefab non assigné — assignez un prefab de coeur dans l'Inspector.", this);
            return;
        }

        // Nettoie les anciens coeurs si la méthode est rappelée
        foreach (Image img in _heartImages)
        {
            if (img != null)
                Destroy(img.gameObject);
        }
        _heartImages.Clear();

        // Instancie un coeur par vie maximum
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heartGO = Instantiate(_heartPrefab, _heartsContainer);
            Image img = heartGO.GetComponent<Image>();

            if (img == null)
                img = heartGO.GetComponentInChildren<Image>();

            if (img != null)
            {
                img.color = ColorFull;
                _heartImages.Add(img);
            }
        }
    }

    // Met à jour la couleur de chaque coeur selon les vies restantes.
    private void RefreshHearts(int currentLives)
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] == null)
                continue;

            // Coeur plein si l'index est inférieur aux vies restantes, vide sinon
            _heartImages[i].color = i < currentLives ? ColorFull : ColorEmpty;
        }
    }
}
