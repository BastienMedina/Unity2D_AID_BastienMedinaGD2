using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField][Range(0f, 1f)] private float _deadZoneFraction = 0.3f;

    private const float CameraDepth = -10f;

    private Camera _camera;

    private void Awake() // Résout la caméra et localise la cible par tag si absente
    {
        _camera = GetComponent<Camera>() ?? Camera.main;

        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _target = player.transform;
        }
    }

    private void LateUpdate() // Suit le joueur avec dead zone, en fin de frame
    {
        if (_target == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos  = _target.position;

        float halfH      = _camera != null ? _camera.orthographicSize       : 5f;
        float halfW      = halfH   * (_camera != null ? _camera.aspect : 1.78f);
        float thresholdX = halfW   * _deadZoneFraction;
        float thresholdY = halfH   * _deadZoneFraction;

        float offsetX = targetPos.x - currentPos.x;
        float offsetY = targetPos.y - currentPos.y;

        float desiredX = currentPos.x;
        float desiredY = currentPos.y;

        if (Mathf.Abs(offsetX) > thresholdX) desiredX = targetPos.x - Mathf.Sign(offsetX) * thresholdX; // Corrige X si hors dead zone
        if (Mathf.Abs(offsetY) > thresholdY) desiredY = targetPos.y - Mathf.Sign(offsetY) * thresholdY; // Corrige Y si hors dead zone

        Vector3 desired    = new Vector3(desiredX, desiredY, CameraDepth);
        transform.position = Vector3.Lerp(currentPos, desired, _smoothSpeed * Time.deltaTime);
    }
}
