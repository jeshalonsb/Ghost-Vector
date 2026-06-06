using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image vignette;

    [SerializeField] private float maxAlpha = 0.6f;

    private void Update()
    {
        float intensity = 1f - playerHealth.HealthPercent;

        Color c = vignette.color;
        c.a = Mathf.Lerp(0f, maxAlpha, intensity);

        vignette.color = c;
    }
}
