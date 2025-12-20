using UnityEngine;

// Enum để định danh loại Nội tại (tránh dùng chuỗi string dễ sai sót)
public enum HeroPassiveType
{
    None,
    HealOnLevel,      // Gernas: Hồi máu sau mỗi màn
    ScorePlusOne,     // Kbruh: +1 Điểm bài (Risk/Reward)
    DiscountSpyglass  // Pain: Giảm giá Kính soi
}

[CreateAssetMenu(fileName = "New Hero", menuName = "Blackjack/Hero Data")]
public class HeroData : ScriptableObject
{
    [Header("Thông tin hiển thị")]
    public string heroID;           // ID định danh (vd: gernas, kbruh)
    public string heroName;         // Tên hiển thị
    [TextArea] public string description; // Mô tả nội tại
    public Sprite portrait;         // Ảnh đại diện

    [Header("Chỉ số cơ bản")]
    public int baseHP;              // Máu gốc (1000 hoặc 1200)

    [Header("Kỹ năng & Buff")]
    public HeroPassiveType passiveType; // Loại nội tại
    public BuffData startingBuff;       // Buff khởi đầu (Kéo file BuffData vào đây)
}