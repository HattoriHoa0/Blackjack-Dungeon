using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Five Card Charlie")]
public class Buff_FiveCardCharlie : BuffData
{
    // Bonus damage theo cấp: 50%, 100%, 200%
    public float[] damageBonuses = { 0.5f, 1.0f, 2.0f };

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, damageBonuses.Length - 1);
        return $"Nếu có 5 lá bài mà chưa Quắc -> Thắng ngay và gây thêm <color=red>{damageBonuses[index] * 100}%</color> DMG.";
    }

    public override void OnPostPlayerHit(GameManager gm, int level)
    {
        // 1. Lấy dữ liệu bài hiện tại
        // Lưu ý: Cần đảm bảo GameManager có hàm CalculateScore public hoặc truy cập được
        int score = gm.CalculateScore(gm.playerHand);
        int cardCount = gm.playerHand.Count;

        // 2. Kiểm tra điều kiện: Chưa Quắc VÀ Đủ 5 lá
        if (score <= 21 && cardCount >= 5)
        {
            // 3. Tính bonus
            int index = Mathf.Clamp(level - 1, 0, damageBonuses.Length - 1);
            float bonus = damageBonuses[index];

            // 4. Gọi GameManager xử lý thắng
            // Cần thêm hàm MultiplyTempDamage trong GameManager như bạn đã làm ở các bước trước
            gm.MultiplyTempDamage(1 + bonus);
            gm.ShowNotification($"NGŨ LINH! THẮNG NGAY (x{1 + bonus} DMG)");

            // Ép thắng (truyền false để báo không bị bust)
            gm.ResolveCombat(false);
        }
    }
}