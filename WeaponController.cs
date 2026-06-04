using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    public WeaponDataSO data;

    [Header("Точка выстрела")]
    public Transform firePoint;

    // Патроны
    public int currentMagAmmo;
    public int currentReserveAmmo;

    // Финальные параметры (с учетом апгрейдов)
    private int finalMagazineSize;
    private float finalReloadTime;

    private float nextAttackTime;
    private bool isReloading = false;

    private PlayerController owner;

    // Инициализация (принимает множители от игрока)
    public void Initialize(PlayerController player, WeaponDataSO weaponData, int magMult, float reloadMult)
    {
        owner = player;
        data = weaponData;

        // Применяем улучшения:
        // Если magMult = 2, магазин будет двойным
        finalMagazineSize = data.magazineSize * magMult;
        // Если reloadMult = 1.3, перезарядка будет на 30% дольше
        finalReloadTime = data.reloadTime * reloadMult;

        // Заполняем магазин при старте
        currentMagAmmo = finalMagazineSize;
        currentReserveAmmo = data.maxAmmo;

        // Обновляем UI сразу
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateAmmoUI(currentMagAmmo, currentReserveAmmo);
    }

    public void TryAttack()
    {
        if (isReloading) return;
        if (Time.time < nextAttackTime) return;

        if (data.isMelee)
        {
            // Логика ближнего боя (пока отключена)
            // MeleeAttack();
        }
        else
        {
            if (currentMagAmmo > 0)
            {
                Shoot();
            }
            else
            {
                StartReload();
            }
        }
    }

    void Shoot()
    {
        nextAttackTime = Time.time + data.fireRate;
        currentMagAmmo--;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateAmmoUI(currentMagAmmo, currentReserveAmmo);
        for (int i = 0; i < data.pelletsAmount; i++)
        {
            float spread = Random.Range(-data.spreadAngle, data.spreadAngle);
            Quaternion bulletRot = firePoint.rotation * Quaternion.Euler(0, 0, spread);

            GameObject bulletObj = Instantiate(data.bulletPrefab, firePoint.position, bulletRot);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null && owner != null)
            {
                
                owner.ConfigureBullet(bulletScript);

            }
        }

        // Звуки
        if (AudioManager.Instance != null)
        {
            if (data.weaponName == "Shotgun")
                AudioManager.Instance.PlayCombatSFX(AudioManager.Instance.shootShotgun);
            else
                AudioManager.Instance.PlayCombatSFX(AudioManager.Instance.shootPistol);
        }
    }

    public void StartReload()
    {
        // Нельзя перезаряжаться, если уже полный или если нет запаса
        if (isReloading || currentMagAmmo == finalMagazineSize || data.isMelee) return;
        if (currentReserveAmmo <= 0) return;

        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCombatSFX(AudioManager.Instance.reload);

        // Ждем время (с учетом ухудшения перезарядки, если куплен тяжелый магазин)
        yield return new WaitForSeconds(finalReloadTime);

        // Сколько патронов не хватает до полного
        int ammoNeeded = finalMagazineSize - currentMagAmmo;

        // Сколько реально можем взять из запаса
        int ammoToTake = Mathf.Min(ammoNeeded, currentReserveAmmo);

        currentMagAmmo += ammoToTake;
        currentReserveAmmo -= ammoToTake;

        isReloading = false;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateAmmoUI(currentMagAmmo, currentReserveAmmo);
    }

    // Метод для покупки патронов в магазине
    public void RefillFull()
    {
        currentReserveAmmo = data.maxAmmo;
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateAmmoUI(currentMagAmmo, currentReserveAmmo);
    }
}