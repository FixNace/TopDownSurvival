using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Визуал и Префаб")]
    public string charName;
    public GameObject characterPrefab; // <-- ТЕПЕРЬ СПАВНИМ ЭТО
    // public Sprite bodySprite; // Больше не нужно, так как у нас целый префаб

    [Header("Характеристики")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;

    [Header("Пассивки (Старые)")]
    public bool isVampire = false;
    public float vampirismPercent = 0.1f;
    public bool isTank = false;
    public float blockChance = 0.25f;

    [Header("Активная Способность (Новое)")]
    public GameObject abilityPrefab; // Префаб круга (щита или зоны)
    public float abilityDuration = 10f;
    public float abilityCooldown = 20f;
    public AbilityType abilityType;
}

public enum AbilityType
{
    TankShield,
    ReloadBuff,
    VampireAura
}