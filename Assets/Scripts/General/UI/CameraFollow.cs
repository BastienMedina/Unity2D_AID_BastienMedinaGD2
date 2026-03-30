using UnityEngine;

// Suit la position du joueur avec un lissage configurable (caméra 2D vue du dessus)
public class CameraFollow : MonoBehaviour
{
    // Cible dont la position est suivie par la caméra
    [SerializeField] private Transform _target;

    // Vitesse de lissage du suivi (interpolation linéaire par frame)
    [SerializeField] private float _smoothSpeed = 5f;

    // Position Z fixe de la caméra orthographique (doit rester à -10)
    private const float CameraDepth = -10f;

    // Interpole la position de la caméra vers la cible à chaque fin de frame
    private void LateUpdate()
    {
        // Ignore la logique si aucune cible n'est assignée dans l'inspecteur
        if (_target == null)
        {
            return;
        }

        // Calcule la position cible en conservant la profondeur fixe de la caméra
        Vector3 targetPosition = new Vector3(
            _target.position.x,
            _target.position.y,
            CameraDepth
        );

        // Interpolation linéaire vers la cible selon la vitesse et le temps écoulé
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            _smoothSpeed * Time.deltaTime
        );
    }
}
