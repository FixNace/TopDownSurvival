using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private CharacterDataSO charData;

    [Header("Параметры")]
    public float maxHealth = 100f;
    public float currentHealth;

    private HealthBarColor healthBarColor;

    public void SetHealthBar(HealthBarColor hudBar)
    {
        healthBarColor = hudBar;
        UpdateUI();
    }

    public void InitializeHealth(CharacterDataSO data)
    {
        charData = data;
        maxHealth = data.maxHealth;
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.hitPlayer);
        if (charData != null && charData.isTank)
        {
            if (Random.value < charData.blockChance) return;
        }

        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthBarColor != null)
        {
            healthBarColor.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameOver();
        Destroy(gameObject);
    }
}