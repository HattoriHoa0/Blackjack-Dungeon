using UnityEngine;

[CreateAssetMenu(fileName = "New Buff_HitOrMiss", menuName = "Blackjack/Buff/HitOrMiss")]
public class Buff_HitOrMiss : BuffData
{
    public float[] successRates = { 0.45f, 0.50f, 0.55f };
    public float[] dmgBonus = { 0.50f, 0.60f, 0.70f };

    public float[] failRates = { 0.20f, 0.15f, 0.10f };
    public float[] dmgPenalty = { 0.50f, 0.40f, 0.30f };

    // 1. Phải hiện thực hàm mô tả
    public override string GetDescription(int level)
    {
        int i = Mathf.Clamp(level - 1, 0, successRates.Length - 1);
        return $"<color=green>{successRates[i] * 100}%</color> tỉ lệ tăng <color=green>{dmgBonus[i] * 100}%</color> dmg.\n" +
               $"Nếu trượt, <color=red>{failRates[i] * 100}%</color> tỉ lệ giảm <color=red>{dmgPenalty[i] * 100}%</color> dmg.";
    }

    // 2. Sửa lại hàm Override cho đúng tham số
    public override int ModifyPlayerDamage(int finalDamage, GameManager gm, int level)
    {
        int tierIndex = Mathf.Clamp(level - 1, 0, successRates.Length - 1);

        // Check Tăng Dmg
        if (Random.value < successRates[tierIndex])
        {
            int bonus = Mathf.RoundToInt(finalDamage * dmgBonus[tierIndex]);
            // Gọi hàm UpdateResultText của GameManager để báo cho người chơi biết
            gm.UpdateResultText($"MAY MẮN! +{bonus} DMG");
            return finalDamage + bonus;
        }
        // Check Giảm Dmg
        else if (Random.value < failRates[tierIndex])
        {
            int penalty = Mathf.RoundToInt(finalDamage * dmgPenalty[tierIndex]);
            gm.UpdateResultText($"XUI XẺO... -{penalty} DMG");
            return finalDamage - penalty;
        }

        return finalDamage;
    }
}