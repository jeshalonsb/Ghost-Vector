using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}