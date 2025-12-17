using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Items/Time Clock")]
public class Item_TimeClock : ItemData
{
    public override bool OnUse(GameManager gm)
    {
        // Gọi hàm đổi bài đặc biệt trong GM
        gm.UseTimeClockItem();
        return true;
    }
}