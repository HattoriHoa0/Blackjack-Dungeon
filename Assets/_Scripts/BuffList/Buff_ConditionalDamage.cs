using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Conditional Damage")]
public class Buff_ConditionalDamage : BuffData
{
    public enum Condition { LessThan, GreaterThan }
    public Condition condition;
    public int threshold;

    // MẢNG CHỈ SỐ: Cấp 1, Cấp 2, Cấp 3
    public float[] damageMultipliers = { 0.5f, 0.7f, 1.0f };

    public override string GetDescription(int level)
    {
        // level truyền vào là 1, 2, 3 -> index là 0, 1, 2
        int index = Mathf.Clamp(level - 1, 0, damageMultipliers.Length - 1);
        float percent = damageMultipliers[index] * 100;

        string condStr = (condition == Condition.LessThan) ? "từ " + threshold + " trở xuống" : "từ " + threshold + " trở lên";
        return $"Khi thắng với điểm {condStr}, gây thêm <color=red>{percent}%</color> sát thương.";
    }

    public override int OnCalculateOutgoingDamage(int baseDamage, int myScore, bool isDouble, int level)
    {
        int index = Mathf.Clamp(level - 1, 0, damageMultipliers.Length - 1);
        bool triggered = false;

        if (condition == Condition.LessThan && myScore <= threshold) triggered = true;
        if (condition == Condition.GreaterThan && myScore >= threshold) triggered = true;

        if (triggered)
        {
            return baseDamage + Mathf.RoundToInt(baseDamage * damageMultipliers[index]);
        }
        return baseDamage;
    }
}