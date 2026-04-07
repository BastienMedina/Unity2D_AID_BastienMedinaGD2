using TMPro;
using UnityEngine;

// Affiche le numéro d'étage courant dans le Canvas
[RequireComponent(typeof(TextMeshProUGUI))]
public class FloorLabelUI : MonoBehaviour
{
    // Préfixe affiché avant le numéro d'étage
    [SerializeField] private string _prefix = "ÉTAGE ";

    // Affiche le numéro d'étage actuel au démarrage
    private void Start()
    {
        // Affiche le numéro d'étage actuel
        GetComponent<TextMeshProUGUI>().text =
            _prefix + GameProgress.Instance.CurrentFloor;
    }
}
