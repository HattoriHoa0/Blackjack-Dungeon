using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Silence Lock")]
public class Ability_SilenceLock : EnemyAbilityData
{
    // --- 1. LOGIC CŨ: KHÓA BÀI ---
    public override void OnRoundStart(GameManager gm)
    {
        // Khóa nút Hit
        if (gm.hitButton) gm.hitButton.interactable = false;

        // Mở nút Double nếu đủ máu
        // (Sử dụng biến currentBet trong GameManager)
        if (gm.doubleButton)
        {
            // Kiểm tra xem người chơi có đủ máu để Double không
            bool canDouble = gm.player.CurrentHP >= gm.currentBet * 2;
            gm.doubleButton.interactable = canDouble;
        }

        // Thông báo
        if (gm.resultText) gm.resultText.text = "CẤM RÚT BÀI!";
        gm.ShowNotification("CẤM HIT");
    }

    public override bool OnDealerTurnStop(GameManager gm)
    {
        return true; // Dealer cũng không được rút thêm
    }

    // --- 2. LOGIC MỚI: CỘNG ĐIỂM THÔNG MINH ---
    public override int OnCalculateScoreBonus(int currentScore)
    {
        int bonus = 3;

        // Nếu cộng 3 mà làm điểm số vượt quá 21 (Bị Quắc)
        if (currentScore + bonus > 21)
        {
            // Trường hợp 1: Bài gốc đã >= 21 rồi (Blackjack hoặc đã Quắc sẵn)
            // -> Không làm gì cả
            if (currentScore >= 21) return 0;

            // Trường hợp 2: Bài gốc đang dưới 21 (VD: 19, 20)
            // -> Chỉ cộng phần bù để vừa đủ 21 điểm
            // Ví dụ: 19 + 3 = 22 (Quắc) -> Code sẽ đổi thành 19 + 2 = 21.
            return 21 - currentScore;
        }

        // Nếu cộng 3 vẫn an toàn (<= 21) thì cộng full
        return bonus;
    }
}