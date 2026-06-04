using UnityEngine;

public class BossBomb : MonoBehaviour
{
    public float damageToBoss = 150f;
    [HideInInspector] public bool isPulled = false; // Флаг: притянули ли бомбу крюком?

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Взрываемся об босса ТОЛЬКО если бомба притянута крюком
        if (isPulled && other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null && enemy.behaviorType == EnemyBehaviorType.HookBoss)
            {
                EnemyHealth bossHealth = other.GetComponent<EnemyHealth>();
                if (bossHealth != null) bossHealth.TakeDamage(damageToBoss);

                if (EffectSpawner.Instance) EffectSpawner.Instance.SpawnDeathEffect(transform.position);

                Destroy(gameObject);
            }
        }
    }
}