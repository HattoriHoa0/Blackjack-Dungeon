using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Card Synergy (Pair Bonus)")]
public class Buff_CardSynergy : BuffData
{
    [Header("Chỉ số % tăng thêm mỗi lá (0.35 = 35%)")]
    public float[] damagePercentPerCard = { 0.35f, 0.50f, 0.70f };

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, damagePercentPerCard.Length - 1);
        int percent = Mathf.RoundToInt(damagePercentPerCard[index] * 100);

        return $"Với mỗi lá bài có cặp/bộ trên tay, tăng <color=red>{percent}%</color> DMG.\n";
    }

    // Dùng Hook này vì nó cho phép truy cập GameManager để lấy danh sách bài
    public override int ModifyPlayerDamage(int finalDamage, GameManager gm, int level)
    {
        // 1. Lấy danh sách bài hiện tại (Cần đảm bảo biến playerHand trong GameManager là public)
        // Nếu playerHand đang private, bạn cần đổi nó thành public trong GameManager.cs
        List<CardData> hand = gm.playerHand;

        if (hand == null || hand.Count < 2) return finalDamage;

        // 2. Đếm tần suất xuất hiện của từng giá trị
        Dictionary<int, int> valueCounts = new Dictionary<int, int>();
        foreach (var card in hand)
        {
            int val = card.value;
            if (!valueCounts.ContainsKey(val)) valueCounts[val] = 0;
            valueCounts[val]++;
        }

        // 3. Tính tổng số lá bài tham gia vào các bộ (Pair, Triple...)
        int matchingCardsCount = 0;
        foreach (var pair in valueCounts)
        {
            // Chỉ tính nếu có từ 2 lá giống nhau trở lên
            if (pair.Value >= 2)
            {
                matchingCardsCount += pair.Value;
            }
        }

        // 4. Tính toán sát thương cộng thêm
        if (matchingCardsCount > 0)
        {
            int index = Mathf.Clamp(level - 1, 0, damagePercentPerCard.Length - 1);
            float percentPerCard = damagePercentPerCard[index];

            float totalBonusPercent = matchingCardsCount * percentPerCard;
            int bonusDamage = Mathf.RoundToInt(finalDamage * totalBonusPercent);

            // Hiển thị thông báo cho ngầu
            gm.ShowNotification($"CỘNG HƯỞNG: +{bonusDamage} DMG ({matchingCardsCount} LÁ)");

            return finalDamage + bonusDamage;
        }

        return finalDamage;
    }
}