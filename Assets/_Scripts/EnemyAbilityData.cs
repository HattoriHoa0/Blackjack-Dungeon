using UnityEngine;

// Lớp cơ sở (Abstract) cho mọi kỹ năng quái
public abstract class EnemyAbilityData : ScriptableObject
{
    [TextArea] public string description;

    // Hook 1: Khi ván đấu bắt đầu (Dùng cho Silence Lock - Khóa nút)
    public virtual void OnRoundStart(GameManager gm) { }

    // Hook 2: Khi người chơi bấm Hit (Dùng cho Slippery Hands - Rút thêm)
    public virtual void OnPlayerHit(GameManager gm) { }

    // Hook 3: Khi lượt của Dealer (Dùng cho Silence Lock - Dealer không rút)
    // Trả về true nếu muốn Dealer dừng rút bài ngay lập tức
    public virtual bool OnDealerTurnStop(GameManager gm) { return false; }

    // Hook 4: Khi tính sát thương (Dùng cho Iron Defense - Giảm dmg)
    public virtual int OnCalculateDamage(int incomingDamage, int playerScore)
    {
        return incomingDamage; // Mặc định không đổi dmg
    }

    // Dùng cho Slime: Can thiệp vào sát thương quái nhận vào
    public virtual int OnModifyIncomingDamage(int rawDamage, GameManager gm)
    {
        return rawDamage;
    }

    // Dùng cho Slime: Xử lý khi máu về 0 (Trả về true nếu quái tự hồi sinh)
    public virtual bool OnTryRevive(GameManager gm)
    {
        return false;
    }

    // Dùng cho Nhện: Gọi khi quái đánh trúng người chơi
    public virtual void OnEnemyDealsDamage(GameManager gm)
    {
    }

    // Dùng cho Nhện: Gọi mỗi khi kết thúc lượt (để trừ máu độc)
    public virtual void OnTurnEnd(GameManager gm)
    {
    }

    // Dùng cho Skeleton: Cộng điểm ảo vào bài của quái
    public virtual int OnCalculateScoreBonus(int currentScore)
    {
        return 0;
    }

    // Dùng cho Vampire & Skeleton: Can thiệp sát thương quái GÂY RA
    public virtual int OnModifyOutgoingDamage(int finalDamage, GameManager gm)
    {
        return finalDamage;
    }
}