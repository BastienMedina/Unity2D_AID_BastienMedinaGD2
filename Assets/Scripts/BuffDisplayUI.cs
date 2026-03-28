using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Affiche les buffs actifs avec leurs icônes et timers
public class BuffDisplayUI : MonoBehaviour
{
    // Référence au gestionnaire de stats du joueur
    [SerializeField] private PlayerStatsManager _statsManager;

    // Prefab d'un slot de buff (icône + timer text)
    [SerializeField] private GameObject _buffSlotPrefab;

    // Conteneur horizontal des buffs actifs
    [SerializeField] private Transform _buffContainer;

    // Dictionnaire liant chaque buff à son slot UI
    private Dictionary<ActiveBuff, GameObject> _buffSlots
        = new Dictionary<ActiveBuff, GameObject>();

    // Abonne l'UI aux événements de buff
    private void OnEnable()
    {
        // Écoute les ajouts de buffs
        _statsManager.OnBuffAdded.AddListener(AddBuffSlot);

        // Écoute les expirations de buffs
        _statsManager.OnBuffRemoved.AddListener(RemoveBuffSlot);
    }

    // Désabonne l'UI des événements de buff
    private void OnDisable()
    {
        // Retire les listeners pour éviter les fuites mémoire
        _statsManager.OnBuffAdded.RemoveListener(AddBuffSlot);
        _statsManager.OnBuffRemoved.RemoveListener(RemoveBuffSlot);
    }

    // Met à jour les timers chaque frame
    private void Update()
    {
        // Parcourt chaque buff actif pour mettre à jour son slot
        foreach (var pair in _buffSlots)
        {
            ActiveBuff buff = pair.Key;
            GameObject slot = pair.Value;

            // Cherche le texte du timer dans les enfants du slot
            TextMeshProUGUI timerText =
                slot.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();

            // Met à jour le texte du timer avec les secondes restantes
            if (timerText != null)
                timerText.text = Mathf.Ceil(buff.RemainingTime).ToString() + "s";

            // Met à jour la barre de progression du buff
            Image fillBar =
                slot.transform.Find("FillBar")?.GetComponent<Image>();

            // Calcule le ratio de remplissage de la barre
            if (fillBar != null)
                fillBar.fillAmount = buff.RemainingTime / buff.TotalDuration;
        }
    }

    // Crée un slot UI pour un nouveau buff temporaire
    private void AddBuffSlot(ActiveBuff buff)
    {
        // Ignore les buffs permanents sans durée
        if (!buff.HasDuration) return;

        // Instancie le prefab de slot dans le conteneur
        GameObject slot = Instantiate(_buffSlotPrefab, _buffContainer);

        // Affiche l'icône du buff si disponible
        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();

        // Assigne le sprite uniquement si l'icône existe
        if (icon != null && buff.Icon != null)
            icon.sprite = buff.Icon;

        // Affiche le nom du buff dans le label
        TextMeshProUGUI label =
            slot.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

        // Remplit le texte du label avec le type de buff
        if (label != null)
            label.text = buff.Type.ToString();

        // Enregistre le slot dans le dictionnaire
        _buffSlots[buff] = slot;
    }

    // Supprime le slot UI d'un buff expiré
    private void RemoveBuffSlot(ActiveBuff buff)
    {
        // Vérifie que le buff est bien dans le dictionnaire
        if (!_buffSlots.ContainsKey(buff)) return;

        // Détruit le slot UI associé
        Destroy(_buffSlots[buff]);

        // Retire l'entrée du dictionnaire
        _buffSlots.Remove(buff);
    }
}
