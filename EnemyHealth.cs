using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour
{
    [Header("Босс-логика")]
    public bool isBoss;
    public bool needsMinionsDead; // Должны ли миньоны умереть для получения урона
    [SerializeField] private float hp = 50f;
    [SerializeField] private float maxHp;
    [SerializeField] private int moneyReward = 10;

    [Header("Для Босса (Опционально)")]
    public Slider localHealthBar; // <--- Перетащи сюда Slider из Canvas босса!

    private bool isDead = false;

    public void SetData(float health, Color color)
    {
        maxHp = health;
        hp = health;
        GetComponent<SpriteRenderer>().color = color;
        moneyReward = Mathf.RoundToInt(health / 5);

        if (localHealthBar != null)
        {
            localHealthBar.maxValue = maxHp;
            localHealthBar.value = hp;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (needsMinionsDead)
        {
            // Ищем всех врагов, кроме самого себя
            GameObject[] minions = GameObject.FindGameObjectsWithTag("Enemy");
            if (minions.Length > 1) // 1 — это сам босс
            {
                Debug.Log("Босс неуязвим, пока живы слуги!");
                return;
            }
        }
        hp -= damage;
        //if (AudioManager.Instance) AudioManager.Instance.PlayCombatSFX(AudioManager.Instance.hitEnemy);
        if (localHealthBar != null) localHealthBar.value = hp; // Обновляем ползунок
        
        if (hp <= 0)
        {
            Die();
            if (AudioManager.Instance) AudioManager.Instance.PlayCombatSFX(AudioManager.Instance.enemyDeath);
        }
    }

    void Die()
    {
        if (isDead) return; // Двойная проверка
        isDead = true;
        SlimeSplitter slime = GetComponent<SlimeSplitter>();
        if (slime != null) slime.OnSlimeDeath();
        // Сообщаем GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled(this, moneyReward); // Передаем ссылку на себя
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPrioritySFX(AudioManager.Instance.enemyDeath);
        }
        Destroy(gameObject);
    }
}