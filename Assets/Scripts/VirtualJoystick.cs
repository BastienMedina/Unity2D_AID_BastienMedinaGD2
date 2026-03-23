using UnityEngine;
using UnityEngine.EventSystems;

// Détecte le glissement tactile et expose la direction normalisée
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // Cercle extérieur de fond du joystick virtuel
    [SerializeField] private RectTransform _background;

    // Point intérieur mobile représentant la position du doigt
    [SerializeField] private RectTransform _handle;

    // Rayon maximal de déplacement du handle en pixels
    [SerializeField] private float _maxRadius = 50f;

    // Direction normalisée calculée depuis l'ancre vers le handle
    private Vector2 _direction = Vector2.zero;

    // Indique si un toucher est actuellement actif
    private bool _isTouching = false;

    // Retourne la direction normalisée ou zéro si inactif
    public Vector2 GetDirection()
    {
        // Renvoie zéro si aucun toucher n'est en cours
        if (!_isTouching)
        {
            return Vector2.zero;
        }

        // Renvoie la direction calculée lors du dernier glissement
        return _direction;
    }

    // Enregistre le contact initial et active le suivi du doigt
    public void OnPointerDown(PointerEventData eventData)
    {
        // Marque le joystick comme actif au premier contact
        _isTouching = true;

        // Calcule la direction dès le toucher initial
        UpdateHandlePosition(eventData);
    }

    // Recalcule la direction et déplace le handle lors du glissement
    public void OnDrag(PointerEventData eventData)
    {
        // Met à jour la position du handle selon le doigt glissé
        UpdateHandlePosition(eventData);
    }

    // Réinitialise le handle et la direction au relâchement du doigt
    public void OnPointerUp(PointerEventData eventData)
    {
        // Désactive le suivi du toucher
        _isTouching = false;

        // Remet la direction calculée à zéro
        _direction = Vector2.zero;

        // Replace le handle au centre du fond du joystick
        _handle.anchoredPosition = Vector2.zero;
    }

    // Calcule la position clampée du handle et la direction normalisée
    private void UpdateHandlePosition(PointerEventData eventData)
    {
        // Convertit la position écran du doigt en position locale du fond
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // Clamp le point local dans le rayon maximal autorisé
        Vector2 clampedPoint = Vector2.ClampMagnitude(localPoint, _maxRadius);

        // Applique la position clampée au handle dans l'espace local
        _handle.anchoredPosition = clampedPoint;

        // Calcule la direction normalisée entre -1 et 1
        _direction = clampedPoint / _maxRadius;
    }
}
