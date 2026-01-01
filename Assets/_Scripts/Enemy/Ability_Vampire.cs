using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Vampire")]
public class Ability_Vampire : EnemyAbilityData
{
    // Hút máu: Gây 50% dmg, Hồi 50% dmg
    public override int OnModifyOutgoingDamage(int finalDamage, GameManager gm)
    {
        // 1. Giảm một nửa sát thương gây ra
        int reducedDmg = finalDamage / 2;

        // 2. Hồi máu cho bản thân bằng lượng đó
        if (reducedDmg > 0)
        {
            gm.enemy.Heal(reducedDmg);
            gm.ShowNotification($"HÚT MÁU! (+{reducedDmg} HP)");
        }

        return reducedDmg;
    }
}