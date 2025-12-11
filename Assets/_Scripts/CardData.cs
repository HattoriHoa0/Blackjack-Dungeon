using UnityEngine;

// Dòng này giúp bạn tạo lá bài bằng chuột phải trong Unity
[CreateAssetMenu(fileName = "New Card", menuName = "Blackjack/Card")]
public class CardData : ScriptableObject
{
    public string cardName; // Ví dụ: "Ace of Spades"
    public int value;       // Giá trị: 1 đến 11 (J,Q,K là 10, A là 11)
    public Sprite cardImage; // Hình ảnh hiển thị
}