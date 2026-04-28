using System.Collections;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
    [SerializeField] private float _animationDuration = 0.6f;

    private Vector3 _closedLocalPosition;
    private Vector3 _openLocalPosition;
    private Coroutine _animationCoroutine;

    public void InitializeWorld(Vector3 closedWorldPosition, Vector3 openWorldPosition) // Convertit world vers local et démarre fermée
    {
        _closedLocalPosition = transform.parent != null
            ? transform.parent.InverseTransformPoint(closedWorldPosition)
            : closedWorldPosition;

        _openLocalPosition = transform.parent != null
            ? transform.parent.InverseTransformPoint(openWorldPosition)
            : openWorldPosition;

        transform.localPosition = _closedLocalPosition;
    }

    public void Initialize(Vector3 closedLocalPosition, Vector3 openLocalPosition) // Configure positions locales et démarre fermée
    {
        _closedLocalPosition    = closedLocalPosition;
        _openLocalPosition      = openLocalPosition;
        transform.localPosition = _closedLocalPosition;
    }

    public void Close() // Lance l'animation de fermeture
    {
        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(AnimateTo(_closedLocalPosition));
    }

    public void Open() // Lance l'animation d'ouverture
    {
        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(AnimateTo(_openLocalPosition));
    }

    private IEnumerator AnimateTo(Vector3 target) // Déplace la porte en Lerp vers la cible
    {
        Vector3 start   = transform.localPosition;
        float   elapsed = 0f;

        while (elapsed < _animationDuration) // Interpole position jusqu'à la cible
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _animationDuration));
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
        _animationCoroutine     = null;
    }
}
