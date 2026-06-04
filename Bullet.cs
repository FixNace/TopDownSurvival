using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    private float damage;
    private bool isVampire;
    private float vampirismAmount;
    private PlayerHealth playerHealthRef; // Ссылка на игрока для лечения
    private int pierceRemaining;
    public void Initialize(Vector2 direction, float speed, float dmg, float size, int pierce, bool vamp, float vampAmount, PlayerHealth playerRef)
    {
        damage = dmg;
        isVampire = vamp;
        vampirismAmount = vampAmount;
        playerHealthRef = playerRef;

        // Устанавливаем размер. 
        // Если пуля все равно маленькая, проверь, что в WeaponDataSO 'bulletSizeScale' стоит например 2 или 3.
        transform.localScale = new Vector3(size, size, 1f);

        Destroy(gameObject, 5f);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // ВАЖНО: Мы пускаем пулю туда, куда смотрит сама ПУЛЯ (её нос)
        rb.linearVelocity = transform.right * speed;
        pierceRemaining = pierce; // Получаем значение из PlayerController
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // ЛОГИКА ВАМПИРА (Исправленная)
                if (isVampire && playerHealthRef != null)
                {
                    // Лечим на % от нанесенного урона
                    float healVal = damage * vampirismAmount;
                    if (healVal < 1) healVal = 1; // Минимум 1 хп
                    playerHealthRef.Heal(healVal);
                }
            }

            // Эффекты
            if (EffectSpawner.Instance != null)
                EffectSpawner.Instance.SpawnHitEffect(transform.position, Quaternion.identity);

            if (pierceRemaining > 0)
            {
                pierceRemaining--; // Уменьшаем количество оставшихся пробитий
                                   // Пуля продолжает полет (Destroy не вызывается)
            }
            else
            {
                Destroy(gameObject); // Исчезает, если пробитий больше нет
            }
            
        }
        else if (other.CompareTag("Wall"))
        {
            if (EffectSpawner.Instance != null)
                EffectSpawner.Instance.SpawnHitEffect(transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}