using UnityEngine;

// Déplace la balle vers le haut et détecte les impacts sur virus et murs
public class SI_PlayerBullet : MonoBehaviour
{
    // Vitesse de déplacement vertical de la balle en unités par seconde
    [SerializeField] private float _speed = 12f;

    // Dégâts infligés au virus touché par cette balle
    [SerializeField] private int _damage = 1;

    // Distance maximale avant auto-destruction de la balle
    [SerializeField] private float _maxRange = 12f;

    // Référence au Rigidbody2D pour le déplacement physique
    private Rigidbody2D _rigidbody;

    // Position enregistrée à l'instanciation pour le calcul de portée
    private Vector2 _spawnPosition;

    // Initialise le Rigidbody2D et mémorise la position de spawn
    private void Awake()
    {
        // Récupère le Rigidbody2D requis pour MovePosition
        _rigidbody = GetComponent<Rigidbody2D>();

        // Signale si le Rigidbody2D est manquant sur la balle
        if (_rigidbody == null)
        {
            Debug.LogError("[BULLET] SI_PlayerBullet — Rigidbody2D manquant sur la balle");
        }

        // Enregistre la position monde au moment de l'instanciation
        _spawnPosition = transform.position;
    }

    // Déplace la balle vers le haut et vérifie la portée maximale
    private void FixedUpdate()
    {
        // Abandonne si le Rigidbody2D est absent de la balle
        if (_rigidbody == null)
        {
            return;
        }

        // Calcule la prochaine position en montant tout droit vers le haut
        Vector2 nextPosition = _rigidbody.position + (Vector2.up * (_speed * Time.fixedDeltaTime));

        // Déplace la balle vers la position calculée via la physique
        _rigidbody.MovePosition(nextPosition);

        // Détruit la balle si la distance parcourue dépasse la portée
        if (Vector2.Distance(_spawnPosition, _rigidbody.position) >= _maxRange)
        {
            // Supprime le GameObject quand la portée maximale est atteinte
            Destroy(gameObject);
        }
    }

    // Détecte les virus et les murs via le trigger CircleCollider2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Tente de récupérer l'interface IVirusDamageable sur le collider
        IVirusDamageable target = null;
        other.TryGetComponent(out target);

        // Cherche l'interface sur le parent si absente sur le collider direct
        if (target == null)
        {
            target = other.GetComponentInParent<IVirusDamageable>();
        }

        // Applique les dégâts et détruit la balle si la cible est un virus
        if (target != null)
        {
            // Inflige les dégâts configurés au virus touché
            target.TakeDamage(_damage);

            // Supprime la balle immédiatement après l'impact sur le virus
            Destroy(gameObject);
            return;
        }

        // Vérifie si l'objet touché appartient au layer Wall
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // Supprime la balle à l'impact avec un mur
            Destroy(gameObject);
        }
    }
}
