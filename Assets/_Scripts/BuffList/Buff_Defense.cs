using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Defense")]
public class Buff_Defense : BuffData
{
    public int threshold = 20;
    // Tỉ lệ nhận sát thương: 0.75, 0.65, 0.5
    public float[] receiveMultipliers = { 0.75f, 0.65f, 0.50f };

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, receiveMultipliers.Length - 1);
        float percent = receiveMultipliers[index] * 100;
        return $"Khi địch thắng với điểm >= {threshold}, bạn chỉ nhận <color=green>{percent}%</color> sát thương.";
    }

    public override int OnCalculateIncomingDamage(int incomingDamage, int enemyScore, int level)
    {
        if (enemyScore >= threshold)
        {
            int index = Mathf.Clamp(level - 1, 0, receiveMultipliers.Length - 1);
            return Mathf.RoundToInt(incomingDamage * receiveMultipliers[index]);
        }
        return incomingDamage;
    }
}