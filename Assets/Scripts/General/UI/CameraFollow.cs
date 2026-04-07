using UnityEngine;

// Suit la position du joueur avec une dead zone : la caméra ne bouge que
// si le joueur dépasse un seuil configurable en fraction de l'écran.
public class CameraFollow : MonoBehaviour
{
    // Cible dont la position est suivie par la caméra
    [SerializeField] private Transform _target;

    // Vitesse de rattrapage quand la caméra doit se déplacer
    [SerializeField] private float _smoothSpeed = 5f;

    // Fraction de la demi-taille de l'écran à partir de laquelle la caméra se déclenche
    // (0 = suit toujours, 1 = bord exact de l'écran)
    [SerializeField] [Range(0f, 1f)] private float _deadZoneFraction = 0.3f;

    // Position Z fixe de la caméra orthographique (doit rester à -10)
    private const float CameraDepth = -10f;

    // Caméra utilisée pour convertir les limites en unités monde
    private Camera _camera;

    // Initialise la référence à la caméra et résout la cible si manquante
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
            _camera = Camera.main;

        // Résout automatiquement la cible depuis le tag Player si non assignée
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _target = player.transform;
            else
                Debug.LogWarning("[CameraFollow] Aucun GameObject taggé 'Player' trouvé — assignez _target en Inspector.", this);
        }
    }

    // Suit le joueur en fin de frame pour éviter les décalages avec la physique
    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos  = _target.position;

        // Calcule la demi-taille de la vue en unités monde
        float halfH = _camera != null ? _camera.orthographicSize : 5f;
        float halfW = halfH * (_camera != null ? _camera.aspect : 1.78f);

        // Calcul du seuil en unités monde depuis le centre de la caméra
        float thresholdX = halfW * _deadZoneFraction;
        float thresholdY = halfH * _deadZoneFraction;

        // Offset du joueur par rapport au centre de la caméra
        float offsetX = targetPos.x - currentPos.x;
        float offsetY = targetPos.y - currentPos.y;

        // Calcule la position cible en ne corrigeant que si hors dead zone
        float desiredX = currentPos.x;
        float desiredY = currentPos.y;

        if (Mathf.Abs(offsetX) > thresholdX)
            desiredX = targetPos.x - Mathf.Sign(offsetX) * thresholdX;

        if (Mathf.Abs(offsetY) > thresholdY)
            desiredY = targetPos.y - Mathf.Sign(offsetY) * thresholdY;

        Vector3 desired = new Vector3(desiredX, desiredY, CameraDepth);

        // Interpole vers la position désirée selon la vitesse configurée
        transform.position = Vector3.Lerp(currentPos, desired, _smoothSpeed * Time.deltaTime);
    }
}
