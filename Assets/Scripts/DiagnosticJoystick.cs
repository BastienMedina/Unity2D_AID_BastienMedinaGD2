using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Vérifie les prérequis du joystick au démarrage
public class DiagnosticJoystick : MonoBehaviour
{
    // Conteneur du joystick virtuel dans le Canvas
    [SerializeField] private GameObject _joystickContainer;

    // Canvas principal contenant les éléments UI
    [SerializeField] private Canvas _uiCanvas;

    // Exécute tous les contrôles diagnostics une seule fois
    private void Start()
    {
        // Vérifie la présence de l'EventSystem dans la scène
        var es = FindObjectOfType<EventSystem>();
        Debug.Log($"[JOY] EventSystem found = {es != null}");

        // Vérifie la présence du StandaloneInputModule
        var sim = FindObjectOfType<StandaloneInputModule>();
        Debug.Log($"[JOY] StandaloneInputModule found = {sim != null}");

        // Vérifie le GraphicRaycaster sur le Canvas
        var gr = _uiCanvas != null
            ? _uiCanvas.GetComponent<GraphicRaycaster>()
            : null;
        Debug.Log($"[JOY] GraphicRaycaster on UI_Canvas = {gr != null}");

        // Vérifie le composant Image sur le conteneur joystick
        var img = _joystickContainer != null
            ? _joystickContainer.GetComponent<Image>()
            : null;
        Debug.Log($"[JOY] Image on Joystick_Container = {img != null}");

        // Vérifie que l'Image est bien raycastable
        if (img != null)
            Debug.Log($"[JOY] Image.raycastTarget = {img.raycastTarget}");

        // Vérifie que VirtualJoystick est attaché au conteneur
        var vj = _joystickContainer != null
            ? _joystickContainer.GetComponent<VirtualJoystick>()
            : null;
        Debug.Log($"[JOY] VirtualJoystick component found = {vj != null}");
    }
}
