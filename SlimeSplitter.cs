using UnityEngine;
using System.Collections;

public class SlimeSplitter : MonoBehaviour
{
    [Header("Настройки деления")]
    public GameObject childPrefab;
    public int generation = 1;
    public int maxGenerations = 5;

    [Header("Бонус: Способность")]
    public float jumpInterval = 4f;
    private float nextJumpTime;

    private EnemyAI ai;
    private Vector3 originalScale;

    // Переменная для сохранения стандартной скорости
    private float defaultSpeed = 2f;

    void Start()
    {
        ai = GetComponent<EnemyAI>();
        originalScale = transform.localScale;
        nextJumpTime = Time.time + jumpInterval;

        if (ai != null)
        {
            defaultSpeed = ai.speed;
        }
    }

    void Update()
    {
        // Прыгаем только если ИИ активен и скорость больше нуля (чтобы не прыгал во время отбрасывания)
        if (Time.time > nextJumpTime && ai != null && ai.enabled)
        {
            StartCoroutine(JumpRoutine());
            nextJumpTime = Time.time + jumpInterval;
        }
    }

    IEnumerator JumpRoutine()
    {
        float originalSpeed = ai.speed;

        // 1. Остановка и Сжатие (Готовится к прыжку)
        ai.speed = 0;
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z), t / 0.3f);
            yield return null;
        }

        if (AudioManager.Instance) AudioManager.Instance.PlaySFX(null);

        // 2. Прыжок и Растяжение (Летит к игроку)
        ai.speed = originalSpeed * 4f;
        t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z),
                                                new Vector3(originalScale.x * 0.8f, originalScale.y * 1.2f, originalScale.z), t / 0.2f);
            yield return null;
        }

        // 3. Возврат к нормальному состоянию и скорости
        ai.speed = originalSpeed;
        t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(new Vector3(originalScale.x * 0.8f, originalScale.y * 1.2f, originalScale.z), originalScale, t / 0.2f);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void OnSlimeDeath()
    {
        if (generation >= maxGenerations || childPrefab == null) return;

        for (int i = 0; i < 2; i++)
        {
            // Добавляем небольшой отступ при спавне, чтобы их коллайдеры не пересекались идеально по центру
            Vector3 offset = (i == 0) ? new Vector3(-0.3f, 0, 0) : new Vector3(0.3f, 0, 0);
            GameObject child = Instantiate(childPrefab, transform.position + offset, Quaternion.identity);

            SlimeSplitter childScript = child.GetComponent<SlimeSplitter>();
            if (childScript != null) childScript.generation = generation + 1;

            EnemyHealth childHealth = child.GetComponent<EnemyHealth>();
            if (childHealth != null && GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemy(childHealth);
            }

            // Направление отбрасывания (влево для первого, вправо для второго)
            Vector2 burstDir = (i == 0) ? Vector2.left : Vector2.right;

            // Запускаем корутину отбрасывания на НОВОМ слайме
            if (childScript != null)
            {
                childScript.StartCoroutine(childScript.KnockbackRoutine(burstDir));
            }

            if (AudioManager.Instance) AudioManager.Instance.PlaySFX(null);
        }
    }

    // НОВОЕ: Корутина отбрасывания при появлении
    public IEnumerator KnockbackRoutine(Vector2 direction)
    {
        // Получаем ИИ, так как Start() мог еще не успеть отработать у нового клона
        if (ai == null) ai = GetComponent<EnemyAI>();

        // Отключаем ИИ, чтобы он не мешал физике двигать слайма
        if (ai != null)
        {
            ai.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Применяем физический толчок
            rb.AddForce(direction * 7f, ForceMode2D.Impulse);
        }

        // Ждем полсекунды, пока они разлетятся
        yield return new WaitForSeconds(0.5f);

        // Останавливаем скольжение по инерции (чтобы слайм не улетал как на льду)
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Возвращаем ИИ к жизни
        if (ai != null)
        {
            ai.enabled = true;
            ai.speed = defaultSpeed;
        }
    }
}