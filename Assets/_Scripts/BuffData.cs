using UnityEngine;

public abstract class BuffData : ScriptableObject
{
    [Header("Thông tin chung")]
    public string id; // ID duy nhất (VD: "hit_or_miss", "well_prepared")
    public string buffName;
    public Sprite icon;

    // Hàm lấy mô tả thay đổi theo Level
    public abstract string GetDescription(int level);

    // --- NHÓM 1: TÍNH TOÁN CHỈ SỐ (Logic cũ) ---

    // Dùng cho buff "Trúng hay Hụt", "Võ sĩ hạng nặng"...
    public virtual int OnCalculateOutgoingDamage(int baseDamage, int myScore, bool isDouble, int level)
    {
        return baseDamage;
    }

    // Dùng cho buff "Biện pháp an toàn"...
    public virtual int OnCalculateIncomingDamage(int incomingDamage, int enemyScore, int level)
    {
        return incomingDamage;
    }

    // --- NHÓM 2: [MỚI] SỰ KIỆN GAME (Hooks) ---

    // Dùng cho buff "Chuẩn bị kĩ lưỡng" (Well Prepared)
    // Cần tham số GameManager để biết Level hiện tại là bao nhiêu và để gọi hàm Heal()
    public virtual void OnLevelStart(GameManager gm, int level)
    {
        // Mặc định không làm gì cả (để các buff khác không bị lỗi)
    }

    // Cần truyền GameManager vào để buff có thể gọi hiển thị thông báo (UpdateResultText)
    public virtual int ModifyPlayerDamage(int finalDamage, GameManager gm, int level)
    {
        return finalDamage; // Mặc định trả về damage gốc, không đổi
    }
}