using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string enemyName;
    public Sprite portrait;
    public int baseHP = 100;

    [Header("Kỹ năng Logic")]
    // Thay thế Enum bằng biến này
    public EnemyAbilityData abilityLogic;
}