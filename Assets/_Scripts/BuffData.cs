using UnityEngine;

public abstract class BuffData : ScriptableObject
{
    [Header("Thông tin chung")]
    public string id; // ID duy nhất để nhận biết buff trùng nhau (VD: "low_roll")
    public string buffName;
    public Sprite icon;

    // Hàm lấy mô tả thay đổi theo Level (VD: Level 1 hiện 50%, Level 2 hiện 70%)
    public abstract string GetDescription(int level);

    // Các hàm tính toán giờ đây sẽ nhận thêm tham số "level"
    public virtual int OnCalculateOutgoingDamage(int baseDamage, int myScore, bool isDouble, int level)
    {
        return baseDamage;
    }

    public virtual int OnCalculateIncomingDamage(int incomingDamage, int enemyScore, int level)
    {
        return incomingDamage;
    }
}