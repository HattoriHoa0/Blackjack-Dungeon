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
}