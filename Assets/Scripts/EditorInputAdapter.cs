using UnityEngine;
using UnityEngine.UI;

// Adapte l'entrée souris pour les tests en éditeur Unity
public class EditorInputAdapter : MonoBehaviour
{
    // Référence au héros pour déclencher les attaques via souris
    [SerializeField] private HeroDonkeyKong _hero;

    // Référence au script de déplacement du joueur
    [SerializeField] private PlayerMovement _playerMovement;

    // Bouton UI ATK relié pour déclencher l'attaque depuis l'interface
    [SerializeField] private Button _attackButton;

    // Abonne le bouton ATK au déclenchement de l'attaque
    private void OnEnable()
    {
        // Ignore l'abonnement si aucun bouton n'est assigné dans l'inspecteur
        if (_attackButton == null)
        {
            return;
        }

        // Abonne HandleAttackButtonPressed au clic du bouton ATK
        _attackButton.onClick.AddListener(HandleAttackButtonPressed);
    }

    // Désabonne le bouton ATK pour éviter les fuites mémoire
    private void OnDisable()
    {
        // Retire l'abonnement si le bouton était bien assigné
        if (_attackButton != null)
        {
            _attackButton.onClick.RemoveListener(HandleAttackButtonPressed);
        }
    }

    /// <summary>Déclenche une attaque dans la direction du déplacement — appelé par le bouton ATK de l'UI.</summary>
    // Lance l'attaque depuis le bouton ATK de l'interface mobile
    public void AttackFromUI()
    {
        // Ignore si aucun héros n'est assigné dans l'inspecteur
        if (_hero == null)
        {
            return;
        }

        // Récupère la direction de visée depuis le déplacement
        Vector2 attackDirection = _playerMovement != null
            ? _playerMovement.GetFacingDirection()
            : Vector2.up;

        // Lance l'attaque dans la direction du joueur
        _hero.Attack(attackDirection);
    }

    // Déclenche l'attaque dans la direction de visée courante
    private void HandleAttackButtonPressed()
    {
        // Ignore si aucun héros n'est assigné dans l'inspecteur
        if (_hero == null)
        {
            return;
        }

        // Récupère la direction de visée depuis le déplacement
        Vector2 attackDirection = _playerMovement != null
            ? _playerMovement.GetFacingDirection()
            : Vector2.up;

        // Lance l'attaque dans la direction du joueur
        _hero.Attack(attackDirection);
    }
}
