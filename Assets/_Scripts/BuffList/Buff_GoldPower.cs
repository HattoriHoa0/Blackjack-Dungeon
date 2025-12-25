using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Gold Power (Damage Scaling)")]
public class Buff_GoldPower : BuffData
{
    [Header("% Damage tăng mỗi 50 Vàng (0.2 = 20%)")]
    public float[] percentPerStack = { 0.20f, 0.30f, 0.45f };

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, percentPerStack.Length - 1);
        int percent = Mathf.RoundToInt(percentPerStack[index] * 100);
        return $"Với mỗi <color=yellow>50 Vàng</color> đang sở hữu, tăng <color=red>{percent}%</color> Sát thương.";
    }

    public override int ModifyPlayerDamage(int finalDamage, GameManager gm, int level)
    {
        // 1. Lấy số vàng hiện tại
        int currentGold = gm.player.currentGold;

        // 2. Tính số "Stack" (mỗi 50 vàng là 1 stack)
        int stacks = currentGold / 50;

        if (stacks > 0)
        {
            // 3. Tính lượng tăng thêm
            int index = Mathf.Clamp(level - 1, 0, percentPerStack.Length - 1);
            float bonusPercent = stacks * percentPerStack[index];

            int bonusDamage = Mathf.RoundToInt(finalDamage * bonusPercent);

            // (Tùy chọn) Hiện thông báo nếu bonus to
            // gm.ShowNotification($"TIỀN ĐÈ CHẾT NGƯỜI (+{bonusDamage} DMG)");

            return finalDamage + bonusDamage;
        }

        return finalDamage;
    }
}