using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFill;

    private void Update()
    {
        healthFill.fillAmount = playerHealth.HealthPercent;
    }
}
