using UnityEngine;
using UnityEngine.UI;

public class HealthBarColor : MonoBehaviour
{
    [SerializeField] private Image healthFill; // Fill Image ползунка
    [SerializeField] private Slider healthSlider;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFill == null || healthSlider == null) return;

        // Заполнение ползунка
        float healthPercent = currentHealth / maxHealth;
        healthSlider.value = healthPercent;

        // 🔥 ПРОСТЫЕ ЦВЕТА ПО HP!
        Color barColor;
        if (healthPercent > 0.6f)
            barColor = Color.green;      // 🟢 100-60%
        else if (healthPercent > 0.3f)
            barColor = Color.yellow;     // 🟡 60-30%  
        else
            barColor = Color.red;        // 🔴 <30%

        healthFill.color = barColor;
    }
}
