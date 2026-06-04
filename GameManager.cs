using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Предупреждение о Боссе")]
    [SerializeField] private GameObject bossWarningPanel;

    [Header("UI Геймплей")]
    [SerializeField] private Joystick joystickUI;
    [SerializeField] private Button shootButtonUI;
    [SerializeField] private Button abilityButtonUI;
    [SerializeField] private Button switchWeaponButtonUI;
    [SerializeField] private Button reloadButtonUI;
    [SerializeField] private TextMeshProUGUI reloadButtonText;
    [SerializeField] private HealthBarColor healthBarUI;
    [SerializeField] private Button pauseButtonUI;

    [Header("UI Панели")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("UI Слайдеры Настроек")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Текст")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Системные")]
    [SerializeField] private LevelConfigSO[] allLevels;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private float timeBetweenWaves = 10f;

    // Ссылки
    private PlayerController currentPlayer;
    private int currentMoney = 0;
    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private LevelConfigSO currentLevelData;
    private bool isPaused = false;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Инициализация UI
        pausePanel.SetActive(false);
        deathPanel.SetActive(false);
        settingsPanel.SetActive(false);
        victoryPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        if (bossWarningPanel != null) bossWarningPanel.SetActive(false);

        // Настройка кнопки паузы
        if (pauseButtonUI) pauseButtonUI.onClick.AddListener(TogglePause);

        // Настройка слайдеров
        if (musicSlider)
        {
            musicSlider.value = AudioManager.Instance.musicVolume;
            musicSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetVolumes(v, sfxSlider.value));
        }
        if (sfxSlider)
        {
            sfxSlider.value = AudioManager.Instance.sfxVolume;
            sfxSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetVolumes(musicSlider.value, v));
        }

        activeEnemies.Clear();
        currentMoney = 0;
        UpdateMoneyUI();
        SpawnPlayerCharacter();

        int levelIndex = PlayerPrefs.GetInt("CurrentLevel", 1) - 1;
        currentLevelData = (allLevels != null && levelIndex < allLevels.Length && levelIndex >= 0) ? allLevels[levelIndex] : allLevels[0];

        StartCoroutine(StartGameLoop());
    }

    // --- ЛОГИКА ПАУЗЫ И МЕНЮ ---

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            gameplayPanel.SetActive(false);
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        }
        else
        {
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            gameplayPanel.SetActive(true);
            AudioManager.Instance.PlayMusic(AudioManager.Instance.battleMusic);
        }
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameplayPanel.SetActive(false);
        deathPanel.SetActive(true);
        AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        Debug.Log("Game Over!");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // --- ОСТАЛЬНОЙ КОД (Спавн, Волны) ---

    void SpawnPlayerCharacter()
    {
        string charName = PlayerPrefs.GetString("SelectedCharacter", "Normal");
        CharacterDataSO charData = Resources.Load<CharacterDataSO>("Characters/" + charName);

        if (charData == null)
        {
            Debug.LogError($"[GameManager] Не удалось найти данные персонажа: Resources/Characters/{charName}. Проверь название файла и папку!");
            return;
        }

        GameObject playerObj = Instantiate(charData.characterPrefab, Vector3.zero, Quaternion.identity);
        currentPlayer = playerObj.GetComponent<PlayerController>();
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();

        if (currentPlayer != null)
        {
            currentPlayer.SetupControls(joystickUI, shootButtonUI, abilityButtonUI, switchWeaponButtonUI, reloadButtonUI);
            currentPlayer.InitializeCharacter(charData);
        }

        if (playerHealth != null) playerHealth.SetHealthBar(healthBarUI);
        if (shopManager != null && currentPlayer != null) shopManager.Initialize(currentPlayer);
    }

    public void UpdateAmmoUI(int currentMag, int reserve)
    {
        if (reloadButtonText != null)
            reloadButtonText.text = (reserve < 0) ? "∞" : $"{currentMag}/{reserve}";
    }

    IEnumerator StartGameLoop()
    {
        yield return new WaitForSeconds(1f);
        if (currentLevelData == null) yield break;

        for (int i = 0; i < currentLevelData.waves.Count; i++)
        {
            waveText.text = $"Волна {i + 1}";

            // Запускаем саму волну!
            yield return StartCoroutine(RunWave(currentLevelData.waves[i]));

            // Фаза магазина между волнами (кроме последней)
            if (i < currentLevelData.waves.Count - 1)
                yield return StartCoroutine(ShopPhase());
        }
        Victory();
    }

    IEnumerator RunWave(WaveSettings wave)
    {
        if (wave.isBossWave)
        {
            // ЭПИЧНОЕ ПРЕДУПРЕЖДЕНИЕ
            AudioManager.Instance.PlayMusic(null); // Тишина перед бурей
            if (bossWarningPanel != null) bossWarningPanel.SetActive(true);

            // Если у тебя есть скрипт CameraController, можешь раскомментировать тряску камеры
            if (CameraController.Instance != null)
                CameraController.Instance.ShakeCamera(0.5f, 3f);

            yield return new WaitForSeconds(3f); // Ждем 3 секунды в страхе

            if (bossWarningPanel != null) bossWarningPanel.SetActive(false);

            // Если есть музыка босса, включай её тут:
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
        }
        else
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.battleMusic);
        }

        // Возвращать ли игрока в центр перед каждой волной? Решай сам, пока оставляю закомментированным:
         if (currentPlayer != null) currentPlayer.transform.position = Vector3.zero;

        yield return new WaitForSeconds(wave.delayBeforeWave);

        // Спавн врагов из групп
        foreach (var group in wave.enemyGroups)
        {
            for (int k = 0; k < group.count; k++)
            {
                SpawnEnemy(group.enemyType);
                yield return new WaitForSeconds(0.5f); // Задержка между спавном каждого врага
            }
        }

        // Ждем, пока игрок не убьет всех врагов
        while (activeEnemies.Count > 0)
        {
            // Удаляем пустые ссылки (если враг уничтожился не через OnEnemyKilled)
            activeEnemies.RemoveAll(x => x == null || x.gameObject == null);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ShopPhase()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.shopMusic);

        gameplayPanel.SetActive(false);
        if (shopManager) shopManager.OpenShop();

        float timer = timeBetweenWaves;
        while (timer > 0)
        {
            waveText.text = $"Магазин: {timer:F1}";
            yield return null;
            timer -= Time.deltaTime;
        }

        if (shopManager) shopManager.CloseShop();
        gameplayPanel.SetActive(true);
    }

    void SpawnEnemy(EnemyTypeSO type)
    {
        if (spawnPoints.Length == 0) return;
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyObj = Instantiate(type.prefab, sp.position, Quaternion.identity);

        EnemyHealth healthComp = enemyObj.GetComponent<EnemyHealth>();
        if (healthComp != null)
        {
            healthComp.SetData(type.health, type.color);
            activeEnemies.Add(healthComp);
        }

        EnemyAI aiComp = enemyObj.GetComponent<EnemyAI>();
        if (aiComp) aiComp.speed = type.speed;
    }

    public void OnEnemyKilled(EnemyHealth enemy, int reward)
    {
        if (activeEnemies.Contains(enemy)) activeEnemies.Remove(enemy);
        AddMoney(reward);
    }

    public void AddMoney(int amount) { currentMoney += amount; UpdateMoneyUI(); }

    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount) { currentMoney -= amount; UpdateMoneyUI(); return true; }
        return false;
    }

    void UpdateMoneyUI() { if (moneyText) moneyText.text = $"$: {currentMoney}"; }

    void Victory()
    {
        LevelSelectManager.UnlockNextLevel();
        Time.timeScale = 0f;
        gameplayPanel.SetActive(false);
        victoryPanel.SetActive(true);
        AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
    }
    public void RegisterEnemy(EnemyHealth enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }
}