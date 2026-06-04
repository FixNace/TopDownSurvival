using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Config")]
public partial class LevelConfigSO : ScriptableObject
{
    [Header("Настройки уровня")]
    public string levelName; // Например "Уровень 1 - Легкий"
    public int sceneBuildIndex = 1;
    [Header("Волны")]
    public List<WaveSettings> waves;
}

[System.Serializable]
public class WaveSettings
{
    [Tooltip("Является ли эта волна волной босса?")]
    public bool isBossWave = false; // <-- Перенесли из старого WaveData

    [Tooltip("Сколько времени ждать перед началом этой волны (или после предыдущей)")]
    public float delayBeforeWave = 2f;

    [Tooltip("Список групп врагов для этой волны")]
    public List<EnemyGroup> enemyGroups;
}

[System.Serializable]
public class EnemyGroup
{
    public EnemyTypeSO enemyType; // Ссылка на наш ассет врага (Танк, Быстрый и т.д.)
    public int count; // Сколько таких врагов заспавнить
}