using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    
    [Header("Основные параметры")]
    public string weaponName;
    public GameObject weaponPrefab; // Визуал в руках
    public GameObject bulletPrefab; // Чем стреляет

    [Header("Стрельба")]
    public float damage = 10f;
    public float fireRate = 0.5f; // Задержка между выстрелами
    public float bulletSpeed = 20f;

    [Header("Настройки пули")]
    public float bulletSizeScale = 1f; // <-- НОВОЕ: Размер пули (1 = стандарт, 0.5 = маленькая)

    [Header("Дробовик")]
    public int pelletsAmount = 1; // <-- НОВОЕ: Сколько пуль вылетает за 1 выстрел (для дробовика ставь 5-10)
    public float spreadAngle = 0f; // Разброс (для дробовика ставь 15-30)

    [Header("Магазин")]
    public int magazineSize = 30;
    public int maxAmmo = 120;
    public float reloadTime = 1.5f;

    public bool isMelee = false;
}