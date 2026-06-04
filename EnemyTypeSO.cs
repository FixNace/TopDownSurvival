using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy Type")]
public class EnemyTypeSO : ScriptableObject
{
    public string enemyName;
    public GameObject prefab; // Префаб врага
    public float speed;
    public float health;
    public Color color = Color.white; // Для подкраски спрайта
}