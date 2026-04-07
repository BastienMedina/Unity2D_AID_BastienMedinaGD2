using UnityEngine;
using UnityEngine.UI;

// Adapte l'entrée souris/bouton pour déclencher les attaques du joueur
public class EditorInputAdapter : MonoBehaviour
{
    // Référence au héros pour déclencher les attaques via souris
    [SerializeField] private HeroDonkeyKong _hero;

    // Référence au script de déplacement du joueur
    [SerializeField] private PlayerMovement _playerMovement;

    // Bouton UI ATK relié pour déclencher l'attaque depuis l'interface
    [SerializeField] private Button _attackButton;

    // Noms possibles du bouton ATK dans la hiérarchie UI (ordre de priorité)
    private static readonly string[] AttackButtonNames =
        { "Btn_Attack", "AttackButton", "Button_Attack", "ATK" };

    // Résout toutes les références manquantes après l'init de tous les Awake
    private void Start()
    {
        if (_hero == null)
            _hero = FindFirstObjectByType<HeroDonkeyKong>();

        if (_playerMovement == null)
            _playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (_attackButton == null)
            _attackButton = FindAttackButton();

        // Abonne ici car OnEnable s'exécute avant Start (bouton peut être null à ce moment)
        if (_attackButton != null)
            _attackButton.onClick.AddListener(TriggerAttack);
        else
            Debug.LogWarning(
                "[EditorInputAdapter] Bouton ATK introuvable. " +
                "Vérifiez que son nom correspond à : Btn_Attack, AttackButton, Button_Attack ou ATK.", this);
    }

    private void Awake() { }

    // OnEnable abonne le bouton uniquement s'il était déjà assigné en Inspector avant Start
    private void OnEnable()
    {
        if (_attackButton != null)
            _attackButton.onClick.AddListener(TriggerAttack);
    }

    // Désabonne le bouton ATK pour éviter les fuites mémoire
    private void OnDisable()
    {
        if (_attackButton != null)
            _attackButton.onClick.RemoveListener(TriggerAttack);
    }

    /// <summary>Déclenche une attaque dans la direction du déplacement — appelé par le bouton ATK de l'UI ou depuis l'extérieur.</summary>
    public void AttackFromUI() => TriggerAttack();

    // Lance l'attaque dans la direction de visée courante du joueur
    private void TriggerAttack()
    {
        if (_hero == null)
        {
            Debug.LogWarning("[EditorInputAdapter] HeroDonkeyKong non assigné.", this);
            return;
        }

        // Récupère la direction de visée depuis le déplacement
        Vector2 attackDirection = _playerMovement != null
            ? _playerMovement.GetFacingDirection()
            : Vector2.up;

        _hero.Attack(attackDirection);
    }

    // Cherche le bouton ATK par nom parmi tous les boutons (actifs et inactifs)
    private static Button FindAttackButton()
    {
        Button[] all = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in all)
        {
            foreach (string candidateName in AttackButtonNames)
            {
                if (btn.gameObject.name == candidateName)
                    return btn;
            }
        }
        return null;
    }
}
