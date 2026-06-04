using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("UI Панель")]
    [SerializeField] private GameObject shopPanel;

    [Header("Слот 1: Лечение")]
    [SerializeField] private Button healButton;
    [SerializeField] private TextMeshProUGUI healPriceText;
    [SerializeField] private TextMeshProUGUI healDescText;

    [Header("Слот 2: Урон")]
    [SerializeField] private Button damageButton;
    [SerializeField] private TextMeshProUGUI damagePriceText;
    [SerializeField] private TextMeshProUGUI damageDescText;

    [Header("Слот 3: Тактика (НОВОЕ)")]
    [SerializeField] private Button tacticalButton; // <-- Переименуй в инспекторе ammoButton
    [SerializeField] private TextMeshProUGUI tacticalPriceText;
    [SerializeField] private TextMeshProUGUI tacticalDescText; // <-- Добавь текст описания для 3 слота

    [Header("Слот 4: Специальное")]
    [SerializeField] private Button specialButton;
    [SerializeField] private TextMeshProUGUI specialPriceText;
    [SerializeField] private TextMeshProUGUI specialTitleText;
    [SerializeField] private TextMeshProUGUI specialDescText;

    // Цены
    private int currentHealCost;
    private int currentHealAmount;
    private int currentDamageCost = 100;
    private float currentDamageAmount = 5f;

    private int currentTacticalCost = 150; // Цена тактики

    // Типы спец. улучшений
    private enum SpecialType { AbilityDuration, MoveSpeed, BulletSize, Piercing, CritChance, MaxHP }
    private SpecialType currentSpecialType;
    private int currentSpecialCost;
    private float currentSpecialValue;

    private PlayerController player;
    private PlayerHealth playerHealth;

    public void Initialize(PlayerController playerController)
    {
        player = playerController;
        playerHealth = player.GetComponent<PlayerHealth>();

        shopPanel.SetActive(false);

        healButton.onClick.RemoveAllListeners();
        damageButton.onClick.RemoveAllListeners();
        tacticalButton.onClick.RemoveAllListeners();
        specialButton.onClick.RemoveAllListeners();

        healButton.onClick.AddListener(BuyHeal);
        damageButton.onClick.AddListener(BuyDamage);
        tacticalButton.onClick.AddListener(BuyTactical); // Новая функция
        specialButton.onClick.AddListener(BuySpecial);
    }

    public void OpenShop()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.shopMusic);

        GenerateFixedOffers();
        GenerateRandomSpecialOffer();
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.battleMusic);
        shopPanel.SetActive(false);
    }

    void GenerateFixedOffers()
    {
        // Лечение
        currentHealAmount = Random.Range(30, 60);
        currentHealCost = Mathf.RoundToInt(currentHealAmount * 1.2f);
        healPriceText.text = $"{currentHealCost}$";
        healDescText.text = $"+{currentHealAmount} HP";

        // Урон
        damagePriceText.text = $"{currentDamageCost}$";
        damageDescText.text = $"+{currentDamageAmount} УРОН";

        // Тактика (Магазин + Перезарядка)
        tacticalPriceText.text = $"{currentTacticalCost}$";

        // ИСПРАВЛЕНИЕ ТЕКСТА:
        if (tacticalDescText)
            tacticalDescText.text = "-10% ПЕРЕЗАРЯДКА";
    }

    void GenerateRandomSpecialOffer()
    {
        int rnd = Random.Range(0, 6); // Теперь 6 вариантов

        switch (rnd)
        {
            case 0: // Длительность ульты
                currentSpecialType = SpecialType.AbilityDuration;
                currentSpecialValue = 1.3f; // +30%
                currentSpecialCost = 250;
                specialTitleText.text = "УЛЬТА+";
                specialDescText.text = "+30% ВРЕМЕНИ";
                break;

            case 1: // Крит Шанс (НОВОЕ)
                currentSpecialType = SpecialType.CritChance;
                currentSpecialValue = 0.1f; // +10% шанс
                currentSpecialCost = 350;
                specialTitleText.text = "КРИТИЧЕСКИЙ УДАР";
                specialDescText.text = "+10% ШАНС КРИТА";
                break;

            case 2: // Скорость бега
                currentSpecialType = SpecialType.MoveSpeed;
                currentSpecialValue = 1.0f;
                currentSpecialCost = 150;
                specialTitleText.text = "СКОРОСТЬ";
                specialDescText.text = "+1.0 К БЕГУ";
                break;

            case 3: // Размер пули
                currentSpecialType = SpecialType.BulletSize;
                currentSpecialValue = 0.2f;
                currentSpecialCost = 120;
                specialTitleText.text = "БОЛЬШИЕ ПУЛИ";
                specialDescText.text = "+20% РАЗМЕР";
                break;

            case 4: // Пробитие
                currentSpecialType = SpecialType.Piercing;
                currentSpecialValue = 1f;
                currentSpecialCost = 400;
                specialTitleText.text = "ПРОБИВАНИЕ";
                specialDescText.text = "+1 ВРАГ";
                break;

            case 5: // Макс ХП (НОВОЕ)
                currentSpecialType = SpecialType.MaxHP;
                currentSpecialValue = 50f;
                currentSpecialCost = 200;
                specialTitleText.text = "ТИТАН";
                specialDescText.text = "+50 МАКС. HP";
                break;
        }

        specialPriceText.text = $"{currentSpecialCost}$";
        specialButton.interactable = true;
    }

    // --- ПОКУПКИ ---

    void BuyHeal()
    {
        if (playerHealth.currentHealth >= playerHealth.maxHealth) return;
        if (GameManager.Instance.TrySpendMoney(currentHealCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItem);
            playerHealth.Heal(currentHealAmount);
            healButton.interactable = false;
        }
    }

    void BuyDamage()
    {
        if (GameManager.Instance.TrySpendMoney(currentDamageCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItem);
            player.UpgradeDamage(currentDamageAmount);
            currentDamageCost += 50;
            damagePriceText.text = $"{currentDamageCost}$";
        }
    }

    // Замена патронов на Тактику
    void BuyTactical()
    {
        if (GameManager.Instance.TrySpendMoney(currentTacticalCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItem);
            // +20% магазин, -10% времени перезарядки (умножаем на 0.9)
            player.UpgradeTactical(1.2f, 0.9f);

            currentTacticalCost += 50; // Цена растет
            tacticalPriceText.text = $"{currentTacticalCost}$";
        }
    }

    void BuySpecial()
    {
        if (GameManager.Instance.TrySpendMoney(currentSpecialCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buyItem);
            ApplySpecialUpgrade();
            specialButton.interactable = false;
        }
    }

    void ApplySpecialUpgrade()
    {
        switch (currentSpecialType)
        {
            case SpecialType.AbilityDuration:
                player.UpgradeAbilityDuration(currentSpecialValue);
                break;
            case SpecialType.CritChance:
                player.UpgradeCritChance(currentSpecialValue);
                break;
            case SpecialType.MoveSpeed:
                player.UpgradeMoveSpeed(currentSpecialValue);
                break;
            case SpecialType.BulletSize:
                player.UpgradeBulletSize(currentSpecialValue);
                break;
            case SpecialType.Piercing:
                player.UpgradePiercing((int)currentSpecialValue);
                break;
            case SpecialType.MaxHP:
                player.UpgradeMaxHP(currentSpecialValue);
                break;
        }
    }
}