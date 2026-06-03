using UnityEngine;

public class Enemy : MonoBehaviour
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
            Debug.Log(health);

            if (health <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
    private void Start()
    {
        Health = StartingHealth;
    }
}
