using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private float _tickInterval = 1f;
    [SerializeField] private float _formationAmplitude = 3f;
    [SerializeField] private float _formationFrequency = 0.5f;

    public event Action OnWaveTick;

    private float _formationOffsetX;
    private readonly Dictionary<VirusBase, float> _basePositions = new Dictionary<VirusBase, float>();

    private void OnEnable() // Lance la boucle de tick à l'activation
    {
        StartCoroutine(WaveTickRoutine());
    }

    private void Update() // Calcule l'offset sinusoïdal de la formation
    {
        _formationOffsetX = Mathf.Sin(Time.time * _formationFrequency) * _formationAmplitude;
    }

    private IEnumerator WaveTickRoutine() // Émet un tick à intervalle régulier
    {
        while (true)
        {
            yield return new WaitForSeconds(_tickInterval); // Attend l'intervalle configuré
            OnWaveTick?.Invoke();
        }
    }

    public float GetFormationOffsetX() => _formationOffsetX; // Expose l'offset horizontal courant

    public void RegisterBasePosition(VirusBase virus, float baseX) // Enregistre la position X initiale du virus
    {
        _basePositions[virus] = baseX;
    }

    public float GetBasePositionX(VirusBase virus) // Retourne la position X de base enregistrée
    {
        return _basePositions.TryGetValue(virus, out float baseX) ? baseX : 0f;
    }
}
