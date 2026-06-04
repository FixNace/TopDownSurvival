using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Нужно для списков

public enum EnemyBehaviorType
{
    Standard,
    Kamikaze,
    Summoner,
    Sniper,
    Shooter,
    HookBoss
}

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Настройки Типа")]
    public EnemyBehaviorType behaviorType = EnemyBehaviorType.Standard;

    [Header("Параметры Движения")]
    public float speed = 3f;
    public float rotationSpeed = 10f;
    [SerializeField] private float obstacleCheckDistance = 1.5f;
    [SerializeField] private float avoidRadius = 0.4f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Бой (Стрелки, Снайперы)")]
    public GameObject projectilePrefab;
    public Transform firePoint; // Используется для обычных стрелков
    public float shootInterval = 4f;
    public float keepDistance = 6f;

    [Header("Призыватель")]
    public GameObject minionPrefab;

    [Header("Снайпер / Босс")]
    public float aimDuration = 1.5f;
    public LineRenderer aimLine; // Используется как лазер и как веревка хука

    [Header("Настройки Босса (HookBoss)")]
    [Tooltip("Точка, откуда стреляет пушка ядрами")]
    public Transform cannonFirePoint;
    [Tooltip("Точка, откуда вылетает крюк (обычно рука)")]
    public Transform hookFirePoint;
    [Tooltip("Префаб самого крюка (спрайт)")]
    public GameObject hookPrefab;
    public GameObject cannonWarningPrefab;
    public GameObject bombPrefab;
    public float bossAttackInterval = 4f;
    public float hookMaxDistance = 15f;
    public float hookFlySpeed = 25f; // Быстрая скорость полета крюка
    public float hookPullSpeed = 15f; // Скорость притягивания (должна быть > скорости игрока!)

    private Transform player;
    private Rigidbody2D rb;
    private float nextActionTime;
    private bool isActing = false;

    // Ссылки для визуала хука
    private GameObject spawnedHookVisual;

    // Оптимизация навигации
    private Vector2 currentDir;
    private Vector2 bestDir;
    private float nextPathUpdate;
    private const float PATH_UPDATE_RATE = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        nextPathUpdate = Time.time + Random.Range(0f, PATH_UPDATE_RATE);
        nextActionTime = Time.time + 2f;

        // Инициализация визуала крюка для босса
        if (behaviorType == EnemyBehaviorType.HookBoss && hookPrefab != null && hookFirePoint != null)
        {
            spawnedHookVisual = Instantiate(hookPrefab, hookFirePoint.position, hookFirePoint.rotation);
            spawnedHookVisual.transform.SetParent(hookFirePoint); // Привязываем к руке босса

            // Настройка LineRenderer для веревки
            if (aimLine != null)
            {
                aimLine.enabled = false;
                aimLine.useWorldSpace = true;
                aimLine.textureMode = LineTextureMode.Tile; // Текстура будет повторяться, создавая эффект "кусков"
            }
        }
    }

    void Update()
    {
        if (player == null || isActing) return;

        // 1. Логика Атак
        if (Time.time >= nextActionTime)
        {
            if (behaviorType == EnemyBehaviorType.Summoner) { Summon(); nextActionTime = Time.time + 5f; }
            else if (behaviorType == EnemyBehaviorType.Sniper) StartCoroutine(SniperShootRoutine());
            else if (behaviorType == EnemyBehaviorType.Shooter) { Shoot(); nextActionTime = Time.time + shootInterval; }
            else if (behaviorType == EnemyBehaviorType.HookBoss)
            {
                int rand = Random.Range(0, 3);
                if (rand == 0) StartCoroutine(BossCannonRoutine());
                else if (rand == 1) StartCoroutine(BossHookRoutine());
                else { SpawnBombs(); nextActionTime = Time.time + bossAttackInterval; } // Полный интервал после бомбы
            }
        }

        if (Time.time >= nextPathUpdate)
        {
            UpdatePathfinding();
            nextPathUpdate = Time.time + PATH_UPDATE_RATE;
        }
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!isActing)
        {
            currentDir = Vector2.Lerp(currentDir, bestDir, Time.fixedDeltaTime * 10f);

            if (currentDir.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = currentDir.normalized * speed;

                float angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void UpdatePathfinding()
    {
        float distSq = (player.position - transform.position).sqrMagnitude;
        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        bool shouldMove = true;

        if (behaviorType == EnemyBehaviorType.Shooter || behaviorType == EnemyBehaviorType.HookBoss)
            if (distSq < keepDistance * keepDistance) shouldMove = false;

        if (behaviorType == EnemyBehaviorType.Summoner || behaviorType == EnemyBehaviorType.Sniper)
            if (distSq < (keepDistance - 2f) * (keepDistance - 2f)) dirToPlayer = -dirToPlayer;
            else if (distSq < keepDistance * keepDistance) shouldMove = false;

        if (shouldMove) bestDir = GetBestDirection(dirToPlayer);
        else bestDir = Vector2.zero;
    }

    Vector2 GetBestDirection(Vector2 targetDir)
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, avoidRadius, targetDir, obstacleCheckDistance, obstacleLayer);
        if (hit.collider == null) return targetDir;

        float[] angles = { 30f, -30f, 60f, -60f, 90f, -90f };
        foreach (float angle in angles)
        {
            Vector2 testDir = Quaternion.Euler(0, 0, angle) * targetDir;
            RaycastHit2D testHit = Physics2D.CircleCast(transform.position, avoidRadius, testDir, obstacleCheckDistance, obstacleLayer);
            if (testHit.collider == null) return testDir;
        }
        return Vector2.zero;
    }

    void Shoot()
    {
        if (!projectilePrefab) return;
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(projectilePrefab, spawnPos, transform.rotation);
        Vector2 dir = (player.position - spawnPos).normalized;
        Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
        if (bRb) bRb.linearVelocity = dir * 15f;
        Destroy(bullet, 5f);
    }

    // --- Улучшенный Метод Стрельбы для Босса ---
    void BossShootCannon()
    {
        if (!projectilePrefab || !cannonFirePoint) return;

        GameObject bullet = Instantiate(projectilePrefab, cannonFirePoint.position, cannonFirePoint.rotation);

        Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
        if (bRb)
        {
            bRb.gravityScale = 0f; // ЖЕСТКО ВЫКЛЮЧАЕМ ПАДЕНИЕ ВНИЗ
            bRb.linearVelocity = transform.right * 15f; // Чуть ускорили ядро
        }
        Destroy(bullet, 5f);
    }
    void Summon() { /* Твой старый код спавна миньонов */ }

    IEnumerator SniperShootRoutine()
    {
        isActing = true;
        rb.linearVelocity = Vector2.zero;
        if (aimLine) aimLine.enabled = true;

        float timer = 0f;
        while (timer < aimDuration && player != null)
        {
            timer += Time.deltaTime;
            if (aimLine)
            {
                aimLine.SetPosition(0, firePoint ? firePoint.position : transform.position);
                aimLine.SetPosition(1, player.position);
            }
            yield return null;
        }

        if (aimLine) aimLine.enabled = false;
        if (player != null) Shoot();

        isActing = false;
        nextActionTime = Time.time + shootInterval;
    }

    // --- ЭПИЧНЫЕ АТАКИ БОССА (ОБНОВЛЕННЫЕ) ---

    IEnumerator BossCannonRoutine()
    {
        isActing = true;
        rb.linearVelocity = Vector2.zero;
        Vector3 targetPos = player.position;
        GameObject warning = null;

        if (cannonWarningPrefab)
        {
            warning = Instantiate(cannonWarningPrefab, targetPos, Quaternion.identity);
            float t = 0;
            while (t < aimDuration)
            {
                if (warning) warning.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 2.5f, t / aimDuration);
                t += Time.deltaTime;
                yield return null;
            }
        }
        else yield return new WaitForSeconds(aimDuration);

        if (warning) Destroy(warning);

        // Поворачиваем босса к точке выстрела перед залпом
        Vector2 aimDir = (targetPos - transform.position).normalized;
        transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg, Vector3.forward);

        // Стреляем из ПУШКИ
        BossShootCannon();

        isActing = false;
        nextActionTime = Time.time + bossAttackInterval;
    }

    IEnumerator BossHookRoutine()
    {
        isActing = true;
        rb.linearVelocity = Vector2.zero;

        if (aimLine) aimLine.enabled = true;

        // 1. Прицеливание
        float timer = 0f;
        Vector2 aimDir = Vector2.zero;
        while (timer < aimDuration && player != null)
        {
            timer += Time.deltaTime;
            Vector3 startPos = hookFirePoint ? hookFirePoint.position : transform.position;
            aimDir = (player.position - startPos).normalized;

            transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg, Vector3.forward);

            if (aimLine)
            {
                aimLine.SetPosition(0, startPos);
                aimLine.SetPosition(1, startPos + (Vector3)(aimDir * hookMaxDistance));
            }
            yield return null;
        }

        if (aimLine) aimLine.enabled = false;
        if (spawnedHookVisual == null || player == null) { isActing = false; yield break; }

        // 2. СКАНИРУЕМ ПУТЬ ПЕРЕД ВЫСТРЕЛАМИ (Ищем первую преграду)
        Vector3 startHookPos = hookFirePoint.position;
        // Пускаем луч и получаем ВСЕ объекты на пути
        RaycastHit2D[] hits = Physics2D.CircleCastAll(startHookPos, 0.5f, aimDir, hookMaxDistance);

        // Сортируем попадания от ближайшего к дальнему, чтобы крюк поймал ПЕРВЫЙ объект
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Vector3 targetHookPos = startHookPos + (Vector3)(aimDir * hookMaxDistance); // По умолчанию летим на макс длину
        Transform caughtTarget = null;

        foreach (var hit in hits)
        {
            // Игнорируем самого босса
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                string tag = hit.collider.tag;
                if (tag == "Player" || tag == "Bomb" || tag == "Shield" || tag == "Wall")
                {
                    targetHookPos = hit.point; // Укорачиваем путь крюка ровно до объекта!

                    if (tag == "Player" || tag == "Bomb")
                    {
                        caughtTarget = hit.collider.transform; // Запоминаем, кого тащить

                        if (tag == "Bomb")
                        {
                            // Снимаем бомбу с предохранителя!
                            BossBomb bomb = hit.collider.GetComponent<BossBomb>();
                            if (bomb != null) bomb.isPulled = true;
                        }
                    }
                    // Если попали в Shield или Wall — caughtTarget остается null, крюк просто ударится и вернется.
                    break;
                }
            }
        }

        // 3. ФИЗИЧЕСКИЙ ПОЛЕТ КРЮКА
        spawnedHookVisual.transform.SetParent(null);
        if (aimLine) aimLine.enabled = true;

        float flyTimer = 0f;
        hookFlySpeed = 60f; // УСКОРИЛИ КРЮК (было 25)
        float duration = Vector2.Distance(startHookPos, targetHookPos) / hookFlySpeed;

        while (flyTimer < duration)
        {
            flyTimer += Time.deltaTime;
            spawnedHookVisual.transform.position = Vector3.Lerp(startHookPos, targetHookPos, flyTimer / duration);
            spawnedHookVisual.transform.right = aimDir;

            if (aimLine)
            {
                aimLine.SetPosition(0, hookFirePoint.position);
                aimLine.SetPosition(1, spawnedHookVisual.transform.position);
            }
            yield return null;
        }

        // 4. ПРИТЯГИВАНИЕ ЖЕРТВЫ (Если кого-то поймали)
        if (caughtTarget != null)
        {
            float pullTimer = 0f;
            Rigidbody2D targetRb = caughtTarget.GetComponent<Rigidbody2D>();
            hookPullSpeed = 30f; // УСКОРИЛИ ПРИТЯГИВАНИЕ (было 15)

            while (pullTimer < 2.0f && caughtTarget != null)
            {
                pullTimer += Time.deltaTime;
                Vector3 currentBossPos = hookFirePoint.position;

                spawnedHookVisual.transform.position = caughtTarget.position;
                spawnedHookVisual.transform.right = (currentBossPos - caughtTarget.position).normalized;

                if (aimLine)
                {
                    aimLine.SetPosition(0, currentBossPos);
                    aimLine.SetPosition(1, caughtTarget.position);
                }

                if (targetRb != null)
                {
                    Vector2 pullDir = (currentBossPos - caughtTarget.position).normalized;
                    targetRb.linearVelocity = pullDir * hookPullSpeed;
                }

                if (Vector2.Distance(caughtTarget.position, currentBossPos) < 1.6f) break;

                yield return null;
            }
        }
        else
        {
            // Если ударились в щит/стену или промазали — быстрый возврат
            float retTimer = 0f;
            Vector3 endPos = spawnedHookVisual.transform.position;
            while (retTimer < 0.2f)
            {
                retTimer += Time.deltaTime;
                spawnedHookVisual.transform.position = Vector3.Lerp(endPos, hookFirePoint.position, retTimer / 0.2f);
                if (aimLine)
                {
                    aimLine.SetPosition(0, hookFirePoint.position);
                    aimLine.SetPosition(1, spawnedHookVisual.transform.position);
                }
                yield return null;
            }
        }

        // Завершение
        if (aimLine) aimLine.enabled = false;
        spawnedHookVisual.transform.SetParent(hookFirePoint);
        spawnedHookVisual.transform.localPosition = Vector3.zero;
        spawnedHookVisual.transform.localRotation = Quaternion.identity;

        isActing = false;
        nextActionTime = Time.time + bossAttackInterval;
    }

    // Уменьшено кол-во бомб
    void SpawnBombs()
    {
        if (bombPrefab == null) return;

        // Спавним ТОЛЬКО ОДНУ бомбу за раз
        Vector2 spawnOffset = Random.insideUnitCircle * keepDistance;
        Instantiate(bombPrefab, (Vector2)transform.position + spawnOffset, Quaternion.identity);
    }

    // Урон игроку (Твой старый метод)
    private void OnCollisionStay2D(Collision2D other) { CheckHit(other.gameObject); }
    private void OnTriggerEnter2D(Collider2D other) { CheckHit(other.gameObject); }
    void CheckHit(GameObject target)
    {
        PlayerHealth ph = target.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            float dmg = 10f; // Стандартный урон

            if (behaviorType == EnemyBehaviorType.Kamikaze)
            {
                dmg = 40f;
                if (AudioManager.Instance) AudioManager.Instance.PlayPrioritySFX(AudioManager.Instance.enemyDeath);
                if (EffectSpawner.Instance) EffectSpawner.Instance.SpawnDeathEffect(transform.position);
                Destroy(gameObject);
            }
            else if (behaviorType == EnemyBehaviorType.HookBoss)
            {
                dmg = 35f; // БОСС БЬЕТ БОЛЬНО В БЛИЖНЕМ БОЮ
            }

            ph.TakeDamage(dmg);
        }
    }
}