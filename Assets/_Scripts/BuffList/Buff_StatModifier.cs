using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Buffs/Stat Modifier")]
public class Buff_StatModifier : BuffData
{
    public enum StatType { MaxHP_Percent, Inventory_Slots }
    public StatType statType;

    [Header("Giá trị theo Level (1, 2, 3)")]
    public float[] values; // VD: 0.1, 0.2, 0.4 cho HP hoặc 1, 2, 3 cho Slots

    public override string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, values.Length - 1);
        float val = values[index];

        if (statType == StatType.MaxHP_Percent)
            return $"Tăng Max HP thêm <color=green>{val * 100}%</color>.";
        else
            return $"Mở rộng Balo thêm <color=yellow>{val}</color> ô.";
    }

    public override void ApplyStatModifiers(CharacterBase character, int level)
    {
        int index = Mathf.Clamp(level - 1, 0, values.Length - 1);
        float val = values[index];

        if (statType == StatType.MaxHP_Percent)
        {
            // Tăng Max HP theo % của Base HP
            int bonus = Mathf.RoundToInt(character.baseMaxHP * val);
            character.maxHP += bonus;

        }
        else if (statType == StatType.Inventory_Slots)
        {
            // Cộng thẳng vào số slot
            character.maxInventorySlots += (int)val;
        }
    }
}