using UnityEngine;
using UnityEngine.EventSystems;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifetime = 3f;

    private Vector3 moveDirection;
    private bool firedByPlayer;

    public void Initailize(Vector3 direction, bool isPlayerBullet)
    {
        moveDirection = direction.normalized;
        firedByPlayer = isPlayerBullet;
    }
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (firedByPlayer)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}