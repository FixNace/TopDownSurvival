using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Настройки пули врага")]
    [SerializeField] private float damage = 15f; // Теперь можно менять в Инспекторе!

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // ФИКС: Используем переменную damage вместо жесткого 15f
                ph.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Shield"))
        {
            if (EffectSpawner.Instance != null)
                EffectSpawner.Instance.SpawnHitEffect(transform.position, Quaternion.identity);

            Destroy(gameObject); // Уничтожаем пулю об щит
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}