using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Slime")]
public class Ability_Slime : EnemyAbilityData
{
    public float damageReduction = 0.15f; // Giảm 15%

    // 1. Giảm sát thương nhận vào
    public override int OnModifyIncomingDamage(int rawDamage, GameManager gm)
    {
        // Tính lượng giảm
        int reducedAmount = Mathf.RoundToInt(rawDamage * damageReduction);
        int finalDamage = rawDamage - reducedAmount;

        if (reducedAmount > 0)
        {
            gm.ShowNotification($"SLIME NHẦY NHỤA! GIẢM {reducedAmount} ST");
        }

        return finalDamage;
    }

    // 2. Cơ chế Hồi sinh
    public override bool OnTryRevive(GameManager gm)
    {
        // Kiểm tra xem đã hồi sinh lần nào chưa (biến này nằm trong GameManager)
        if (!gm.hasSlimeRevived)
        {
            gm.hasSlimeRevived = true; // Đánh dấu đã dùng

            // Hồi đầy máu
            gm.enemy.Heal(gm.enemy.maxHP);

            gm.ShowNotification("<color=blue>SLIME PHÂN BÀO!</color>");

            // Trả về true để báo cho GameManager biết là "Nó chưa chết đâu, đừng End Game"
            return true;
        }

        return false; // Đã hồi sinh rồi thì cho chết luôn
    }
}