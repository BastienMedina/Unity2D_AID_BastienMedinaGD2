using UnityEngine;
using UnityEngine.EventSystems;

// Gère l'entrée joystick tactile et souris via PointerEventData
public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // Fond circulaire extérieur du joystick virtuel
    [SerializeField] private RectTransform _background;

    // Poignée mobile intérieure représentant la position du doigt
    [SerializeField] private RectTransform _handle;

    // Rayon maximum de déplacement du handle en pixels UI
    [SerializeField] private float _maxRadius = 60f;

    // Seuil minimal pour ignorer les micro-déplacements du handle
    [SerializeField] private float _deadZone = 0.1f;

    // Direction normalisée actuelle calculée depuis le handle
    private Vector2 _currentDirection = Vector2.zero;

    // Indique si un contact ou clic est actuellement actif
    private bool _isActive = false;

    // Enregistre le début du contact et active le suivi
    public void OnPointerDown(PointerEventData eventData)
    {
        // Confirme que OnPointerDown est bien déclenché
        Debug.Log($"[JOY] OnPointerDown() — position={eventData.position} camera={eventData.pressEventCamera}");

        // Active le suivi dès le premier contact ou clic
        _isActive = true;

        // Calcule la position du handle immédiatement au contact
        OnDrag(eventData);
    }

    // Met à jour la position du handle pendant le glissement
    public void OnDrag(PointerEventData eventData)
    {
        // Ignore si aucun contact actif n'est en cours
        if (!_isActive)
        {
            return;
        }

        // Confirme que OnDrag reçoit les événements souris
        Debug.Log($"[JOY] OnDrag() — rawPosition={eventData.position}");

        // Convertit la position écran en coordonnées locales du fond
        // pressEventCamera est null pour ScreenSpaceOverlay — correct
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // Affiche la position locale calculée dans le fond
        Debug.Log($"[JOY] localPoint={localPoint} | background null={_background == null}");

        // Limite le déplacement au rayon maximal autorisé
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, _maxRadius);

        // Déplace le handle vers la position locale calculée
        _handle.anchoredPosition = clamped;

        // Calcule la direction normalisée uniquement si hors de la zone morte
        if (clamped.magnitude > _deadZone)
        {
            _currentDirection = clamped / _maxRadius;
        }
        // Remet la direction à zéro si dans la zone morte
        else
        {
            _currentDirection = Vector2.zero;
        }

        // Affiche la direction normalisée finale du joystick
        Debug.Log($"[JOY] clamped={clamped} | currentDirection={_currentDirection}");
    }

    // Réinitialise le joystick au relâchement du contact
    public void OnPointerUp(PointerEventData eventData)
    {
        // Confirme le relâchement du joystick
        Debug.Log("[JOY] OnPointerUp() — joystick released, direction reset to zero");

        // Désactive le suivi du contact ou du clic
        _isActive = false;

        // Replace le handle au centre du fond du joystick
        _handle.anchoredPosition = Vector2.zero;

        // Remet la direction normalisée à zéro
        _currentDirection = Vector2.zero;
    }

    /// <summary>Retourne la direction normalisée actuelle du joystick, Vector2.zero si inactif.</summary>
    // Expose la direction sans permettre de la modifier
    public Vector2 GetDirection()
    {
        return _currentDirection;
    }
}
