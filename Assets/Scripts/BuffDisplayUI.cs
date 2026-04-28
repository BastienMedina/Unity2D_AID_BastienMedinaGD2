using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffDisplayUI : MonoBehaviour
{
    [SerializeField] private PlayerStatsManager _statsManager;
    [SerializeField] private GameObject _buffSlotPrefab;
    [SerializeField] private Transform _buffContainer;

    private Dictionary<ActiveBuff, GameObject> _buffSlots = new Dictionary<ActiveBuff, GameObject>();

    private void OnEnable() // Abonne l'UI aux événements de buff
    {
        RefreshContainerVisibility();
        if (_statsManager == null) return;
        _statsManager.OnBuffAdded.AddListener(AddBuffSlot);
        _statsManager.OnBuffRemoved.AddListener(RemoveBuffSlot);
    }

    private void OnDisable() // Désabonne l'UI des événements de buff
    {
        if (_statsManager == null) return;
        _statsManager.OnBuffAdded.RemoveListener(AddBuffSlot);
        _statsManager.OnBuffRemoved.RemoveListener(RemoveBuffSlot);
    }

    private void Update() // Met à jour les timers chaque frame
    {
        foreach (var pair in _buffSlots) // Parcourt chaque buff actif
        {
            ActiveBuff buff = pair.Key;
            GameObject slot = pair.Value;

            TextMeshProUGUI timerText = slot.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
            if (timerText != null)
                timerText.text = Mathf.Ceil(buff.RemainingTime).ToString() + "s";

            Image fillBar = slot.transform.Find("FillBar")?.GetComponent<Image>();
            if (fillBar != null)
                fillBar.fillAmount = buff.RemainingTime / buff.TotalDuration;
        }
    }

    private void AddBuffSlot(ActiveBuff buff) // Crée un slot UI pour un buff temporaire
    {
        if (!buff.HasDuration) return;

        GameObject slot = Instantiate(_buffSlotPrefab, _buffContainer);

        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null && buff.Icon != null)
            icon.sprite = buff.Icon;

        TextMeshProUGUI label = slot.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (label != null)
            label.text = buff.Type.ToString();

        _buffSlots[buff] = slot;
        RefreshContainerVisibility();
    }

    private void RemoveBuffSlot(ActiveBuff buff) // Supprime le slot UI d'un buff expiré
    {
        if (!_buffSlots.ContainsKey(buff)) return;
        Destroy(_buffSlots[buff]);
        _buffSlots.Remove(buff);
        RefreshContainerVisibility();
    }

    private void RefreshContainerVisibility() // Affiche ou masque le conteneur selon les buffs
    {
        if (_buffContainer == null) return;
        _buffContainer.gameObject.SetActive(_buffSlots.Count > 0);
    }
}
