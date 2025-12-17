using UnityEngine;

[CreateAssetMenu(fileName = "New Buff_HitOrMiss", menuName = "Blackjack/Buffs/HitOrMiss")]
public class Buff_HitOrMiss : BuffData
{
    [Header("Cấu hình Tỉ lệ")]
    // Tỉ lệ thành công (0.45 = 45%)
    public float[] successRates = { 0.45f, 0.50f, 0.55f };
    // Lượng dmg tăng thêm (0.50 = 50%)
    public float[] dmgBonus = { 0.50f, 0.60f, 0.70f };

    [Header("Cấu hình Rủi ro")]
    // Tỉ lệ thất bại (nếu trượt thành công)
    public float[] failRates = { 0.20f, 0.15f, 0.10f };
    // Lượng dmg bị trừ
    public float[] dmgPenalty = { 0.50f, 0.40f, 0.30f };

    // 1. Hiện thực hàm mô tả
    public override string GetDescription(int level)
    {
        int i = Mathf.Clamp(level - 1, 0, successRates.Length - 1);

        // Format string: P0 = phần trăm không số lẻ (50%), P1 = 1 số lẻ (50.5%)
        return $"Có <color=red>{successRates[i] * 100}%</color> tỉ lệ tăng <color=red>{dmgBonus[i] * 100}%</color> dmg khi thắng.\n" +
               $"Nếu không, <color=grey>{failRates[i] * 100}%</color> tỉ lệ giảm <color=grey>{dmgPenalty[i] * 100}%</color> dmg.";
    }

    // 2. Logic xử lý Damage
    public override int ModifyPlayerDamage(int finalDamage, GameManager gm, int level)
    {
        int tierIndex = Mathf.Clamp(level - 1, 0, successRates.Length - 1);

        // --- Check 1: MAY MẮN (Tăng Damage) ---
        if (Random.value < successRates[tierIndex])
        {
            int bonus = Mathf.RoundToInt(finalDamage * dmgBonus[tierIndex]);

            // Gọi thông báo lên màn hình
            if (gm != null) gm.UpdateResultText($"MAY MẮN! +{bonus} DMG");

            return finalDamage + bonus;
        }

        // --- Check 2: XUI XẺO (Giảm Damage) ---
        // Chỉ chạy check này nếu check 1 bị trượt (else if)
        else if (Random.value < failRates[tierIndex])
        {
            int penalty = Mathf.RoundToInt(finalDamage * dmgPenalty[tierIndex]);

            if (gm != null) gm.UpdateResultText($"XUI XẺO... -{penalty} DMG");

            return finalDamage - penalty;
        }

        // Nếu không trúng cái nào thì giữ nguyên damage
        return finalDamage;
    }
}