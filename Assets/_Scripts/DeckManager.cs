using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Dữ liệu Bài")]
    public List<CardData> deckPattern; // Kéo thả các lá bài gốc (ScriptableObject) vào đây

    private List<CardData> drawPile = new List<CardData>(); // Bộ bài đang dùng để rút

    // Hàm chuẩn bị bộ bài mới (Xóa cũ, nạp mới, xào lên)
    public void InitializeDeck()
    {
        drawPile.Clear();
        drawPile.AddRange(deckPattern);
        ShuffleDeck();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            CardData temp = drawPile[i];
            int r = Random.Range(i, drawPile.Count);
            drawPile[i] = drawPile[r];
            drawPile[r] = temp;
        }
        Debug.Log("Đã xào bài xong!");
    }

    // Hàm trả về 1 lá bài (Chỉ trả về Data, không quan tâm UI)
    public CardData GetNextCard()
    {
        if (drawPile.Count <= 0) return null;

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }
}