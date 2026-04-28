using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _handle;
    [SerializeField] private float _maxRadius = 60f;
    [SerializeField] private float _deadZone = 0.1f;

    private Vector2 _currentDirection = Vector2.zero;
    private bool _isActive = false;

    public void OnPointerDown(PointerEventData eventData) // Active le suivi et calcule position initiale
    {
        _isActive = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) // Met à jour le handle et la direction courante
    {
        if (!_isActive) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        Vector2 clamped          = Vector2.ClampMagnitude(localPoint, _maxRadius);
        _handle.anchoredPosition = clamped;

        _currentDirection = clamped.magnitude > _deadZone // Zéro si dans la zone morte
            ? clamped / _maxRadius
            : Vector2.zero;
    }

    public void OnPointerUp(PointerEventData eventData) // Réinitialise handle et direction au relâchement
    {
        _isActive                = false;
        _handle.anchoredPosition = Vector2.zero;
        _currentDirection        = Vector2.zero;
    }

    public Vector2 GetDirection() => _currentDirection; // Expose la direction normalisée courante
}
