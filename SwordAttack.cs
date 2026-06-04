using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    private Collider2D swordCollider;

    void Start()
    {
        swordCollider = GetComponent<Collider2D>();
        swordCollider.enabled = false; // Выключен по умолчанию
    }

    public void EnableSword()
    {
        swordCollider.enabled = true;
    }

    public void DisableSword()
    {
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}
