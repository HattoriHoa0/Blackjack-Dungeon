using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Skeleton Knight")]
public class Ability_Skeleton : EnemyAbilityData
{
    // 1. Luôn cộng thêm 1 điểm
    public override int OnCalculateScoreBonus(int currentScore)
    {
        return 1;
    }

    // 2. Nếu điểm >= 17 thì +50% DMG
    public override int OnModifyOutgoingDamage(int finalDamage, GameManager gm)
    {
        // Tính lại điểm hiện tại của quái (Hàm CalculateScore sẽ tự cộng bonus +1 ở bước sau)
        int score = gm.CalculateScore(gm.dealerHand);

        if (score >= 17)
        {
            int bonus = Mathf.RoundToInt(finalDamage * 0.5f);
            gm.ShowNotification($"CÚ ĐÁNH TỬ THẦN! (+{bonus} DMG)");
            return finalDamage + bonus;
        }

        return finalDamage;
    }
}