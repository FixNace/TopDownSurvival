using UnityEngine;
using System.Collections;

public class NecromancerBoss : MonoBehaviour
{
    [Header("Настройки Босса")]
    public GameObject poisonPuddlePrefab;
    public GameObject[] eliteMinionPrefabs; // Положи сюда префабы Снайпера и Камикадзе

    public float summonInterval = 7f;
    public float poisonInterval = 4f;

    private Transform player;
    private float nextSummonTime;
    private float nextPoisonTime;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        nextSummonTime = Time.time + 3f; // Даем 3 секунды форы в начале
        nextPoisonTime = Time.time + 5f;
    }

    void Update()
    {
        if (player == null) return;

        // Поворот к игроку (он стоит на месте или слегка левитирует)
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Спавн элиты
        if (Time.time >= nextSummonTime)
        {
            nextSummonTime = Time.time + summonInterval;
            SummonElites();
        }

        // Каст лужи яда под игрока
        if (Time.time >= nextPoisonTime)
        {
            nextPoisonTime = Time.time + poisonInterval;
            CastPoison();
        }
    }

    void SummonElites()
    {
        if (eliteMinionPrefabs.Length == 0) return;

        // Спавним 2 случайных элитных врагов
        for (int i = 0; i < 2; i++)
        {
            GameObject prefab = eliteMinionPrefabs[Random.Range(0, eliteMinionPrefabs.Length)];
            Vector2 spawnOffset = Random.insideUnitCircle * 2f;
            Instantiate(prefab, (Vector2)transform.position + spawnOffset, Quaternion.identity);
        }

        // Можно добавить анимацию или звук призыва
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPrioritySFX(AudioManager.Instance.abilityUse);
    }

    void CastPoison()
    {
        if (poisonPuddlePrefab == null || player == null) return;

        // Создаем лужу ровно там, где сейчас стоит игрок
        Instantiate(poisonPuddlePrefab, player.position, Quaternion.identity);
    }
}