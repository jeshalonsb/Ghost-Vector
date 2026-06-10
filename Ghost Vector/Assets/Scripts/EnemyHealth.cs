using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float StartingHealth;
    private float health;
    public float Health 
    {
        get 
        {
            return health;
        }
        set
        {
            health = value;
            Debug.Log("Enemy Health: " + health);

            if (health <= 0f)
            {
                Die();
            }
        }
    }
    private void Start()
    {
        Health = StartingHealth;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
    }
    private void Die()
    {
        Debug.Log("Enemy Died!");

        EnemyAi enemyAi = GetComponent<EnemyAi>();

        if (enemyAi != null)
        {
            enemyAi.Die();
        }
        else
        {
            Destroy(gameObject);
        }
            
    }
}

