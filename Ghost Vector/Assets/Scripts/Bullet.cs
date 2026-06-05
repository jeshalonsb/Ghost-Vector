using UnityEngine;
using UnityEngine.EventSystems;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifetime = 3f;

    private Vector3 moveDirection;

    public void Initailize(Vector3 direction)
    {
        moveDirection = direction.normalized;
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
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}