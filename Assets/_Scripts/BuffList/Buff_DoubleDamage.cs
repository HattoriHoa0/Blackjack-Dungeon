using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Double Damage")]
public class Buff_DoubleDamage : BuffData
{
    // Cấp 1: 100%, Cấp 2: 130%, Cấp 3: 180%
    public float[] bonusMultipliers = { 1.0f, 1.3f, 1.8f };

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, bonusMultipliers.Length - 1);
        float percent = bonusMultipliers[index] * 100;
        return $"Khi thắng bằng Double, gây thêm <color=red>{percent}%</color> lượng cược ban đầu.";
    }

    public override int OnCalculateOutgoingDamage(int baseDamage, int myScore, bool isDouble, int level)
    {
        if (isDouble)
        {
            int index = Mathf.Clamp(level - 1, 0, bonusMultipliers.Length - 1);
            // baseDamage lúc này là (Cược x 2). Cược gốc là baseDamage / 2.
            int originalBet = baseDamage / 2;
            int bonus = Mathf.RoundToInt(originalBet * bonusMultipliers[index]);
            return baseDamage + bonus;
        }
        return baseDamage;
    }
}