using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Elite Hunter (Gold Bonus)")]
public class Buff_EliteHunter : BuffData
{
    [Header("Vàng nhận thêm (Level 1, 2, 3)")]
    public int[] bonusGold = { 10, 15, 25 };

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, bonusGold.Length - 1);
        return $"Khi chiến thắng kẻ địch <color=red>ELITE</color>, nhận thêm <color=yellow>{bonusGold[index]} Vàng</color>.";
    }

    public override void OnEnemyKilled(GameManager gm, EnemyData enemyData, int level)
    {
        // 1. Kiểm tra xem quái vừa chết có phải là Elite không
        // (Dựa vào danh sách Elite trong GameManager)
        if (gm.eliteEnemiesList.Contains(enemyData))
        {
            int index = Mathf.Clamp(level - 1, 0, bonusGold.Length - 1);
            int amount = bonusGold[index];

            // 2. Thưởng tiền
            gm.player.AddGold(amount);

            // 3. Thông báo
            gm.ShowNotification($"SĂN ELITE THÀNH CÔNG! (+{amount}G)");
        }
    }
}