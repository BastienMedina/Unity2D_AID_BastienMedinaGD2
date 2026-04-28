using System.Collections;
using UnityEngine;

// Anime une porte d'ascenseur en la déplaçant entre sa position fermée et ouverte.
// Chaque porte est un GameObject enfant de l'ascenseur créé par ElevatorDoorController.
public class ElevatorDoor : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Durée de l'animation d'ouverture ou de fermeture en secondes
    [SerializeField] private float _animationDuration = 0.6f;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Position locale quand la porte est complètement fermée
    private Vector3 _closedLocalPosition;

    // Position locale quand la porte est complètement ouverte
    private Vector3 _openLocalPosition;

    // Coroutine d'animation en cours pour éviter les chevauchements
    private Coroutine _animationCoroutine;

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    /// <summary>Configure les positions ouverte et fermée en coordonnées world space.
    /// Appelé par ElevatorDoorController après la création du GO.</summary>
    public void InitializeWorld(Vector3 closedWorldPosition, Vector3 openWorldPosition)
    {
        _closedLocalPosition = transform.parent != null
            ? transform.parent.InverseTransformPoint(closedWorldPosition)
            : closedWorldPosition;

        _openLocalPosition = transform.parent != null
            ? transform.parent.InverseTransformPoint(openWorldPosition)
            : openWorldPosition;

        transform.localPosition = _closedLocalPosition;
    }

    /// <summary>Configure les positions ouverte et fermée en coordonnées locales.
    /// Appelé par ElevatorDoorController après la création du GO.</summary>
    public void Initialize(Vector3 closedLocalPosition, Vector3 openLocalPosition)
    {
        _closedLocalPosition = closedLocalPosition;
        _openLocalPosition   = openLocalPosition;

        // Démarre fermée
        transform.localPosition = _closedLocalPosition;
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Lance l'animation de fermeture de la porte.</summary>
    public void Close()
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(AnimateTo(_closedLocalPosition));
    }

    /// <summary>Lance l'animation d'ouverture de la porte.</summary>
    public void Open()
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(AnimateTo(_openLocalPosition));
    }

    // -------------------------------------------------------------------------
    // Animation
    // -------------------------------------------------------------------------

    // Déplace la porte en douceur vers la position cible via Lerp
    private IEnumerator AnimateTo(Vector3 target)
    {
        Vector3 start   = transform.localPosition;
        float   elapsed = 0f;

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _animationDuration));
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
        _animationCoroutine     = null;
    }
}
