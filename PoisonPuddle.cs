using UnityEngine;
using System.Collections;

public class PoisonPuddle : MonoBehaviour
{
    [Header("Настройки яда")]
    public float warningTime = 1f; // Время до активации
    public float activeTime = 4f;  // Сколько лужа живет
    public float damagePerTick = 10f;
    public float tickRate = 0.5f;

    private SpriteRenderer sr;
    private CircleCollider2D col;
    private bool isActive = false;
    private float nextDamageTime;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();
        col.enabled = false; // Отключаем урон на время предупреждения

        sr.color = new Color(0, 1, 0, 0.3f); // Прозрачный зеленый (предупреждение)
        StartCoroutine(PuddleLifeCycle());
    }

    IEnumerator PuddleLifeCycle()
    {
        // 1. Фаза предупреждения
        yield return new WaitForSeconds(warningTime);

        // 2. Фаза урона
        isActive = true;
        col.enabled = true;
        sr.color = new Color(0, 1, 0, 0.8f); // Яркий зеленый (ОПАСНО)

        yield return new WaitForSeconds(activeTime);

        // 3. Исчезновение
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            nextDamageTime = Time.time + tickRate;
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damagePerTick);
                // Тут можно добавить тихий звук шипения яда
            }
        }
    }
}