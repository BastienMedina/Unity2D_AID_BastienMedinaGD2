using UnityEngine;

public class EnemyNetworkUnit : EnemyBase
{
    private Transform _playerTransform;
    private LivesManager _livesManager;
    private EnemyNetworkSpawner _spawner;
    private float _speed;
    private int _damage;
    private float _despawnRadius;
    private Vector3 _spawnPosition;

    public void Initialize(Transform playerTransform, LivesManager livesManager, EnemyNetworkSpawner spawner, float speed, int damage, float despawnRadius) // Injecte toutes les données du spawner
    {
        _playerTransform = playerTransform;
        _livesManager    = livesManager;
        _spawner         = spawner;
        _speed           = speed;
        _damage          = damage;
        _despawnRadius   = despawnRadius;
        _spawnPosition   = transform.position;
    }

    private void Update() // Poursuit le joueur et vérifie le rayon de despawn
    {
        if (IsDead() || _playerTransform == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _speed * Time.deltaTime);

        if (Vector3.Distance(_spawnPosition, transform.position) >= _despawnRadius) // Despawn si hors portée
            Despawn();
    }

    private void OnCollisionEnter2D(Collision2D collision) // Inflige dégâts au joueur au contact
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        _livesManager.TakeDamage();
    }

    private void Despawn() // Notifie le spawner et se détruit
    {
        _spawner.NotifyUnitRemoved();
        Destroy(gameObject);
    }

    protected override void HandleDeath() // Notifie le spawner et désactive l'objet
    {
        _spawner.NotifyUnitRemoved();
        gameObject.SetActive(false);
    }
}
