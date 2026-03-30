using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gère les ticks de vague et l'offset de formation des virus
public class WaveManager : MonoBehaviour
{
    // Intervalle en secondes entre chaque tick de vague
    [SerializeField] private float _tickInterval = 1f;

    // Amplitude maximale du balancement horizontal de la formation
    [SerializeField] private float _formationAmplitude = 3f;

    // Vitesse angulaire du balancement horizontal de la formation
    [SerializeField] private float _formationFrequency = 0.5f;

    // Événement déclenché à chaque tick de vague pour les virus
    public event Action OnWaveTick;

    // Offset horizontal courant calculé depuis le début du jeu
    private float _formationOffsetX;

    // Table de correspondance entre virus et position X de base
    private readonly Dictionary<VirusBase, float> _basePositions = new Dictionary<VirusBase, float>();

    // Lance la coroutine de tick dès l'activation du WaveManager
    private void OnEnable()
    {
        // Démarre la boucle périodique d'émission des ticks de vague
        StartCoroutine(WaveTickRoutine());
    }

    // Met à jour l'offset de formation chaque frame via une sinusoïde
    private void Update()
    {
        // Calcule le balancement horizontal selon le temps écoulé
        _formationOffsetX = Mathf.Sin(Time.time * _formationFrequency) * _formationAmplitude;
    }

    // Émet un tick à intervalle régulier pour toute la formation
    private IEnumerator WaveTickRoutine()
    {
        // Répète indéfiniment tant que le WaveManager est actif
        while (true)
        {
            // Attend l'intervalle configuré avant le prochain tick
            yield return new WaitForSeconds(_tickInterval);

            // Notifie tous les virus abonnés du nouveau tick de vague
            OnWaveTick?.Invoke();
        }
    }

    /// <summary>Retourne l'offset horizontal courant de la formation.</summary>
    // Expose l'offset calculé en lecture seule aux virus
    public float GetFormationOffsetX()
    {
        // Renvoie le décalage horizontal courant de la formation
        return _formationOffsetX;
    }

    /// <summary>Enregistre la position X de base d'un virus à son spawn.</summary>
    // Mémorise la position initiale du virus dans la formation
    public void RegisterBasePosition(VirusBase virus, float baseX)
    {
        // Ajoute ou met à jour la position de base dans la table
        _basePositions[virus] = baseX;
    }

    /// <summary>Retourne la position X de base enregistrée pour ce virus.</summary>
    // Expose la position X initiale du virus dans la formation
    public float GetBasePositionX(VirusBase virus)
    {
        // Retourne la position enregistrée ou zéro si absente
        return _basePositions.TryGetValue(virus, out float baseX) ? baseX : 0f;
    }
}
