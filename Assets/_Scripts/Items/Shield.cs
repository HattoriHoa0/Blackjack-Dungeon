using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Items/Shield")]
public class Item_Shield : ItemData
{
    public int blockAmount = 200;

    public override bool OnUse(GameManager gm)
    {
        gm.AddTempBlock(blockAmount);
        return true;
    }
}