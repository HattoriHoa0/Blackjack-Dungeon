using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Dữ liệu Bài")]
    public List<CardData> deckPattern; // Bộ bài gốc (Template)

    [Header("Trạng thái Runtime")]
    // Để public để nhìn trên Inspector cho dễ debug, sau này đổi về private
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    // Hàm khởi tạo ban đầu (Reset toàn bộ)
    public void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        drawPile.AddRange(deckPattern);
        ShuffleDeck(drawPile);
    }

    // Hàm xào bài (Dùng chung cho cả lúc đầu và lúc tái chế)
    void ShuffleDeck(List<CardData> pileToShuffle)
    {
        for (int i = 0; i < pileToShuffle.Count; i++)
        {
            CardData temp = pileToShuffle[i];
            int r = Random.Range(i, pileToShuffle.Count);
            pileToShuffle[i] = pileToShuffle[r];
            pileToShuffle[r] = temp;
        }
    }

    // Hàm đếm bài (GameManager cần cái này)
    public int GetCardsRemaining()
    {
        // Tổng số bài có thể dùng = bài đang úp + bài trong thùng rác
        return drawPile.Count + discardPile.Count;
    }

    // Hàm rút bài thông minh
    public CardData GetNextCard()
    {
        // Nếu chồng bài rút hết sạch -> Lấy thùng rác ra xào lại
        if (drawPile.Count <= 0)
        {
            if (discardPile.Count > 0)
            {
                // Đổ bài rác vào chồng rút
                drawPile.AddRange(discardPile);
                discardPile.Clear();

                // Xào lên
                ShuffleDeck(drawPile);
                Debug.Log($"<color=yellow>Đã xào lại {drawPile.Count} lá bài từ bài bỏ!</color>");
            }
            else
            {
                Debug.LogError("Hết sạch sành sanh bài rồi! (Draw=0, Discard=0)");
                return null;
            }
        }

        // Rút lá đầu tiên
        CardData card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }

    // Hàm ném bài vào thùng rác (Gọi khi hết ván)
    public void AddToDiscardPile(List<CardData> usedCards)
    {
        discardPile.AddRange(usedCards);
    }
}