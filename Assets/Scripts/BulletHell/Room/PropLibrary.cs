using UnityEngine;

/// <summary>
/// Centralise tous les prefabs de props d'environnement utilisés par
/// MapVisualBuilder et ProceduralMapGenerator.
/// Assignez un prefab par type de prop — laissez null pour utiliser le fallback couleur.
/// </summary>
[CreateAssetMenu(fileName = "PropLibrary", menuName = "BulletHell/PropLibrary")]
public class PropLibrary : ScriptableObject
{
    [Header("Open Space — Bureaux")]
    public GameObject deskPrefab;

    [Header("Toilettes")]
    public GameObject cabinePrefab;
    public GameObject wcPrefab;
    public GameObject lavaboPrefab;

    [Header("Salle de Réunion")]
    public GameObject tableReunionPrefab;
    public GameObject chaisePrefab;

    [Header("Vestiaire")]
    public GameObject casierPrefab;
    public GameObject bancPrefab;

    [Header("Salle de Repos")]
    public GameObject canapePrefab;
    public GameObject tableBassePrefab;
    public GameObject machineCafePrefab;
    public GameObject plantePrefab;
}
