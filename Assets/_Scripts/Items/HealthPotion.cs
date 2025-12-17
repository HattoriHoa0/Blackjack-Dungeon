using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Items/Health Potion")]
public class Item_HealthPotion : ItemData
{
    public int healAmount = 100;

    public override bool OnUse(GameManager gm)
    {
        // Chỉ dùng được nếu máu chưa đầy (tùy bạn chọn logic này hay không)
        if (gm.player.CurrentHP >= gm.player.maxHP) return false;

        gm.player.Heal(healAmount);
        return true; // Dùng thành công, trừ item khỏi túi
    }
}