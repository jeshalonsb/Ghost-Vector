using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("References")]
    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField] CharacterController controller;

    public float HealthPercent => currentHealth / maxHealth;

    private bool isDead;


    private void Start ()
    {
        currentHealth = maxHealth;
        Debug.Log("Player Health" + currentHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Player died!");

        if (playerMovement != null) 
            playerMovement.enabled = false;

        if (controller != null)
            controller.enabled = false;

        Invoke(nameof(Restart), 2f);

        FindFirstObjectByType<FirstPersonController>().DisableMovement();
    }
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
