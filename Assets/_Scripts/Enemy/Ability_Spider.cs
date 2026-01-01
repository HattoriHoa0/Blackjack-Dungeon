using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Poison Spider")]
public class Ability_Spider : EnemyAbilityData
{
    public int poisonDmgPerStack = 25;
    public int maxStacks = 4;

    // 1. Khi đánh trúng -> Tích độc
    public override void OnEnemyDealsDamage(GameManager gm)
    {
        if (gm.poisonStacks < maxStacks)
        {
            gm.poisonStacks++;
            gm.ShowNotification($"TRÚNG ĐỘC! (STACK {gm.poisonStacks}/{maxStacks})");
        }
    }

    // 2. Cuối lượt -> Trừ máu độc
    public override void OnTurnEnd(GameManager gm)
    {
        if (gm.poisonStacks > 0)
        {
            int dmg = gm.poisonStacks * poisonDmgPerStack;
            gm.player.TakeDamage(dmg);
            gm.ShowNotification($"<color=purple>ĐỘC RÚT MÁU: -{dmg} HP</color>");
        }
    }
}