using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Items/Ragebait")]
public class Item_Ragebait : ItemData
{
    public override bool OnUse(GameManager gm)
    {
        gm.MultiplyTempDamage(2f); // Nhân 2 damage
        return true;
    }
}