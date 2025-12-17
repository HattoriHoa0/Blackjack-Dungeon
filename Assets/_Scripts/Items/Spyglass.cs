using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Items/Spyglass")]
public class Item_Spyglass : ItemData
{
    public override bool OnUse(GameManager gm)
    {
        // Logic hiển thị lá bài tiếp theo
        // Ta cần GameManager trả về lá bài top deck để hiển thị UI
        CardData nextCard = gm.PeekNextCard();
        if (nextCard != null)
        {
            Debug.Log($"Soi thấy bài: {nextCard.cardName}");
            // TODO: Hiển thị UI popup hình lá bài ở đây
            gm.ShowSpyglassUI(nextCard);
            return true;
        }
        return false;
    }
}