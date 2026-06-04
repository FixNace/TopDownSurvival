using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    // ... (Твои UI переменные оставь без изменений)
    [HideInInspector] public Joystick joystick;
    [HideInInspector] public Button shootButton;
    [HideInInspector] public Button abilityButton;
    [HideInInspector] public Button switchWeaponButton;
    [HideInInspector] public Button reloadButton;

    [Header("Настройки Оружия")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private List<WeaponDataSO> inventory;

    public CharacterDataSO currentCharacter;
    private WeaponDataSO currentWeapon;
    private WeaponController currentWeaponInstance;
    private int currentWeaponIndex = 0;

    // Статы
    private float currentDamage;
    private float currentSpeed = 5f;
    private float bulletSize = 0.3f;
    private int piercingCount = 0;

    // Новые статы
    private float critChance = 0f; // Шанс крита (0.1 = 10%)
    private float critMultiplier = 2.0f; // Множитель крита (x2)

    // Множители
    private float abilityDurationMultiplier = 1f;
    private float reloadSpeedMultiplier = 1f;
    private int magSizeMultiplier = 1;

    private bool hasReloadBuff = false;
    private bool hasVampireBuff = false;
    private bool hasDamageBuff = false;

    private float nextAbilityTime;
    private Rigidbody2D rb;
    private PlayerHealth playerHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    public void SetupControls(Joystick joy, Button shootBtn, Button abilBtn, Button switchBtn, Button reloadBtn)
    {
        // ... (Код без изменений, скопируй из старого или оставь как есть)
        joystick = joy; shootButton = shootBtn; abilityButton = abilBtn; reloadButton = reloadBtn; switchWeaponButton = switchBtn;
        if (shootButton) { shootButton.onClick.RemoveAllListeners(); shootButton.onClick.AddListener(AttackCommand); }
        if (abilityButton) { abilityButton.onClick.RemoveAllListeners(); abilityButton.onClick.AddListener(CastAbility); }
        if (reloadButton) { reloadButton.onClick.RemoveAllListeners(); reloadButton.onClick.AddListener(OnReloadPress); }
        if (switchWeaponButton) { switchWeaponButton.onClick.RemoveAllListeners(); switchWeaponButton.onClick.AddListener(SwapWeapon); }
    }

    public void InitializeCharacter(CharacterDataSO data)
    {
        if (data == null) return;
        currentCharacter = data;
        currentSpeed = data.moveSpeed;
        if (playerHealth != null) playerHealth.InitializeHealth(data);
        if (inventory != null && inventory.Count > 0) EquipWeapon(0);
    }

    void Update()
    {
        if (joystick == null) return;
        Vector2 moveInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        rb.linearVelocity = moveInput * currentSpeed;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        if (abilityButton != null) abilityButton.interactable = Time.time >= nextAbilityTime;
    }

    public void AttackCommand() { if (currentWeaponInstance != null) currentWeaponInstance.TryAttack(); }
    public void OnReloadPress() { if (currentWeaponInstance != null) { currentWeaponInstance.StartReload(); AudioManager.Instance.PlaySFX(AudioManager.Instance.reload); } }

    public void CastAbility()
    {
        // ... (Код без изменений)
        if (currentCharacter == null || currentCharacter.abilityPrefab == null) return;
        if (Time.time < nextAbilityTime) return;
        float duration = currentCharacter.abilityDuration * abilityDurationMultiplier;
        nextAbilityTime = Time.time + currentCharacter.abilityCooldown;
        GameObject zoneObj = Instantiate(currentCharacter.abilityPrefab, transform.position, Quaternion.identity);
        zoneObj.transform.SetParent(this.transform);
        AbilityZone zoneScript = zoneObj.GetComponent<AbilityZone>();
        if (zoneScript != null) { zoneScript.Initialize(currentCharacter.abilityType, duration); AudioManager.Instance.PlayCombatSFX(AudioManager.Instance.abilityUse); }
    }

    public void ConfigureBullet(Bullet bullet)
    {
        bool isVampire = currentCharacter.isVampire || hasVampireBuff;
        float vampAmount = 0.1f;
        if (currentCharacter.isVampire && hasVampireBuff) vampAmount = 0.2f;

        float dmg = currentDamage;
        if (hasDamageBuff) dmg *= 1.5f;

        // РАСЧЕТ КРИТА
        if (Random.value < critChance)
        {
            dmg *= critMultiplier;
            bullet.transform.localScale *= 1.2f;
        }

        if (currentWeapon != null)
        {
            // ФИКС: Умножаем базовый размер пули игрока на множитель из настроек оружия
            float finalSize = bulletSize * currentWeapon.bulletSizeScale;

            bullet.Initialize(transform.right, currentWeapon.bulletSpeed, dmg, finalSize, piercingCount, isVampire, vampAmount, playerHealth);
        }
    }

    public void SwapWeapon()
    {
        if (inventory.Count < 2) return;
        currentWeaponIndex++;
        if (currentWeaponIndex >= inventory.Count) currentWeaponIndex = 0;
        EquipWeapon(currentWeaponIndex);
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= inventory.Count) return;
        if (currentWeaponInstance != null) Destroy(currentWeaponInstance.gameObject);

        currentWeapon = inventory[index];
        currentDamage = currentWeapon.damage;

        GameObject newWeaponObj = Instantiate(currentWeapon.weaponPrefab, weaponHolder.position, weaponHolder.rotation);
        newWeaponObj.transform.SetParent(weaponHolder);

        currentWeaponInstance = newWeaponObj.GetComponent<WeaponController>();
        if (currentWeaponInstance != null)
        {
            currentWeaponInstance.Initialize(this, currentWeapon, magSizeMultiplier, reloadSpeedMultiplier);
        }
        currentWeaponIndex = index;
    }

    public void RefillAmmo() { if (currentWeaponInstance != null) currentWeaponInstance.RefillFull(); }

    // --- АПГРЕЙДЫ ---
    public void SetDamageBuff(bool active) { hasDamageBuff = active; }
    public void SetVampireBuff(bool active) { hasVampireBuff = active; }
    public void UpgradeDamage(float amount) { currentDamage += amount; }
    public void UpgradeMoveSpeed(float amount) { currentSpeed += amount; }
    public void UpgradeBulletSize(float amount) { bulletSize += amount; }
    public void UpgradePiercing(int amount) { piercingCount += amount; }
    public void SetReloadBuff(bool active) { hasReloadBuff = active; }
    public void UpgradeAbilityDuration(float mult) { abilityDurationMultiplier *= mult; }

    // НОВОЕ: Прокачка Тактики (Магазин + Перезарядка)
    public void UpgradeTactical(float magMult, float reloadSpeedMult)
    {
        magSizeMultiplier = Mathf.RoundToInt(magSizeMultiplier * magMult); // Например +20%
        reloadSpeedMultiplier *= reloadSpeedMult; // Уменьшаем время (умножаем на 0.9)
        EquipWeapon(currentWeaponIndex); // Пересоздаем оружие с новыми статами
    }

    // НОВОЕ: Прокачка Крита
    public void UpgradeCritChance(float amount)
    {
        critChance += amount;
        if (critChance > 1f) critChance = 1f;
    }

    // НОВОЕ: Прокачка Макс ХП
    public void UpgradeMaxHP(float amount)
    {
        if (playerHealth != null)
        {
            playerHealth.maxHealth += amount;
            playerHealth.Heal(amount); // Сразу лечим на добавленное количество
        }
    }
}