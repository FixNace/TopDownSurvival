using UnityEngine;

public class EffectSpawner : MonoBehaviour
{
    public static EffectSpawner Instance { get; private set; }

    [Header("Ёффекты")]
    [SerializeField] private GameObject hitEffectPrefab; // —юда перетащи префаб взрыва/крови
    [SerializeField] private GameObject deathEffectPrefab; // Ёффект смерти (опционально)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnHitEffect(Vector3 position, Quaternion rotation)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, rotation);
            Destroy(effect, 1.5f); // ”дал€ем эффект через 1.5 сек
        }
    }

    public void SpawnDeathEffect(Vector3 position)
    {
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
}