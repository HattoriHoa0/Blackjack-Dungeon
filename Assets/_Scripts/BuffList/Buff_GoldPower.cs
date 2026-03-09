using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Gold Power (Damage Scaling)")]
public class Buff_GoldPower : BuffData
{
    [Header("% Damage tăng mỗi 50 Vàng (0.2 = 20%)")]
    public float[] percentPerStack = { 0.20f, 0.30f, 0.45f };

    [Header("Giới hạn vàng tối đa để tính damage")]
    public int maxGoldCap = 300; // [MỚI] Giới hạn 300 vàng

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, percentPerStack.Length - 1);
        int percent = Mathf.RoundToInt(percentPerStack[index] * 100);

        // Cập nhật mô tả để người chơi biết giới hạn
        return $"Tăng <color=red>{percent}%</color> Sát thương Với mỗi 50 Vàng sở hữu (tối đa {maxGoldCap} vàng)";
    }

    public override int ModifyPlayerDamage(int finalDamage, GameManager gm, int level)
    {
        // 1. Lấy số vàng hiện tại
        int currentGold = gm.player.currentGold;

        // 2. [THAY ĐỔI QUAN TRỌNG] Giới hạn vàng tính toán
        // Nếu vàng hiện tại > 300 thì chỉ lấy 300. Nếu thấp hơn thì lấy số thực tế.
        int effectiveGold = Mathf.Min(currentGold, maxGoldCap);

        // 3. Tính stack dựa trên số vàng ĐÃ GIỚI HẠN
        int stacks = effectiveGold / 50;

        if (stacks > 0)
        {
            int index = Mathf.Clamp(level - 1, 0, percentPerStack.Length - 1);
            float bonusPercent = stacks * percentPerStack[index];

            int bonusDamage = Mathf.RoundToInt(finalDamage * bonusPercent);

            // (Debug để kiểm tra)
            // Debug.Log($"Gold Power: {currentGold} Gold (Tính {effectiveGold}) -> {stacks} Stacks -> +{bonusDamage} DMG");

            return finalDamage + bonusDamage;
        }

        return finalDamage;
    }
}