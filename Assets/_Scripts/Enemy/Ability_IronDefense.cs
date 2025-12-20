using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Iron Defense")]
public class Ability_IronDefense : EnemyAbilityData
{
    public override int OnCalculateDamage(int incomingDamage, int playerScore)
    {
        if (playerScore <= 17)
        {
            Debug.Log("Kích hoạt Giáp Vỏ Sắt!");
            return Mathf.RoundToInt(incomingDamage * 0.5f); // Giảm 50%
        }
        return incomingDamage;
    }
}