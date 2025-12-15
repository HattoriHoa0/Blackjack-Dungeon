using UnityEngine;

[CreateAssetMenu(fileName = "New Buff_WellPrepared", menuName = "Blackjack/Buffs/WellPrepared")]
public class Buff_WellPrepared : BuffData
{
    public float[] healPercentages = { 0.1f, 0.2f, 0.4f };

    // 1. Phải hiện thực hàm mô tả
    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, healPercentages.Length - 1);
        int percent = Mathf.RoundToInt(healPercentages[index] * 100);
        return $"Hồi <color=green>{percent}%</color> máu tối đa khi vào ải Elite (Level chia hết cho 3).";
    }

    // 2. Sửa lại hàm Override cho đúng với BuffData vừa sửa
    public override void OnLevelStart(GameManager gm, int level)
    {
        // Kiểm tra Level game chia hết cho 3
        if (gm.currentLevel % 3 == 0)
        {
            // Dùng tham số 'level' được truyền vào
            int tierIndex = Mathf.Clamp(level - 1, 0, healPercentages.Length - 1);
            float percent = healPercentages[tierIndex];

            // Sửa 'gm.playerStats' thành 'gm.player' (Theo code GameManager của bạn)
            // Giả sử CharacterBase có biến MaxHP và hàm Heal
            // Nếu CharacterBase của bạn dùng tên biến khác (VD: maxHealth), hãy sửa lại dòng dưới
            // Sửa MaxHP thành maxHP (theo file CharacterBase.cs của bạn)
            int healAmount = Mathf.RoundToInt(gm.player.maxHP * percent);

            // Gọi hàm Heal vừa tạo ở bước 2
            gm.player.Heal(healAmount);

            Debug.Log($"<color=green>Buff Chuẩn Bị: Hồi {healAmount} HP!</color>");
        }
    }
}