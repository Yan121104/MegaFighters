using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Referencia a la imagen de relleno")]
    public Image fillImage;

    private float maxHealth;
    private float currentHealth;

    public void Initialize(float max)
    {
        maxHealth = max;
        currentHealth = max;
        UpdateBar();
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (fillImage != null)
            fillImage.fillAmount = currentHealth / maxHealth;
    }

    private void LateUpdate()
    {
        // Mantener la barra orientada a la cámara
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }
}
